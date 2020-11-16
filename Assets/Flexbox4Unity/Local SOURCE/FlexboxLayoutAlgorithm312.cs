#define MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
//THIS DEFINE SLIGHTLY CHANGES LAYOUT BEHAVIOUR:
#define AUTOSIZED_CALC_IGNORE_CUSTOM_ATTACHED_ELEMENTS // Increases speed (approx 7-10% on small examples), but will IGNORE any custom ILayoutElements attached that are not written by Unity
#define AUTOSIZED_CALC_ONLY_SEARCH_DIRECT_CHILDREN_FOR_EMBEDDED_UIBEHAVIOURS_WHEN_DOING_AUTOCONTENT_SIZING // More correct, very slightly (<0.01%) reduces speed
//#define AUTOSIZED_CALC_BATCH_GETCOMPONENT_CALLS // sadly: this is actually slower than the more obvious direct code that calls each individually
#define AUTOSIZED_CALC_NONALLOC_GETCOMPONENT // Unity's GetComponent<> call is slow in-editor, compared to the non-allocating TryGetComponent<>
#define PROPRIETARY_ASPECT_FLEXBASIS // This is not in CSS-3, added specifically for Unity and game-developers

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Axis = UnityEngine.RectTransform.Axis;
using Object = System.Object;

#pragma warning disable 0642 
namespace Flexbox4Unity
{
 /**
  See private var "_featureList" for accurate list of changes
  */
 [CreateAssetMenu(menuName = "Flexbox/Layout Algorithms/v3.1.2", fileName = "Layout Algorithm v3.1.2.asset")]
 public class FlexboxLayoutAlgorithm312 : IFlexboxLayoutAlgorithmV3
 {
  public override string defaultAssetName
  {
   get { return "Layout Algorithm v3.1.2"; }
  }
  private static readonly ReadOnlyCollection<string> _featureList = new ReadOnlyCollection<string>(
   new List<string> 
   {
    "PERFORMANCE: up to 20x faster on text-heavy layouts",
    "ADDED: automatic-sizing support for TextMeshPro",
    "ADDED: high-performance samplers for the Unity profiler, making it easier to optimize your projects",
    "ADDED: integrated with the Flexbox automated performance-tests",
    "FIXED: clamped flexlines in containers with padding and/or margins were overflowing the parent bounds",
    "FIXED: responsive-design layouts will now work with ROW+WRAP embedded inside a Vertical Scrollview",
    "FIXED: grow/shrink was getting overwritten if some siblings were frozen (grow/shrink == 0)"
   }
  );
  public override ReadOnlyCollection<string> featureDescription
  {
   get { return _featureList; }
  }

  #region Performance - Unity CustomSamplers to make it easier to use the Unity Profiler
  public CustomSampler samplerMainAlgorithm = CustomSampler.Create( "flex.CoreLayout" );
  public CustomSampler samplerMainAlgorithmSizing = CustomSampler.Create( "flex.CoreLayout.Sizing" );
  public CustomSampler samplerMainAlgorithmPositioning = CustomSampler.Create( "flex.CoreLayout.Positioning" );
  public CustomSampler samplerContentSizeNonContainer = CustomSampler.Create( "flex.AUTO|CONTENT.size[FlexItem]" );
  public CustomSampler samplerContentSizeContainer = CustomSampler.Create( "flex.AUTO|CONTENT.size[FlexContainer]" );
  public CustomSampler samplerMainLengthBase = CustomSampler.Create( "flex.Main.baseSize" );
  public CustomSampler samplerCrossLengthBase = CustomSampler.Create( "flex.Cross.baseSize" );
  public CustomSampler samplerFlexLines = CustomSampler.Create( "flex.CalcFlexLines" );
  #endregion
  
  private struct FlexLayoutData
  {
   public FlexItem containerAsItem;
   public RectTransform containerTransform;
   
   public FlexContainer containersParent;
   public Vector2? containersParentSize;

   public Vector2 innerContentSize, outerContentSize;
   public float containersPaddingLeft, containersPaddingRight, containersPaddingTop, containersPaddingBottom;
   public Axis axisMain;

   public Axis axisCross
   {
    get { return (axisMain == Axis.Horizontal) ? Axis.Vertical : Axis.Horizontal; }
   }

   public List<FlexItem> childItems;
  }

  /**
   * Main layout method: decides FlexItem *SIZES then *POSITIONS*
   */
  public override void AlgorithmLayout(FlexContainer container, Vector2 availableSize)
  {
   samplerMainAlgorithm.Begin();
   try
   {
    FlexLayoutData d = FlexLayoutSetup( container, availableSize );

    if( container.showDebugMessages ) Debug.Log( "[" + container.name + "]: Starting LayoutWithinCurrentSize; containersParentSize=" + d.containersParentSize + ", outerContentSize=" + d.outerContentSize + ", innerContentSize=" + d.innerContentSize );
    //if( container.showDebugMessages ) Debug.Log("Starting self-layout; total active self-layout calls was: "+FlexboxElement.totalActiveLayoutSelfAndChildren);

    
     /**
      *
      * Phase 1:
      *
      * Calculate sizes of all children, and fit them into the available space, ready to be positioned
      * 
      * NOTE: the "finalSizes" here do NOT include margins; they are the flex INNER sizes, aka the box-sizes
      */
     List<List<FlexItem>> flexLines; // NOTE: this is calculated as a side-effect of calculating child sizes
     Dictionary<List<FlexItem>, float> usedMainSpacePerLine;
     Dictionary<List<FlexItem>, float> unusedMainSpacePerLine;
     Dictionary<List<FlexItem>, float> flexLineCrossHeights;
     Dictionary<FlexItem, Vector2> finalSizes;

     samplerMainAlgorithmSizing.Begin();
     try
     {
      finalSizes = CalculateAndSetAllChildSizes( d, container, out usedMainSpacePerLine, out unusedMainSpacePerLine, out flexLines, out flexLineCrossHeights );
      
     /*********** DEBUG AT END OF SIZING ROUTINES *************/
     if( container.showDebugMessages )
     {
      foreach( var child in d.childItems )
      {
       var childTransform = child.transform as RectTransform;
       Debug.Log( "[" + container.name + "]: Sizing complete, child (" + child.name + ") has size: " + childTransform.rect.size );
      }
     }
    }
    finally
    {
     samplerMainAlgorithmSizing.End();
    }

    /**
     *
     *
     * ------------- Sizing complete, now position ---------------------
     *
     *
     * 
     */

    samplerMainAlgorithmPositioning.Begin();
    try
    {
     float accumulatedPreviousLineCrossLengths = 0;
     for( int flexLineIndex = 0; flexLineIndex < flexLines.Count; flexLineIndex++ )
     {
      List<FlexItem> flexLine = flexLines[flexLineIndex];
      float containerUsedMainLengthThisLine = usedMainSpacePerLine[flexLine];

      /**
       *
       * ------ Step 3: Calculate advances needed to maintain alignment while positioning children ------
       *
       * 
       */
      float offsetAccumulatedFromPreviousItems = 0;
      float childFinalLength = 0;

      /** Pre-calculate the direction we're adding pixels in along main-axis (either: positive or negative (in Unity's UI space)) */
      float mainAxisAdvanceDirection;
      float crossAxisAdvanceDirection;
      float flipYAbsoluteMeasures; // ONLY used in absolute-position calcs, i.e. the justifiedStartMainAxis and justifiedStartCrossAxis
      switch( container.direction )
      {
       case FlexDirection.COLUMN_REVERSED: // note that Unity does Y+ = up, where flex does Y+ = down, so COLUMN and COLUMN_REVERSED are swapped here
       case FlexDirection.ROW:
        mainAxisAdvanceDirection = 1f;
        break;
       case FlexDirection.COLUMN:
       case FlexDirection.ROW_REVERSED:
        mainAxisAdvanceDirection = -1f;
        break;
       default:
        throw new Exception( "Impossible: j352sdflkj" );
      }

      switch( container.direction )
      {
       case FlexDirection.ROW:
       case FlexDirection.ROW_REVERSED:
        crossAxisAdvanceDirection = -1f; // because Unity does Y = up, and we do our cross-axis directions downwards
        break;
       case FlexDirection.COLUMN:
       case FlexDirection.COLUMN_REVERSED:
        crossAxisAdvanceDirection = 1f;
        break;
       default:
        throw new Exception( "Impossible: j352ssd324234kj" );
      }

      switch( container.direction )
      {
       case FlexDirection.ROW:
       case FlexDirection.ROW_REVERSED:
        flipYAbsoluteMeasures = 1f;
        break;
       case FlexDirection.COLUMN:
       case FlexDirection.COLUMN_REVERSED:
        flipYAbsoluteMeasures = -1f; // because Unity does Y = up, and we do our cross-axis directions downwards
        break;
       default:
        throw new Exception( "Impossible: j352ssd324234kj" );
      }

      /** ... main axis: calculate advance to support different "justify" settings */
      float preAdvance, postAdvance;
      CalculateAdvances( flexLine.Count, container, unusedMainSpacePerLine[flexLine], out preAdvance, out postAdvance );

      if( container.showDebugMessages ) Debug.LogFormat( "[" + container.name + "] Advances calc'd: preAdvance ={0}, postAdvance = {1}", preAdvance, postAdvance );

      /**
       *
       * ------ Step 4: Calculate start point ------
       *
       * 
       */

      /**
       * We're going to (selectively) shift positions by the padding amounts later
       * (different directions, different align-modes ... will need to SELECTIVELY ignore the padding,
       * this is a side-effect of some very painfully bad design of the CSS core layout algorithm
       * from many years ago that everyone is now stuck with)
       *
       * ... so this is the ONLY place where we layout relative to the parent's outer content size,
       *     i.e. the padded size.
       */
      float mainAxisContainerPaddedLength = d.axisMain.Length( d.outerContentSize );
      float crossAxisContainerPaddedLength = d.axisCross.Length( d.outerContentSize );
      float justifiedStartMainAxis = 0;
      switch( container.justifyContent )
      {
       case FlexJustify.SPACE_BETWEEN:
       case FlexJustify.SPACE_AROUND:
       case FlexJustify.SPACE_EVENLY:
        justifiedStartMainAxis = -1f * mainAxisContainerPaddedLength / 2f;
        if( (flipYAbsoluteMeasures * mainAxisAdvanceDirection) < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
         justifiedStartMainAxis += mainAxisContainerPaddedLength; // note this is CONTAINER length
        break;

       case FlexJustify.START:
        justifiedStartMainAxis = -1f * mainAxisContainerPaddedLength / 2f;
        if( (flipYAbsoluteMeasures * mainAxisAdvanceDirection) < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
         justifiedStartMainAxis += containerUsedMainLengthThisLine; // note this is CHILDREN USED length
        break;

       case FlexJustify.CENTER:
        justifiedStartMainAxis = 0 - containerUsedMainLengthThisLine / 2f;
        if( (flipYAbsoluteMeasures * mainAxisAdvanceDirection) < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
         justifiedStartMainAxis += containerUsedMainLengthThisLine; // note this is CHILDREN USED length
        break;

       case FlexJustify.END:
        justifiedStartMainAxis = mainAxisContainerPaddedLength / 2f - containerUsedMainLengthThisLine;
        if( (flipYAbsoluteMeasures * mainAxisAdvanceDirection) < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
         justifiedStartMainAxis += containerUsedMainLengthThisLine;
        break;
      }


      /**
       * Correction for the fact that Y is reversed in Unity GUI, and so the start point on main axis needs to have its
       * sign reversed.
       *
       * Note how this is only needed here, and in the if() checks in the switch above, and nowhere else,
       * because everything else is handled by our mainAxisAdvanceDirection multiplier
       */
      justifiedStartMainAxis *= flipYAbsoluteMeasures;

      if( container.showDebugMessages ) Debug.Log( "[" + container.name + "] Layout(" + container.justifyContent + ") -- justifiedStartMainAxis = " + justifiedStartMainAxis + " (flipped for Y? = " + (flipYAbsoluteMeasures < 0f) + "     (ContainerPaddedLength = " + mainAxisContainerPaddedLength + ", container USED length: " + containerUsedMainLengthThisLine + ")" );
      if( container.showDebugMessages ) Debug.Log( "[" + container.name + "] Layout(" + container.justifyContent + "). Advances: pre=" + preAdvance + ", post=" + postAdvance + " --  Unity first offset = " + justifiedStartMainAxis );

      float justifiedStartCrossAxis = 0;
      switch( container.alignItems )
      {
       case AlignItems.START:
       case AlignItems.STRETCH:
        justifiedStartCrossAxis = -1f * crossAxisContainerPaddedLength / 2f;
        if( crossAxisAdvanceDirection < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
         justifiedStartCrossAxis += crossAxisContainerPaddedLength;
        break;

       case AlignItems.CENTER:
        justifiedStartCrossAxis = 0f; //-1f * (crossAxisContainerPaddedLength) / 2f;
        break;

       case AlignItems.END:
        justifiedStartCrossAxis = flipYAbsoluteMeasures * -1f * crossAxisContainerPaddedLength / 2f;
        break;
      }

      /** 5c. initial offset from the container's own internal padding */

      float mainOffsetFromParentPadding;
      float crossOffsetFromParentPadding;
      if( d.containerAsItem == null )
      {
       mainOffsetFromParentPadding = 0;
       crossOffsetFromParentPadding = 0;
      }
      else
      {
       /**
        * The offset to the "justifiedStartMainAxis" varies a lot based on container-direction AND justify settings.
        * This segment merges all the special cases into a few lines of code.
        */
       float _mainAxisContainersStartPadding = (d.axisMain == Axis.Horizontal) ? d.containersPaddingLeft : d.containersPaddingTop;
       float _mainAxisContainersEndPadding = (d.axisMain == Axis.Horizontal) ? d.containersPaddingRight : d.containersPaddingBottom;
       float _crossAxisContainersStartPadding = (d.axisMain == Axis.Vertical) ? d.containersPaddingLeft : d.containersPaddingTop;
       float _crossAxisContainersEndPadding = (d.axisMain == Axis.Vertical) ? d.containersPaddingRight : d.containersPaddingBottom;
       switch( container.justifyContent ) // Main axis based on JUSTIFY
       {
        case FlexJustify.START:
         mainOffsetFromParentPadding = mainAxisAdvanceDirection * flipYAbsoluteMeasures * _mainAxisContainersStartPadding;
         break;
        case FlexJustify.END:
         mainOffsetFromParentPadding = mainAxisAdvanceDirection * flipYAbsoluteMeasures * -1f * _mainAxisContainersEndPadding;
         break;
        case FlexJustify.CENTER:
         mainOffsetFromParentPadding = flipYAbsoluteMeasures * (_mainAxisContainersStartPadding - _mainAxisContainersEndPadding);
         break;
        case FlexJustify.SPACE_AROUND:
        case FlexJustify.SPACE_EVENLY:
        case FlexJustify.SPACE_BETWEEN:
         mainOffsetFromParentPadding = _mainAxisContainersStartPadding;
         break;
        default:
         throw new Exception( "Impossible:52jklj" );
       }

       switch( container.alignItems ) // Cross axis based on ALIGN
       {
        case AlignItems.START:
        case AlignItems.STRETCH:
         crossOffsetFromParentPadding = _crossAxisContainersStartPadding;
         break;

        case AlignItems.END:
         crossOffsetFromParentPadding = -1f * _crossAxisContainersEndPadding;
         break;

        case AlignItems.CENTER:
         crossOffsetFromParentPadding = 1f * (_crossAxisContainersStartPadding - _crossAxisContainersEndPadding);
         /** NB: cross-alignment starts at CENTER, not LEFT/TOP edge (used by main-alignment), so we have to half any padding-offsets */
         crossOffsetFromParentPadding *= 0.5f;
         break;

        default:
         crossOffsetFromParentPadding = 0;
         break;
       }

      }

      /**
       *
       * ------ Step 5: Position each item along main-axis and cross-axis ------
       *
       * 
       */
      foreach( var child in flexLine )
      {
       /*
        * ...now iterating over each child...
        *
        * Define position as:
        *
        *     position = mainOffsetFromParentPadding + justifiedStartMainAxis + offsetAccumulatedFromPreviousItems + (preAdvance + itemLeadingMargin + childFinalLength / 2f);
        */


       /** 5b. the child's length along main axis */
       childFinalLength = d.axisMain.Length( finalSizes[child] );


       /** Pre-calculate the axis-less margins, as they're used in two places: one for main-axis position, and a second time for cross-axis position */
       float childMarginLeft = child.cssMargins.LeftOrZero( d.innerContentSize );
       float childMarginRight = child.cssMargins.RightOrZero( d.innerContentSize );
       float childMarginTop = child.cssMargins.TopOrZero( d.innerContentSize );
       float childMarginBottom = child.cssMargins.BottomOrZero( d.innerContentSize );

       /**
        * 5.Final: position the element, and accumulate the offset
        */
       float itemLeadingMargin, itemTrailingMargin;
       switch( container.direction )
       {
        case FlexDirection.ROW:
         itemLeadingMargin = childMarginLeft;
         itemTrailingMargin = childMarginRight;
         break;
        case FlexDirection.ROW_REVERSED:
         itemLeadingMargin = childMarginRight;
         itemTrailingMargin = childMarginLeft;
         break;
        case FlexDirection.COLUMN:
         itemLeadingMargin = childMarginTop;
         itemTrailingMargin = childMarginBottom;
         break;
        case FlexDirection.COLUMN_REVERSED:
         itemLeadingMargin = childMarginBottom;
         itemTrailingMargin = childMarginTop;
         break;
        default:
         throw new Exception( "Impossible: w3dfg32lr" );
       }


       /** Positioning: */
       float mainAxisPosition = justifiedStartMainAxis // the only one that is absolute, and hence not subject to a direction
                                +
                                mainAxisAdvanceDirection
                                *
                                (
                                 mainOffsetFromParentPadding
                                 +
                                 offsetAccumulatedFromPreviousItems // everthing after this gets added to this on the next loop
                                 +
                                 preAdvance
                                 +
                                 itemLeadingMargin
                                 +
                                 childFinalLength / 2f
                                );

       if( container.showDebugMessages )
        Debug.Log( "[" + container.name + "." + child.name + "] position will be: **[ " + mainAxisPosition + "] ** <== "
                   + " mainaxisJustification:" + justifiedStartMainAxis
                   + " + (main-advance-direction:" + mainAxisAdvanceDirection + ") * {"
                   + " parentpadding:" + mainOffsetFromParentPadding
                   + " + offset:" + offsetAccumulatedFromPreviousItems
                   + " + preadv:" + preAdvance
                   + " + itemmargin(LEADING):" + itemLeadingMargin
                   + " + (0.5*objectsize):" + childFinalLength / 2f + "" );

       /**
        * ACCUMULATE the total advance from this child:
        */
       offsetAccumulatedFromPreviousItems += preAdvance + (itemLeadingMargin + childFinalLength + itemTrailingMargin) + postAdvance;

       /** Cross Axis */
       /** Cross-axis margin handling is bizarre and complicated (many strange behaviours due to the CSS3 spec).
        *  ... first you position according to center/start/end/etc
        *  ... then you SELECTIVELY IGNORE margins based on whichever align-items value you had (row + center honours top + bottom; row + start IGNORES bottom, etc)
        */
       //float crossFullLength = crossAxisContainerContentLength;
       float crossChildLength = d.axisCross.Length( finalSizes[child] );

       if( container.showDebugMessages ) Debug.Log( "[" + container.name + "." + child.name + "] ... cross-axis child-length: " + crossChildLength );

       /** ... cross-axis: calculate center point along cross axis */
       bool mainIsVertical = d.axisMain == Axis.Vertical;
       float crossAxisPosition = 0f;
       float crossAxisChildStartOffset = 0f; // Unlike Main, Cross has a different ABSOLUTE start point PER-CHILD
       float itemCrossLeadingMargin = 0f;
       switch( container.alignItems )
       {
        case AlignItems.CENTER:
        {
         crossAxisChildStartOffset = flipYAbsoluteMeasures * crossChildLength / 2f;
         if( mainIsVertical )
          itemCrossLeadingMargin = (childMarginLeft - childMarginRight) / 2f;
         else
          itemCrossLeadingMargin = (childMarginTop - childMarginBottom);
        }
         break;

        case AlignItems.START:
        case AlignItems.STRETCH:
        {
         crossAxisChildStartOffset = 0f;
         if( mainIsVertical )
          itemCrossLeadingMargin = childMarginLeft;
         else
          itemCrossLeadingMargin = childMarginTop;
        }
         break;

        case AlignItems.END:
        {
         //crossAxisChildStartOffset = crossChildLength/2f;
         //works for COL:
         crossAxisChildStartOffset = crossChildLength * flipYAbsoluteMeasures;
         if( mainIsVertical )
          itemCrossLeadingMargin = 0 - (childMarginRight);
         else
          itemCrossLeadingMargin = 0 - (childMarginBottom);
        }
         break;

        case AlignItems.BASELINE:
        {
         Debug.LogError( "FlexBox layouts: 'alignItems=Baseline' not supported" );
        }
         break;
       }

       /** ... cross-axis: calculate final position on cross-axis dimension */
       crossAxisPosition = justifiedStartCrossAxis + crossAxisChildStartOffset
                                                   + crossAxisAdvanceDirection * (crossOffsetFromParentPadding + itemCrossLeadingMargin + crossChildLength / 2f)
                                                   + crossAxisAdvanceDirection * accumulatedPreviousLineCrossLengths;

       /** Position it */
       (child.transform as RectTransform).anchoredPosition = (d.axisMain == Axis.Horizontal)
        ? new Vector2( mainAxisPosition, crossAxisPosition )
        : new Vector2( crossAxisPosition, mainAxisPosition );

       //if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "] ...cross-axis changes to position: (" +  + "," + positionY + ")");
      }

      accumulatedPreviousLineCrossLengths += flexLineCrossHeights[flexLine];
     }
    }
    finally
    {
     samplerMainAlgorithmPositioning.End();
    }
   }
   finally
   {
    samplerMainAlgorithm.End();
   }
  }

  private FlexLayoutData FlexLayoutSetup(FlexContainer container, Vector2 availableSize)
  {
   FlexLayoutData result = new FlexLayoutData();

   result.containerAsItem = container.gameObject.GetComponent<FlexItem>();
   result.containerTransform = container.transform as RectTransform;

   /** We need to know the container's own container, so that we can calculate the container's padding (used to layout container's children)
    */
   result.containersParent = (container.transform.parent == null) ? null : container.transform.parent.gameObject.GetComponent<FlexContainer>();
   result.containersParentSize = (result.containersParent == null) ? (Vector2?) null : (result.containersParent.transform as RectTransform).rect.size;
   
   /** NB: the following line relies upon the careful design of mine that Containers are always sized before they are laid-out,
    * and the algorithm for sizing them will layout their children if it has to, so that containers of indefinite size get to
    * this method (their chance to do layout) with a size that is ALREADY correctly set to what it will end up being after the
    * layout completes.
    *
    * This is not the most performant, but the loss in performance is very small, and the advantage is greatly simplified code
    * for the main layout method.
    */
   result.outerContentSize = result.innerContentSize = availableSize;
   if( result.containerAsItem != null ) // it may have internal margins that we need to offset to generate the true innerContentSize
   {
    // TODO: these should be calculated lazily, as the PARENT-CONTAINER's size could be CONTENT and depend upon this container
    result.containersPaddingLeft = result.containerAsItem.cssPadding.LeftOrNull(result.containersParentSize) ?? 0;
    result.containersPaddingRight = result.containerAsItem.cssPadding.RightOrNull(result.containersParentSize) ?? 0;
    result.containersPaddingTop = result.containerAsItem.cssPadding.TopOrNull(result.containersParentSize) ?? 0;
    result.containersPaddingBottom = result.containerAsItem.cssPadding.BottomOrNull(result.containersParentSize) ?? 0;

    result.innerContentSize.x -= result.containersPaddingLeft + result.containersPaddingRight;
    result.innerContentSize.y -= result.containersPaddingTop + result.containersPaddingBottom;
   }

   result.axisMain = MainAxis(container.direction);

   /**
    * Gather the direct child layout elements
    * 
    * NB: Unity's GetComponentsInChildren API does NOT do what the container.name suggests it does: instead it returns components on SELF AS WELL AS on children with this call. Also, we need a List,
    * which Unity still (in 2019) does not provide in their API.
    */
   result.childItems = new List<FlexItem>();
   FlexItem component = null;
   foreach( Transform t in container.transform )
   {
    if( t.gameObject.activeSelf )
    {
     component = t.gameObject.GetComponent<FlexItem>();
     if( component != null )
      result.childItems.Add(component);
    }
   }

   /**
    * Spec: https://www.w3.org/TR/css-flexbox-1/#order-property
    * 
    * ------ Sort children (this is needed AT LEAST before we start the flex-lines / flex-wrap code) ------
    *
    * Sort them according to Order (spec says: sort each flexOrder as a complete group), but defaulting to Transform order where .order values are identical
    */
   Dictionary<FlexItem, int> flexOrderGroups = new Dictionary<FlexItem, int>();
   Dictionary<FlexItem, int> orderWithinGroup = new Dictionary<FlexItem, int>();
   foreach( var c in result.childItems )
   {
    flexOrderGroups[c] = c.flexOrder;
    orderWithinGroup[c] = result.childItems.IndexOf(c);
   }
   result.childItems.Sort((a, b) =>
    {
     int aGroup = flexOrderGroups[a];
     int bGroup = flexOrderGroups[b];

     int comparedGroups = aGroup.CompareTo(bGroup);

     if( comparedGroups != 0 )
      return comparedGroups;

     int aSuborder = orderWithinGroup[a];
     int bSuborder = orderWithinGroup[b];

     return aSuborder.CompareTo(bSuborder);
    }
   );
   
   return result;
  }

  /**
   * Why is this method so difficult to get right, especially when adding WRAP? ... well ... it turns out that it's
   * SO difficult that the CSS Specification authors had to write an ENTIRE CHAPTER on the problems, called "Orthogonal Flow",
   * see here: https://www.w3.org/TR/css-writing-modes-3/#orthogonal-auto
   */
  private Vector2 ContentSize_AContainer(FlexContainer element, Vector2 containingBlocksizeForOrthogonalFLow )//, VectorNullable2 elementFixedSize ) // TODO: rename 2nd arg to "availableSpaceInParent_orSelfFixedSize" or similar
  {
   samplerContentSizeContainer.Begin();
   try
   {
    FlexItem elementAsItem = element.GetComponent<FlexItem>();

    /** Container CONTENT size always depends on its children, so we'll need these in all the approaches below */
    List<FlexItem> children = new List<FlexItem>();
    foreach( Transform t in element.transform )
    {
     if( t.gameObject.activeSelf )
     {
      FlexItem component = t.gameObject.GetComponent<FlexItem>();
      if( component != null )
       children.Add( component );
     }
    }

    Axis mainAxis = MainAxis( element.direction );
    Axis crossAxis = CrossAxis( element.direction );

    /**
     * Each child's size is: main-length & cross-length
     *
     * (Hypothetical lengths = base-length constrained by minWidth/maxWidth and minHeight/maxHeight)
     */
    Dictionary<FlexItem, float> mainHypotheticalLengths = new Dictionary<FlexItem, float>();
    Dictionary<FlexItem, float> crossHyptheticalLengths = new Dictionary<FlexItem, float>();


    if( element.showDebugMessages )
     Debug.Log( "[" + element.name + "] Calculating self-size as a CONTENT-mode container; with: " + children.Count
                + " children, and available-space for children = ?" /*+elementsInnerSize*/ );
    foreach( var child in children )
    {
     #if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
     float childMain = Base_MainLength( child, MainAxis( element.direction ), containingBlocksizeForOrthogonalFLow, out Vector2? baseSizeBothAxes, element );
     #else
     float childMain = Base_MainLength( child, MainAxis( element.direction ), containingBlocksizeForOrthogonalFLow, element );
#endif
     
     mainHypotheticalLengths[child] = HypotheticalMainLength( child, element.direction, childMain, new VectorNullable2( containingBlocksizeForOrthogonalFLow ) );

#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
     float childCross = Base_CrossLength( child, CrossAxis( element.direction ), containingBlocksizeForOrthogonalFLow, baseSizeBothAxes );
     #else
     float childCross = Base_CrossLength( child, CrossAxis( element.direction ), containingBlocksizeForOrthogonalFLow );
#endif
     crossHyptheticalLengths[child] = HypotheticalCrossLength( child, element.direction, childCross, new VectorNullable2( containingBlocksizeForOrthogonalFLow ) );

     if( element.showDebugMessages ) Debug.Log( "[" + element.name + "] ...Child: " + child + " has base-main = " + childMain + "" );
    }

    /** Calculate flex lines: will be precisely 1 if NOWRAP, and 1 or more if WRAP */
    // Original (v3.0.0)
    //List<List<FlexItem>> flexLines = CalculateFlexLines( element.wrap, null, children, null /*mainAxis.Length(elementsInnerSize)*/, mainHypotheticalLengths, crossHyptheticalLengths, out Dictionary<List<FlexItem>,float> unusedMainSpacePerLine, out Dictionary<List<FlexItem>,float> lineBaseCrossLengths );
    // Improved (v3.0.1) allows responsive design to ROW+WRAP inside a vertical scrollview: 
    List<List<FlexItem>> flexLines = CalculateFlexLines( element.wrap, null, children, mainAxis.Length( containingBlocksizeForOrthogonalFLow ) /*mainAxis.Length(elementsInnerSize)*/, mainHypotheticalLengths, crossHyptheticalLengths, out Dictionary<List<FlexItem>, float> unusedMainSpacePerLine, out Dictionary<List<FlexItem>, float> lineBaseCrossLengths );
    // Failes -- causes all the layout to go wrong and huge and bizarre
    //List<List<FlexItem>> flexLines = CalculateFlexLines( element.wrap, crossAxis.Length(containingBlocksizeForOrthogonalFLow), children, mainAxis.Length(containingBlocksizeForOrthogonalFLow) /*mainAxis.Length(elementsInnerSize)*/, mainHypotheticalLengths, crossHyptheticalLengths, out Dictionary<List<FlexItem>,float> unusedMainSpacePerLine, out Dictionary<List<FlexItem>,float> lineBaseCrossLengths );

    if( element.showDebugMessages )
     for( int lineIndex = 0; lineIndex < flexLines.Count; lineIndex++ )
      Debug.Log( "[" + element.name + "] .. as a CONTENT-mode container, found: " + flexLines.Count + " flexlines, line[" + lineIndex + "] has cross-length = " + lineBaseCrossLengths[flexLines[lineIndex]] );

    /**
     * Spec 9.4.8: https://www.w3.org/TR/css-flexbox-1/#algo-cross-line
     * 
     * For each flexline: calculate a separate cross-length for that specific line.
     * Sum the totals to get the final cross-length
     */
    float totalCrossLength = 0;
    foreach( var line in lineBaseCrossLengths )
     totalCrossLength += line.Value;

    float totalMainLength = 0;
#if THIS_IS_ALL_WRONG
   /**
    * Spec 9.2.2: https://www.w3.org/TR/css-flexbox-1/#algo-available
    *    "	if that dimension of the flex container’s content box is a definite size, use that"
    * 
    * Calculate total main length based on whether it was specified or not
    *
    * TODO: this is a bit dodgy ... why isn't this re-using code from the layout system?
    */
   float? definiteContainerMainSize = ResolveDefiniteSize(elementAsItem, mainAxis, elementsInnerSize);
   if( definiteContainerMainSize != null )
   {
    totalMainLength = definiteContainerMainSize.Value;
   }
   else // sum the child main lengths
#endif
    {
     foreach( var child in children ) // NB: no special code needed for wrap: if inner-main was indefinite, wrapping didn't happen
      totalMainLength += mainHypotheticalLengths[child];
    }

    /***************** We now know the cross length and the main length ************************/

    switch( MainAxis( element.direction ) )
    {
     case Axis.Horizontal:
      return new Vector2( totalMainLength, totalCrossLength );
     case Axis.Vertical:
      return new Vector2( totalCrossLength, totalMainLength );
     default:
      throw new Exception( "Impossible: lkjw5dfs9" );
    }
   }
   finally
   {
    samplerContentSizeContainer.End();
   }
  }

  private bool _FirstActiveChildUIComponent<T>(FlexItem child, out T uiItem ) where T : UIBehaviour
  {
   /**
    * NB: Unity's API method seems to allow you to skip the "isactive" check, but does not!
    *
    * The API signature is wrong, Unity closed as "by design": https://issuetracker.unity3d.com/issues/getcomponentsinchildren-always-gets-components-from-inactive-objects
    */
   
#if AUTOSIZED_CALC_ONLY_SEARCH_DIRECT_CHILDREN_FOR_EMBEDDED_UIBEHAVIOURS_WHEN_DOING_AUTOCONTENT_SIZING
   /**
    * NB: we ONLY want to check the direct children; once you look at grandchildren, it's "undefined"
    * in UnityUI (and in flexbox!) what that even means, in layout terms.
    *
    * Also: if we recurse any lower than one level we risk huge performance penalties in extreme cases, for (probably) no functional benefit.
    */
   for( int i = 0; i < child.transform.childCount; i++ )
   {
    if( child.transform.GetChild( i ).TryGetComponent<T>( out uiItem ) && uiItem.IsActive() )
     return true;
   }
#else
/**
 * This is faster in the non-recursive case because Unity's API calls are strangely optimized, but
 * it is less correct.
 */
   T[] descendents = child.GetComponentsInChildren<T>();
   foreach( T d in descendents )
    if( d.IsActive() )
    {
     uiItem = d;
     return true;
    }
#endif
   uiItem = null;
   return false;
  }
  
  private Vector2? ContentSize_NotAContainer(FlexItem child)//, Axis axis)
  {
   samplerContentSizeNonContainer.Begin();
   try
   {
    #if AUTOSIZED_CALC_BATCH_GETCOMPONENT_CALLS
    /********************************************************
     *
     * 
     * ATTACHED components take priority over CHILDED ones
     *
     *
     ********************************************************/

    
    /**
     * MASSIVE BUG in Unity (all versions): Button (and all UIBehaviour!) is incompatible with Unity's own layout system (it doesn't implement ILayoutElement)
     * so we have to write slow and complex code here to workaround Unity's bad/wrong API :(.
     */
    UIBehaviour[] behaviourSubclasses = child.GetComponents<UIBehaviour>();
    ILayoutElement[] layoutElementSubclasses = child.GetComponents<ILayoutElement>();
    
    /**
     * Must check ATTACHED components in this order of importance:
     *   1. Text
     *   2. Button
     *   3. Toggle
     *   4. Input Textfield
     *   100. Image (generic)
     *   101. RawImage (generic)
     *   999. [ILayoutElement subclass]
     */
    foreach( var element in layoutElementSubclasses )
     if( element is Text )
      return ContentSizeCalculators.ContentSizeFor( (Text) element ); 
    foreach( var behaviour in behaviourSubclasses )
      if( behaviour is Button )
       return ContentSizeCalculators.ContentSizeFor( (Button) behaviour ); 
    foreach( var behaviour in behaviourSubclasses )
      if( behaviour is Toggle )
       return ContentSizeCalculators.ContentSizeFor( (Toggle) behaviour ); 
    foreach( var element in layoutElementSubclasses )
      if( element is InputField )
       return ContentSizeCalculators.ContentSizeFor( (InputField) element ); 
    foreach( var element in layoutElementSubclasses )
      if( element is Image )
       return ContentSizeCalculators.ContentSizeFor( (Image) element ); 
    foreach( var behaviour in behaviourSubclasses )
      if( behaviour is RawImage )
       return ContentSizeCalculators.ContentSizeFor( (RawImage) behaviour );
      
    if( layoutElementSubclasses.Length > 0 )
    {
     foreach( ILayoutElement element in layoutElementSubclasses )
     {
      /** if it's one of ours, skip it (the child itself will certainly match!) ... no others of ours should be there, but just in case... */
      if( element is FlexItem || element is FlexContainer )
       continue;

      /** Discard disabled components.
       *
       * NB: this means we REQUIRE the potentially-matching ILayoutElements to be instances of MonoBehaviour;
       * since we grabbed them using "GetComponents" we can reasonably expect that to be guaranteed to be true!
       */
      if( !(element as MonoBehaviour).isActiveAndEnabled )
       continue;

      /* DEBUG:
      Vector2 min = new Vector2(element.minWidth, element.minHeight);
      Vector2 flexi = new Vector2(element.flexibleWidth, element.flexibleHeight);
      Vector2 pref = new Vector2(element.preferredWidth, element.preferredHeight);
      
      Debug.LogError("Element: "+element+" has min/pref/flexi: "+min+" / "+pref+" / "+flexi);
      */
      return new Vector2( element.preferredWidth, element.preferredHeight );
     }
    }

    Text childAsText;
    Button childAsButton;
    Toggle childAsToggle;
    InputField childAsInputField;
    Image asImage;
    RawImage asRawImage;
    #else
    /********************************************************
     *
     * 
     * ATTACHED components take priority over CHILDED ones
     *
     *
     **********************
     * Note: amazingly (!) Unity's core API's are so slow that it is actually faster to fetch every
     * individual component and check them by hand ... than to fetch all at once and check them separately.
     * This is because they still haven't upgraded GetComponents* methods to be non-allocating in Editor...
     ********************************************************/

    /**
     * ATTACHED components:
     *   1. Text
     *   1b. TextMeshPro (uses TMP_Text as superclass of TextMeshProUGUI)
     */
#if AUTOSIZED_CALC_NONALLOC_GETCOMPONENT
    if( child.TryGetComponent<Text>( out Text childAsText ) )
#else
    Text childAsText = child.GetComponent<Text>();
    if( childAsText != null )
#endif
    {
     var sz = ContentSizeCalculators.ContentSizeFor( childAsText );
     child.lastReportedUnityTextWidthInPixels = sz.x;
     //Debug.Log("Text reports size: "+sz);
     return sz;
    }
   #if AVAILABLE_TEXTMESHPRO
    #if AUTOSIZED_CALC_IGNORE_CUSTOM_ATTACHED_ELEMENTS
     #if AUTOSIZED_CALC_NONALLOC_GETCOMPONENT
    if( child.TryGetComponent<TMP_Text>( out TMP_Text childAsTMP ) )
     #else
    TMP_Text childAsTMP = child.GetComponent<TMP_Text>();
    if( childAsTMP != null )
     #endif
    {
     var sz = ContentSizeCalculators.ContentSizeFor( childAsTMP );
     child.lastReportedUnityTextWidthInPixels = sz.x;
     //Debug.Log("Text reports size: "+sz);
     return sz;
    }
    #else
    TMP_Text childAsTMP;
    #endif
   #endif

    /**
     * ATTACHED components:
     *   2. Button
     */
#if AUTOSIZED_CALC_NONALLOC_GETCOMPONENT
    if( child.TryGetComponent<Button>( out Button childAsButton ) )
#else
    Button childAsButton = child.GetComponent<Button>();
    if( childAsButton != null )
#endif
     return ContentSizeCalculators.ContentSizeFor( childAsButton );

    /**
     * ATTACHED components:
     *   3. Toggle
     */
#if AUTOSIZED_CALC_NONALLOC_GETCOMPONENT
    if( child.TryGetComponent<Toggle>( out Toggle childAsToggle ) )
#else
    Toggle childAsToggle = child.GetComponent<Toggle>();
    if( childAsToggle != null )
#endif
     return ContentSizeCalculators.ContentSizeFor( childAsToggle );

    /**
     * ATTACHED components:
     *   4. Input Textfield
     */
#if AUTOSIZED_CALC_NONALLOC_GETCOMPONENT
    if( child.TryGetComponent<InputField>( out InputField childAsInputField ) )
#else
    InputField childAsInputField = child.GetComponent<InputField>();
    if( childAsInputField != null )
#endif
     return ContentSizeCalculators.ContentSizeFor( childAsInputField );

    /**
     * ATTACHED components:
     *   100. Image (generic)
     */
#if AUTOSIZED_CALC_NONALLOC_GETCOMPONENT
    if( child.TryGetComponent<Image>( out Image asImage ) )
#else
    Image asImage = child.GetComponent<Image>();
    if( asImage != null )
#endif
     return ContentSizeCalculators.ContentSizeFor( asImage );

    /**
     * ATTACHED components:
     *   101. RawImage (generic)
     */
#if AUTOSIZED_CALC_NONALLOC_GETCOMPONENT
    if( child.TryGetComponent<RawImage>( out RawImage asRawImage ) )
#else
    RawImage asRawImage = child.GetComponent<RawImage>();
    if( asRawImage != null )
#endif
     return ContentSizeCalculators.ContentSizeFor( asRawImage );

    /**
     * ATTACHED components:
     *   999. [ILayoutElement subclass]
     */
    
#if AUTOSIZED_CALC_IGNORE_CUSTOM_ATTACHED_ELEMENTS
    /* DEBUG:
    ILayoutElement[] layoutElementSubclasses = child.GetComponents<ILayoutElement>();
    if( layoutElementSubclasses.Length > 0 )
    {
     foreach( ILayoutElement element in layoutElementSubclasses )
     if( !(element is FlexItem) && !(element is FlexContainer) )
      Debug.Log( "Found + ignoring attached ILayoutElement subclass: {" + child.hierarchicalName + "}." + element.GetType() );
    }
    */
#else
    ILayoutElement[] layoutElementSubclasses = child.GetComponents<ILayoutElement>();
    if( layoutElementSubclasses.Length > 0 )
    {
     foreach( ILayoutElement element in layoutElementSubclasses )
     {
      /** if it's one of ours, skip it (the child itself will certainly match!) ... no others of ours should be there, but just in case... */
      if( element is FlexItem || element is FlexContainer )
       continue;

      /** Discard disabled components.
       *
       * NB: this means we REQUIRE the potentially-matching ILayoutElements to be instances of MonoBehaviour;
       * since we grabbed them using "GetComponents" we can reasonably expect that to be guaranteed to be true!
       */
      if( !(element as MonoBehaviour).isActiveAndEnabled )
       continue;

      /* DEBUG:
      Vector2 min = new Vector2(element.minWidth, element.minHeight);
      Vector2 flexi = new Vector2(element.flexibleWidth, element.flexibleHeight);
      Vector2 pref = new Vector2(element.preferredWidth, element.preferredHeight);
      
      Debug.LogError("Element: "+element+" has min/pref/flexi: "+min+" / "+pref+" / "+flexi);
      */
      return new Vector2( element.preferredWidth, element.preferredHeight );
     }
    }
    #endif
#endif

    /********************************************************************************************************
     * 
     *         NO directly-attached "known components" found; now going to search descendents ...
     * 
     ********************************************************************************************************/


    /**
     * CHILDED components:
     *   1. Button -- NB: we do this BEFORE CHILDED.Text, because both will fire when there's a button, and this should take priority (but same does NOT happen with ATTACHED components)
     */
    if( _FirstActiveChildUIComponent<Button>( child, out childAsButton ) )
     return ContentSizeCalculators.ContentSizeFor( childAsButton );

    /**
     * CHILDED components:
     *   2. InputField -- NB: we do this BEFORE CHILDED.Text, because both will fire when there's one, and this should take priority (but same does NOT happen with ATTACHED components)     */
    if( _FirstActiveChildUIComponent<InputField>( child, out childAsInputField ) )
     return ContentSizeCalculators.ContentSizeFor( childAsInputField );

    /**
     * CHILDED components:
     *   3. Text -- NB: this would conflict with the search for a Button or InputField (which has an embedded Text), which is why we only do it AFTER checking the Button
     *   3b. TextMeshPro
     */
    if( _FirstActiveChildUIComponent<Text>( child, out childAsText ) )
     return ContentSizeCalculators.ContentSizeFor( childAsText );
#if AVAILABLE_TEXTMESHPRO
    if( _FirstActiveChildUIComponent<TMP_Text>( child, out childAsTMP )) 
     return ContentSizeCalculators.ContentSizeFor( childAsTMP );
#endif

    /**
     * CHILDED components:
     *   4. Toggle -- NB: we do this BEFORE CHILDED.Image, because both will fire when there's one, and this should take priority (but same does NOT happen with ATTACHED components)     */
    if( _FirstActiveChildUIComponent<Toggle>( child, out childAsToggle ) )
     return ContentSizeCalculators.ContentSizeFor( childAsToggle );

    /**
     * CHILDED components:
     *   100. Image (generic)
     */
    if( _FirstActiveChildUIComponent<Image>( child, out asImage ) )
     return ContentSizeCalculators.ContentSizeFor( asImage );

    /**
     * CHILDED components:
     *   101. RawImage (generic)
     */
    if( _FirstActiveChildUIComponent<RawImage>( child, out asRawImage ) )
     return ContentSizeCalculators.ContentSizeFor( asRawImage );

    return null;
   }
   finally
   {
    samplerContentSizeNonContainer.End();
   }
  }
  
  /**
   * Why is this method (and the ones it invokes) so difficult to get right, especially when adding WRAP? ... well ... it turns out that it's
   * SO difficult that the CSS Specification authors had to write an ENTIRE CHAPTER on the problems, called "Orthogonal Flow",
   * see here: https://www.w3.org/TR/css-writing-modes-3/#orthogonal-auto
   * 
   * NB: we calculate both width and height for this item and as many of its descendents as required (in most cases,
   * only one level deep is enough, sometimes 2 or 3 depending on how many dependencies there are), even though we
   * often only need ONE of those numbers -- because trying to keep track of which axis is which, when you are nested
   * and recursing, proved a source of way too many bugs. This gives a small performance impact -- but note: in many
   * cases (most? nearly all?) you will end up returning to this method and doing the other axis anyway when you come
   * back to CROSS axis having already done MAIN axis.
   * 
   * TODO: cache the results so that the calc is only done once not twice (currently SOMETIMES done: once in MAIN axis, and again in CROSS axis)
   */
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
  private float Base_ContentSize_OnAxis( FlexItem child, Vector2 containingBlocksizeForOrthogonalFLow, Axis axis, out Vector2? baseSizeBothAxes, Vector2? preCalcedContentSizeBothAxes = null )
  #else
  private float Base_ContentSize_OnAxis( FlexItem child, Vector2 containingBlocksizeForOrthogonalFLow, Axis axis )
#endif
  {
   /**
    *
    * 
    * NOTE: this method is ONLY called when we already know that the child.flexBasis == CONTENT.
    *
    * It has to be a method because it's called from two different places in the same parent method
    * (required by spec, which treats SOME variants of AUTO as identical to CONTENT).
    *
    * 
    */
   
   /** If the child sets width and height, use them - else use the parent's sizes */
   //VectorNullable2 childInnerSize = ResolveDefaultWidthHeight( child, parentInnerSize);
   //Vector2 availableSize = new Vector2( childInnerSize.x?? parentInnerSize.x, childInnerSize.y?? parentInnerSize.y );
   
   /** If the child has a specified width or height, we'll honour them */
   VectorNullable2 fixedSizeOfChild = ResolveDefaultWidthHeight(child, containingBlocksizeForOrthogonalFLow);
   if( axis.Length( fixedSizeOfChild ).HasValue )
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
   {
    baseSizeBothAxes = null;
    return axis.Length( fixedSizeOfChild ).Value;
   }
   #else
    return axis.Length(fixedSizeOfChild).Value;
#endif

   FlexContainer childAsContainer = child.transform.parent == null ? null : child.GetComponent<FlexContainer>();
   float? length;
   if( childAsContainer != null ) // It's a FlexContainer declaring that its size is the size required by its children flex-items
   {
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
    /** Try to use the precalc'd size first if possible (only applies if this method was previously called (eg for MAIN axis),
     * calcd the size, and exported the size, and the caller cached that data, and then passed it back in)
     */
    Vector2? childBaseSize = preCalcedContentSizeBothAxes != null ? preCalcedContentSizeBothAxes : ContentSize_AContainer( childAsContainer, containingBlocksizeForOrthogonalFLow );

    baseSizeBothAxes = childBaseSize;
    #else
    Vector2? childBaseSize = ContentSize_AContainer(childAsContainer, containingBlocksizeForOrthogonalFLow );
#endif
    length = axis.Length( childBaseSize );
    if( childAsContainer.showDebugMessages ) Debug.Log( "[" + childAsContainer.name + "] base CONTENT size = " + childBaseSize + "(resolves to base-length: " + length + ") for axis = " + axis );
   }
   else // it's NOT a container...
   {
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
    /** Try to use the precalc'd size first if possible (only applies if this method was previously called (eg for MAIN axis),
     * calcd the size, and exported the size, and the caller cached that data, and then passed it back in)
     */
    Vector2? autoCalculatedChildContentSize = preCalcedContentSizeBothAxes != null ? preCalcedContentSizeBothAxes : ContentSize_NotAContainer(child);
    baseSizeBothAxes = autoCalculatedChildContentSize;
    #else
    Vector2? autoCalculatedChildContentSize = ContentSize_NotAContainer(child);
#endif
    length = axis.Length(autoCalculatedChildContentSize);
   }
   
   if( length != null )
    return length.Value;
   else
   {
    //Debug.LogError("We have no way of calculating a CONTENT-based length for flexitem = "+child.hierarchicalName+". Each FlexItem needs to provide this info - or a pluggable system for them to do so from context cues (do they have a Unity.Image attached? Or as sole child? ... etc)");

    return 0;
   }
  }
  
  /**
   * Spec: https://www.w3.org/TR/css-flexbox-1/#valdef-flex-basis-auto
   * 
   * "When specified on a flex item, the auto keyword retrieves the value of the main size property as the used flex-basis. If that value is itself auto, then the used value is content."
   * 
   * .. i.e. width or height *if* they are specified, otherwise: Content
   *
   * @return the float value using item's width|height if possible ... and null otherwise (if null, caller needs to calculate CONTENT themself)
   */
  private float? ResolveAutoBasis( FlexItem item, Axis axis, Vector2? containerInnerSize )
  {
   if( (axis == Axis.Horizontal && item.cssDefaultWidth.hasValue) )
    return item.cssDefaultWidth.ValueOrZero(axis.Length(containerInnerSize));
   else if( (axis == Axis.Vertical && item.cssDefaultHeight.hasValue) )
    return item.cssDefaultHeight.ValueOrZero(axis.Length(containerInnerSize));
   else
    return null; 
  }
  private float? ResolveAutoBasis( FlexItem item, Axis axis, VectorNullable2? containerInnerSize )
  {
   if( (axis == Axis.Horizontal && item.cssDefaultWidth.hasValue) )
    return item.cssDefaultWidth.ValueOrZero(axis.Length(containerInnerSize));
   else if( (axis == Axis.Vertical && item.cssDefaultHeight.hasValue) )
    return item.cssDefaultHeight.ValueOrZero(axis.Length(containerInnerSize));
   else
    return null; 
  }
  private float? ResolveDefiniteSize( FlexItem item, Axis axis, float? containerLength )
  {
   if( (axis == Axis.Horizontal && item.cssDefaultWidth.hasValue) )
    return item.cssDefaultWidth.ValueOrZero(containerLength);
   else if( (axis == Axis.Vertical && item.cssDefaultHeight.hasValue) )
    return item.cssDefaultHeight.ValueOrZero(containerLength);
   else
    return null; 
  }
  private float? ResolveDefiniteSize( FlexItem item, Axis axis, VectorNullable2 containerInnerSize )
  {
   if( (axis == Axis.Horizontal && item.cssDefaultWidth.hasValue) )
    return item.cssDefaultWidth.ValueOrZero(axis.Length(containerInnerSize));
   else if( (axis == Axis.Vertical && item.cssDefaultHeight.hasValue) )
    return item.cssDefaultHeight.ValueOrZero(axis.Length(containerInnerSize));
   else
    return null; 
  }
  private float? ResolveDefiniteSize( FlexItem item, Axis axis, Vector2 containerInnerSize )
  {
   if( (axis == Axis.Horizontal && item.cssDefaultWidth.hasValue) )
    return item.cssDefaultWidth.ValueOrZero(axis.Length(containerInnerSize));
   else if( (axis == Axis.Vertical && item.cssDefaultHeight.hasValue) )
    return item.cssDefaultHeight.ValueOrZero(axis.Length(containerInnerSize));
   else
    return null; 
  }
  
  private VectorNullable2 ResolveDefaultWidthHeight( FlexItem item, Vector2? containerInnerSize )
  {
   VectorNullable2 value = new VectorNullable2();
   
     value.x = item.cssDefaultWidth.ValueOrNull( containerInnerSize?.x );
     value.y = item.cssDefaultHeight.ValueOrNull( containerInnerSize?.y );
   
   return value; 
  }
  private VectorNullable2 OverwriteUsingDefaultWidthHeightIfPresent( FlexItem item, VectorNullable2 containerInnerSize )
  {
   VectorNullable2 value = new VectorNullable2();
   
   value.x = item.cssDefaultWidth.ValueOrNull( containerInnerSize.x ) ?? containerInnerSize.x;
   value.y = item.cssDefaultHeight.ValueOrNull( containerInnerSize.y ) ?? containerInnerSize.y;
   
   return value; 
  }
  private Vector2 OverwriteUsingDefaultWidthHeightIfPresent( FlexItem item, Vector2 containerInnerSize )
  {
   Vector2 value = new Vector2();
   
   value.x = item.cssDefaultWidth.ValueOrNull( containerInnerSize.x ) ?? containerInnerSize.x;
   value.y = item.cssDefaultHeight.ValueOrNull( containerInnerSize.y ) ?? containerInnerSize.y;
   
   return value; 
  }
  
#if PROPRIETARY_ASPECT_FLEXBASIS
#if LITE_VERSION
#else
  /**
   *
   *
   * TODO: not convinced that the incoming "containingBlocksizeForOrthogonalFLow" is an acceptable match for "parentInnerSize" param
   */
  private void AspectFit_Extrapolate(VectorNullable2 parentInnerSize, float ratio, out float containerRatio, out float containerWidth, out float containerHeight)
  {
   if( !(parentInnerSize.x.HasValue || parentInnerSize.y.HasValue) )
    throw new Exception("This method only works if at least one of the dimensions on parentInnerSize is defined");
   else
   {
    /**
     * TODO: this is still "Experimental": Infer the missing dimension using the dimension that we DO have
     */
    if( parentInnerSize.x.HasValue && parentInnerSize.y.HasValue )
    {
     containerWidth = parentInnerSize.x.Value;
     containerHeight = parentInnerSize.y.Value;

     containerRatio = containerWidth / containerHeight;
    }
    else if( parentInnerSize.x.HasValue )
    {
     containerRatio = ratio;

     containerWidth = parentInnerSize.x.Value;
     containerHeight = 1f / (containerRatio / containerWidth);
    }
    else // parentInnerSize.y.HasValue
    {
     containerRatio = ratio;

     containerHeight = parentInnerSize.y.Value;
     containerWidth = (containerRatio * containerHeight);
    }
   }
  }
#endif
#endif
  
  /**
   * Base: MAIN length = try using the flexBasis to calculate a length; if it is AUTO then use the default width/height;
   * if it is CONTENT (or AUTO without width/height specified), then recursively layout children as much as needed to
   * work out the total CONTENT SIZE, and then return the MAIN axis of that.
   *
   * (This is the same as Base CROSS length, except that this starts by attempting to use the FlexBasis, which has
   * several possible values that are instantly resolveable, whereas CROSS proceeds as if Flex-"cross"-basis were
   * always AUTO by definition.)
   * 
   * Throws exception if - at any point down the chain - it reaches a PERCENT calculation which depends on a parent with
   * unspecified size. // TODO: this will artificially fail in some cases e.g. "Container.content { child.100px + child.50% }" <-- I can layout that as a human, but this algorithm will bail on it
   * 
   * CONTENT-BOX: base main-length = flexBasis + padding + margin
   * BORDER-BOX: base main-length = flexBasis + margin
   *
   * @param containerInnerSize = the main algorithm can be definite about this EXCEPT WHEN its laying out inside a ScrollView,
   *                             but when FlexContainers with basis=CONTENT are asked for their size, they HAVE TO pass this in as null,
   *                             because they don't know.
   * TODO: convert most uses of Vector2? to VectorNullable2 (with independent x? and y?) so that Containers can say "I know ONE OF my inner dimensions but not the other"
   */
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
  private float Base_MainLength(FlexItem child, Axis mainAxis, Vector2 containingBlocksizeForOrthogonalFLow, out Vector2? baseSizeBothAxes, FlexContainer containerForDebugMessages = null )
  #else
  private float Base_MainLength(FlexItem child, Axis mainAxis, Vector2 containingBlocksizeForOrthogonalFLow, FlexContainer containerForDebugMessages = null )
#endif
  {
   samplerMainLengthBase.Begin();
   try
   {
    float rawBaseLength;
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
    baseSizeBothAxes = null; // defaults to null; will be overwritten IFF it can be
#else
#endif

    /**
     * NOTE: by definition:
     *
     * https://www.w3.org/TR/css-flexbox-1/#propdef-flex-basis
     *  ... "...  flex-basis is resolved the same way as for width and height."
     * 
     * Where "width" links directly to: https://www.w3.org/TR/CSS21/visudet.html#propdef-width
     *  ... "This property specifies the content width of boxes."
     *
     * So the values we get back in the following lines will ONLY be for the content-box. Depending on boxSizing mode,
     * we then may need to add/subtract the padding!
     *
     * Firefox in 2020 does this:
     *   - for CONTENTBOX, the item is laid out as "padding + flexBasis"
     *   - for BORDERBOX, the item is laid out as "flexBasis"
     *   - for [all modes], the item is laid out as "[above] + margin" 
     */
    switch( child.flexBasis.mode )
    {
     case FlexBasis.LENGTH:
      rawBaseLength = child.flexBasis.value;
      break;

     case FlexBasis.PERCENT:
      float? attemptedLength = mainAxis.Length( containingBlocksizeForOrthogonalFLow ); // TODO: when we have VectorNullable2, we can upgrade Length?(..) to give non-null in some cases
      if( attemptedLength != null )
       rawBaseLength = child.flexBasis.value * attemptedLength.Value / 100f;
      else
       throw new FlexboxLayoutException( "Cannot layout: cannot calculate the PERCENT baseLength of child (" + child.hierarchicalName + ") using parent's indefinite inner size (" + containingBlocksizeForOrthogonalFLow + ")" );
      break;

     case FlexBasis.AUTO:
      float? attemptResolveAuto = ResolveDefiniteSize( child, mainAxis, containingBlocksizeForOrthogonalFLow );


#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
      rawBaseLength = attemptResolveAuto ?? Base_ContentSize_OnAxis( child, OverwriteUsingDefaultWidthHeightIfPresent( child, containingBlocksizeForOrthogonalFLow ), mainAxis, out baseSizeBothAxes ); // i.e. if null: skip to what we do for CONTENT below
      #else
      rawBaseLength = attemptResolveAuto ?? Base_ContentSize_OnAxis( child, OverwriteUsingDefaultWidthHeightIfPresent( child, containingBlocksizeForOrthogonalFLow ), mainAxis ); // i.e. if null: skip to what we do for CONTENT below
#endif
      break;

     case FlexBasis.CONTENT:
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
      rawBaseLength = Base_ContentSize_OnAxis( child, OverwriteUsingDefaultWidthHeightIfPresent( child, containingBlocksizeForOrthogonalFLow ), mainAxis, out baseSizeBothAxes );
      #else
      rawBaseLength = Base_ContentSize_OnAxis( child, OverwriteUsingDefaultWidthHeightIfPresent( child, containingBlocksizeForOrthogonalFLow ), mainAxis );
#endif
      break;

#if PROPRIETARY_ASPECT_FLEXBASIS
#if LITE_VERSION
    case FlexBasis.ASPECT_FIT:
     rawBaseLength = 0;
     break;
#else
     case FlexBasis.ASPECT_FIT:
      bool temporaryAspectFitTweak = true;
      if( temporaryAspectFitTweak ) // parentInnerSize.x.HasValue || parentInnerSize.y.HasValue )
      {
       AspectFit_Extrapolate( new VectorNullable2( containingBlocksizeForOrthogonalFLow ), child.flexBasis.value, out float containerRatio, out float containerWidth, out float containerHeight );

       if( child.flexBasis.value < containerRatio )
       {
        if( mainAxis == Axis.Vertical )
         rawBaseLength = containerHeight;
        else
         rawBaseLength = child.flexBasis.value * containerHeight;
       }
       else
       {
        if( mainAxis == Axis.Vertical )
         rawBaseLength = containerWidth / child.flexBasis.value;
        else
         rawBaseLength = containerWidth;
       }

       if( containerForDebugMessages != null && containerForDebugMessages.showDebugMessages )
        Debug.Log( "[" + containerForDebugMessages.name + "]: child is ASPECT_FIT, containerRatio was: " + containerRatio + ", child ratio was: " + child.flexBasis.value + ", parentInnerSize available was: " + containingBlocksizeForOrthogonalFLow );
      }
      else
      {
       rawBaseLength = 0;
      }

      break;
#endif
#endif

     default:
      throw new Exception( "Impossible: 26kdlkjfs" );
    }

    /**
     * Firefox in 2020 does this:
     *   - for CONTENTBOX, the item is laid out as "padding + flexBasis"
     *   - for BORDERBOX, the item is laid out as "flexBasis"
     *   - for [all modes], the item is laid out as "[above] + margin"
     */
    float addPadding = child.boxSizing == BoxSizing.CONTENT_BOX ? 0 : child.cssPadding.ValueOrNull( mainAxis, containingBlocksizeForOrthogonalFLow ) ?? 0;
    float addMargin = child.cssMargins.ValueOrNull( mainAxis, containingBlocksizeForOrthogonalFLow ) ?? 0;

    if( containerForDebugMessages != null && containerForDebugMessages.showDebugMessages ) Debug.Log( "[" + containerForDebugMessages.name + "] child-base = " + (rawBaseLength + addPadding + addMargin) + " [" + child.name + "]. Raw: " + rawBaseLength + " + " + addPadding + " padding + " + addMargin + " margin" );
    return rawBaseLength + addPadding + addMargin;
   }
   finally
   {
    samplerMainLengthBase.End();
   }
  }
  
  /**
   * This method is used under the assumption it conforms to:
   *    Spec: 9.4.7: https://www.w3.org/TR/css-flexbox-1/#algo-cross-item
   *
   * NB: the official spec here is wrong, and uses an undefined term "fit-content". The browser manufacturers
   * have implemented "fit-content" as: "fitContent( X ) == min( [maximum available], max( [minimum available], X )",
   * but this is not mandated by the spec.
   * 
   * Base: CROSS length = try using the default width/height; if no width/height specified, then recursively layout
   * children as much as needed to work out the total CONTENT SIZE, and then return the CROSS axis of that.
   *
   * (This is the same as Base MAIN length, except that instead of trying FlexBasis first, and only using width/height
   * if that is AUTO, and only recursing if AUTO-without-width/height, or if CONTENT ... this goes straight to the
   * "try AUTO, or if that fails, use CONTENT" part.)
   * 
   * 
   * CONTENT-BOX: base cross-length = fit-content + padding + margin
   * BORDER-BOX: base cross-length = fit-content + margin
   */
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
  private float Base_CrossLength(FlexItem child, Axis crossAxis, Vector2 containingBlocksizeForOrthogonalFLow, Vector2? preCalcedContentSizeBothAxes, FlexContainer containerForDebugMessages = null )
  #else
  private float Base_CrossLength(FlexItem child, Axis crossAxis, Vector2 containingBlocksizeForOrthogonalFLow, FlexContainer containerForDebugMessages = null )
#endif
  {
   samplerCrossLengthBase.Begin();
   try
   {
    float rawCrossLength;

#if LITE_VERSION // doesn't include ASPECT_FIT
#else
#if PROPRIETARY_ASPECT_FLEXBASIS // ASPECT_FIT is not part of CSS spec, so you may want to remove it for fuller compatibility
    if( child.flexBasis.mode == FlexBasis.ASPECT_FIT
     // && (containerInnerSize.x.HasValue
     //|| containerInnerSize.y.HasValue) 
    )
    {
     AspectFit_Extrapolate( new VectorNullable2( containingBlocksizeForOrthogonalFLow ), child.flexBasis.value, out float containerRatio, out float containerWidth, out float containerHeight );

     if( child.flexBasis.value < containerRatio )
     {
      if( crossAxis == Axis.Vertical )
       rawCrossLength = containerHeight;
      else
       rawCrossLength = child.flexBasis.value * containerHeight;
     }
     else
     {
      if( crossAxis == Axis.Vertical )
       rawCrossLength = containerWidth / child.flexBasis.value;
      else
       rawCrossLength = containerWidth;
     }
    }
    else
#endif
#endif
    {
     /**
      * "performing layout with the used main size and the available space"
      */
     float? attemptResolveAuto = ResolveAutoBasis( child, crossAxis, containingBlocksizeForOrthogonalFLow );

     if( attemptResolveAuto != null )
     {
      if( containerForDebugMessages != null && containerForDebugMessages.showDebugMessages ) Debug.Log( "[" + containerForDebugMessages.name + "] child: " + child.name + "-CrossLength (base value) RESOLVED_AUTO as = " + attemptResolveAuto.Value );
      rawCrossLength = attemptResolveAuto.Value;
     }
     /**
      * "treating auto as fit-content." <-- i.e. if no explicit values given already, attempt to fit-content
      */
     else
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
      rawCrossLength = Base_ContentSize_OnAxis( child, containingBlocksizeForOrthogonalFLow, crossAxis, out Vector2? DISCARD, preCalcedContentSizeBothAxes );
     #else
     rawCrossLength = Base_ContentSize_OnAxis( child, containingBlocksizeForOrthogonalFLow, crossAxis );
#endif
    }

    /**
     * Firefox in 2020 does this:
     *   - for CONTENTBOX, the item is laid out as "padding + flexBasis"
     *   - for BORDERBOX, the item is laid out as "flexBasis"
     *   - for [all modes], the item is laid out as "[above] + margin"
     */
    float addPadding = child.boxSizing == BoxSizing.CONTENT_BOX ? 0 : child.cssPadding.ValueOrNull( crossAxis, containingBlocksizeForOrthogonalFLow ) ?? 0;
    float addMargin = child.cssMargins.ValueOrNull( crossAxis, containingBlocksizeForOrthogonalFLow ) ?? 0;

    return rawCrossLength + addPadding + addMargin;
   }
   finally
   {
    samplerCrossLengthBase.End();
   }
  }

  private float CalculateSpaceForGrowShrinkLine( FlexLayoutData d, float totalOuterHypotheticalMainLength )
  {
   float leftoverSpace = 0f;
   
   float mainLengthInContainerAvailableForChildContent = d.axisMain.Length(d.innerContentSize);
   leftoverSpace = mainLengthInContainerAvailableForChildContent - totalOuterHypotheticalMainLength /** NB: this includes all margins (and some padding)*/;
   
   return leftoverSpace;
  }

  private List<List<FlexItem>> CalculateFlexLines( FlexWrap wrapMode, float? containersInnerCrossSize, List<FlexItem> childItems, float? availableLineLength, Dictionary<FlexItem,float> childHypotheticalMainLengths, Dictionary<FlexItem,float> childHypotheticalCrossLengths, out Dictionary<List<FlexItem>, float> unusedMainSpacePerLine, out Dictionary<List<FlexItem>,float> lineBaseCrossLengths, FlexContainer debuggableContainer = null )
  {
   samplerFlexLines.Begin();
   try
   {
    List<List<FlexItem>> flexLines = new List<List<FlexItem>>();
    unusedMainSpacePerLine = new Dictionary<List<FlexItem>, float>();
    lineBaseCrossLengths = new Dictionary<List<FlexItem>, float>();

    switch( wrapMode )
    {
     case FlexWrap.NOWRAP:
     {
      List<FlexItem> line0 = new List<FlexItem>( childItems );
      flexLines.Add( line0 );

      if( availableLineLength != null )
      {
       /** have to add-up everything on the line, sadly ... */
       float usedMainLength = 0;
       foreach( var child in childItems )
        usedMainLength += childHypotheticalMainLengths[child];
       unusedMainSpacePerLine[line0] = availableLineLength.Value - usedMainLength;
      }
      else
       unusedMainSpacePerLine[line0] = 0; /** in NOWRAP, we assume there's always zero space spare if the parent had no specific size - i.e. we used all of it */

      /** calculate base cross-length for line */

      // TODO: add special handling for Spec 9.4.8.1 double-auto-margin children: "whose cross-axis margins are both non-auto"

      /**
       * Spec: 9.4.8.1: https://www.w3.org/TR/css-flexbox-1/#algo-cross-line
       *
       * For single-line scenarios, where the container size is DEFINITE, then do NOT look at the child sizes, instead use:
       *
       *   "cross size of the flex line is the flex container’s inner cross size."
       *
       * Otherwise, do as for multi-line scenarios:
       *
       *   "
       */
      if( containersInnerCrossSize.HasValue )
       lineBaseCrossLengths[line0] = containersInnerCrossSize.Value;
      else
      {
       lineBaseCrossLengths[line0] = 0;
       foreach( var item in childItems )
       {
        /**
         * Spec: 9.3.5: https://www.w3.org/TR/css-flexbox-1/#algo-line-break
         *
         * "For this step, the size of a flex item is its outer hypothetical main size. (Note: This can be negative.)"
         *
         * NB: we are only concerned with the main-lenth, not cross-length, because wrap only applies to main-axis in flexbox
         */
        lineBaseCrossLengths[line0] = Math.Max( lineBaseCrossLengths[line0], childHypotheticalCrossLengths[item] );
       }
      }

      if( debuggableContainer && debuggableContainer.showDebugMessages )
       Debug.Log( "[" + debuggableContainer.name + "].CalcFlexLines: NOWRAP, available length = " + availableLineLength + ", single line of cross-length = " + lineBaseCrossLengths[line0] );
     }
      break;

     case FlexWrap.WRAP:
     {
      List<FlexItem> currentLine = new List<FlexItem>();
      flexLines.Add( currentLine );
      /** in WRAP, we assume there's always infinite space if the parent had no specific size */
      float? spaceAvailableOnLine = availableLineLength; // technically identical to : CalculateSpaceForGrowShrinkLine( ..., 0 )

      // TODO: add special handling for Spec 9.4.8.1 double-auto-margin children: "whose cross-axis margins are both non-auto"
      float biggestCrossLengthInLine = 0f;

      foreach( var item in childItems )
      {
       /**
        * Spec: 9.3.5: https://www.w3.org/TR/css-flexbox-1/#algo-line-break
        *
        * "For this step, the size of a flex item is its outer hypothetical main size. (Note: This can be negative.)"
        *
        * NB: we are only concerned with the main-lenth, not cross-length, because wrap only applies to main-axis in flexbox
        */
       float hypotheticalMainLength = childHypotheticalMainLengths[item]; // note that we already included PADDING and MARGIN in the MainLengths calc above, so this is all we need

       if( spaceAvailableOnLine == null /** i.e. infinite */
           || hypotheticalMainLength <= spaceAvailableOnLine
           || currentLine.Count < 1 ) /** Spec says: "	If the very first uncollected item wouldn’t fit, collect just it into the line. " */
       {
        currentLine.Add( item );
        /** calculate base cross-length for line */
        biggestCrossLengthInLine = Math.Max( biggestCrossLengthInLine, childHypotheticalCrossLengths[item] );
       }
       else
       {
        unusedMainSpacePerLine[currentLine] = spaceAvailableOnLine ?? 0f;
        lineBaseCrossLengths[currentLine] = biggestCrossLengthInLine;

        currentLine = new List<FlexItem>();
        flexLines.Add( currentLine );
        currentLine.Add( item );
        /** calculate base cross-length for line */
        biggestCrossLengthInLine = childHypotheticalCrossLengths[item];

        if( spaceAvailableOnLine != null )
         spaceAvailableOnLine = availableLineLength; // technically identical to : CalculateSpaceForGrowShrinkLine( ..., 0 )
       }

       if( spaceAvailableOnLine != null )
        spaceAvailableOnLine -= hypotheticalMainLength;
      }

      /** Store the unused space for the final, open-ended, line */
      unusedMainSpacePerLine[currentLine] = spaceAvailableOnLine ?? 0f;
      lineBaseCrossLengths[currentLine] = biggestCrossLengthInLine;
     }
      break;
    }

    return flexLines;
   }
   finally
   {
    samplerFlexLines.End();
   }
  }

  /**
   * Main method for calculating FlexItem *SIZES*
   *
   * (called by the layout algorithm internally, before it does the calculation of *POSITIONS*)
   */
  private Dictionary<FlexItem, Vector2> CalculateAndSetAllChildSizes( FlexLayoutData d, FlexContainer container,
   out Dictionary<List<FlexItem>, float> usedMainSpacePerLine,
   out Dictionary<List<FlexItem>, float> unusedMainSpacePerLine,
   out List<List<FlexItem>> flexLines,
   out Dictionary<List<FlexItem>, float> flexLineBaseCrossLengths
   )
  {
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Performing ResolveChildren baseLengths ...");

   /**
    * Spec: 9.2.3: https://www.w3.org/TR/css-flexbox-1/#algo-main-item
    * 
    * Step 1a: Base main lengths
    *
    * CONTENT-BOX: base-length = flexBasis + padding + margin
    * BORDER-BOX: base-length = flexBasis + margin
    *
    */
   Dictionary<FlexItem, float> childBaseMainLengths = new Dictionary<FlexItem, float>();
   Dictionary<FlexItem, Vector2?> _partiallyCalcedBaseContentSizes = new Dictionary<FlexItem, Vector2?>();
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Calculating sizes for -- " + d.childItems.Count + " -- child-items");
   foreach( var child in d.childItems )
   {
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
    childBaseMainLengths[child] = Base_MainLength(child, d.axisMain, d.innerContentSize, out Vector2? baseSizeBothAxes, container );
    _partiallyCalcedBaseContentSizes[child] = baseSizeBothAxes;
    #else
    childBaseMainLengths[child] = Base_MainLength(child, d.axisMain, d.innerContentSize, container );
#endif
    //if( container.showDebugMessages ) Debug.Log("[" + container.name + "] child: " + child.name + "-BaseLength = " + childBaseMainLengths[child]);
   }

   /**
    * Spec: 9.2.3: https://www.w3.org/TR/css-flexbox-1/#algo-main-item
    * 
    * Step 1b: hypothetical main lengths
    *
    *  Hypothetical lengths = base-length constrained by minWidth/maxWidth and minHeight/maxHeight
    *
    * Step 2: Calculate total of all children's hypothetical-main-lengths
    *    (MAY include padding for SOME children depending upon their box-sizing settings) 
    */
   Dictionary<FlexItem, float> childHypotheticalMainLengths = new Dictionary<FlexItem, float>();
   foreach( var child in d.childItems )
   {
    if( !childBaseMainLengths.ContainsKey(child) )
     Debug.LogError("Impossible: 235234l23kj");

    //ULTRA debug:if( container.showDebugMessages ) Debug.Log("child [" + child + "] y adding to totalHypotheticalLength (child's baseLength = " + baseSizes[child] + ") : " + child.HypotheticalLength(container.direction, baseSizes[child], parentSizeOrNull));
    childHypotheticalMainLengths[child] = HypotheticalMainLength(child, container.direction, childBaseMainLengths[child], new VectorNullable2(d.innerContentSize));
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "] child: " + child.name + "-HypotheticalMainLength = " + childHypotheticalMainLengths[child]);
   }

   /**
    * Spec: 9.4.7: https://www.w3.org/TR/css-flexbox-1/#algo-cross-item
    * 
    * Step 4a: base cross lengths
    */
   Dictionary<FlexItem, float> childBaseCrossLengths = new Dictionary<FlexItem, float>();
   foreach( var child in d.childItems )
   {
#if MAIN_AUTOSIZE_CACHES_FOR_CROSS_AUTOSIZE
    childBaseCrossLengths[child] = Base_CrossLength(child, d.axisCross, d.innerContentSize, _partiallyCalcedBaseContentSizes[child], container );
    #else
    childBaseCrossLengths[child] = Base_CrossLength(child, d.axisCross, d.innerContentSize, container );
#endif
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "] child: " + child.name + "-CrossLength (base value) = " + childBaseCrossLengths[child]);
   }

   /**
   * Spec: 9.4.7: https://www.w3.org/TR/css-flexbox-1/#algo-cross-item
   * 
   * Step 4b: hypothetical cross lengths
   *
   *  Hypothetical lengths = base-length constrained by minWidth/maxWidth and minHeight/maxHeight
   *
   * Spec: 9.4.8: https://www.w3.org/TR/css-flexbox-1/#algo-cross-line
   * 
   * Step 5: Calculate largest of all children's hypothetical-cross-lengths
   *    (MAY include padding for SOME children depending upon their box-sizing settings) 
   */
   Dictionary<FlexItem, float> childHypotheticalCrossLengths = new Dictionary<FlexItem, float>();
   foreach( var child in d.childItems )
   {
    if( !childBaseCrossLengths.ContainsKey(child) )
     Debug.LogError("Impossible: j324kjlsdf");

    //ULTRA debug:if( container.showDebugMessages ) Debug.Log("child [" + child + "] y adding to totalHypotheticalLength (child's baseLength = " + baseSizes[child] + ") : " + child.HypotheticalLength(container.direction, baseSizes[child], parentSizeOrNull));
    childHypotheticalCrossLengths[child] = HypotheticalCrossLength(child, container.direction, childBaseCrossLengths[child], new VectorNullable2(d.innerContentSize));
   }

   /**
    * Spec: 9.3: https://www.w3.org/TR/css-flexbox-1/#main-sizing
    *
    * Step 3a: calculate flex-lines
    *
    *    (moved to a separate method since it also needs to be called recursively when calculating child sizes of CONTENT parents)
    */
   /** Calculate flex lines: will be precisely 1 if NOWRAP, and 1 or more if WRAP */
   flexLines = CalculateFlexLines(container.wrap, d.axisCross.Length(d.innerContentSize), d.childItems, d.axisMain.Length(d.innerContentSize), childHypotheticalMainLengths, childHypotheticalCrossLengths, out unusedMainSpacePerLine, out flexLineBaseCrossLengths, container);

   /** TODO: DEBUG: delete this
   StringBuilder sb = new StringBuilder();
   for( int i = 0; i < d.childItems.Count; i++ )
    sb.AppendLine(i + " : " + d.childItems[i].hierarchicalName);
   Debug.Log("ALL ITEMS: "+sb);
   sb.Clear();
   for( int i = 0; i < flexLines.Count; i++ )
   {
    for( int k = 0; k < flexLines[i].Count; k++ )
    {
     sb.AppendLine( k+ " : " + flexLines[i][k].hierarchicalName);
    }
    Debug.Log("LINE "+i+": "+sb);
    sb.Clear();
   }
   */

   Dictionary<FlexItem, float> grownAndShrunkDeltaLengths = new Dictionary<FlexItem, float>();
   
   for( int flexLineIndex = 0; flexLineIndex < flexLines.Count; flexLineIndex++ )
   {
    List<FlexItem> flexLine = flexLines[flexLineIndex];

    /**
    *
    *
    * Theoretically, we now possess ALL the information needed to set self.size, if self is in basis = CONTENT (and was
    * therefore unsizable until now).
    *
    *
    */



    /**
     * Step 6: Grow-or-Shrink sizes
     */
    float leftoverSpace = unusedMainSpacePerLine[flexLine];

    if( container.showDebugMessages ) Debug.Log("[" + container.name + "] leftoverspace = " + leftoverSpace + " will ... " + (leftoverSpace < 0 ? "SHRINK" : (leftoverSpace > 0 ? "GROW" : "DO NOTHING")));

    if( leftoverSpace > 0 )
    {
     var extraGrownShrunkLengths = ResolveGrowSizes(flexLine, leftoverSpace, childBaseMainLengths, d.axisMain, d.axisMain.Length(d.innerContentSize), out var unusedMainSpace); // NOTE: leftoverspace was pre-calc'd using HYPOTHETICAL lengths, but grow/shrink only uses the BASE ones
     foreach( var row in extraGrownShrunkLengths ) /** C# sucks and has no simple direct "merge two dicts" */
      grownAndShrunkDeltaLengths[row.Key] = row.Value;
     unusedMainSpacePerLine[flexLine] = unusedMainSpace; // update it, it may now be zero depending on grow settings
    }
    else if( leftoverSpace < 0 )
    {
     var extraGrownShrunkLengths = ResolveShrinkSizes(flexLine, -1f * leftoverSpace, childBaseMainLengths, d.axisMain, d.innerContentSize); // NOTE: leftoverspace was pre-calc'd using HYPOTHETICAL lengths, but grow/shrink only uses the BASE ones
     foreach( var row in extraGrownShrunkLengths ) /** C# sucks and has no simple direct "merge two dicts" */
      grownAndShrunkDeltaLengths[row.Key] = row.Value;
     unusedMainSpacePerLine[flexLine] = 0; // update it, it is now definitely zero
    }
    else
    {
     foreach( var child in flexLine )
      grownAndShrunkDeltaLengths[child] = 0;
    }
   }

   /**
    * Spec: 9.7.5: https://www.w3.org/TR/css-flexbox-1/#resolve-flexible-lengths (SPEC has a bug, it doesn't hotlink the sub-bullets here)
    *
    * Step 7a: Set the final main-sizes using the output of grown-and-shrunk sizing
    * (almost all sizing is now complete)
    *
    * Note that the deltas of grow/shrink are to be applied to the INNER main/cross (excluding margin, and for CONTENT also excluding padding)
    */
   Dictionary<FlexItem, float> finalLengths = new Dictionary<FlexItem, float>();
   foreach( var child in d.childItems )
   {
    finalLengths[child] = childHypotheticalMainLengths[child] + grownAndShrunkDeltaLengths[child];
   }
   
   /**
    * Spec: 9.3.6:
    *
    * Step 7b: Use the final main-sizes to calculate the FINAL main-length of each line (which could be longer than flex-wrap expected, if the line is overflowing!)
    * 
    */
   usedMainSpacePerLine = new Dictionary<List<FlexItem>, float>();
   for( int flexLineIndex = 0; flexLineIndex < flexLines.Count; flexLineIndex++ )
   {
    List<FlexItem> flexLine = flexLines[flexLineIndex];

    float totalMainLengthOfLine = 0f;
    foreach( var child in flexLine )
     totalMainLengthOfLine += finalLengths[child];
    usedMainSpacePerLine[flexLine] = totalMainLengthOfLine;
   }

   /**
    * Spec: 9.4.8.0 (un-numbered)
    *
    * "If the flex container is single-line and has a definite cross size, the cross size of the flex line is the flex container’s inner cross size."
    *
    * NB: d.innerContentSize is always defined, because we inherit whatever size was given to us by outer UnityUI/RectTransform/Canvas,
    * and each inner-item then inherits whatever result happened from the previous bits of layout done by its parents that dictated its
    * final size.
    *
    * ... so, that's never undefined, and to check for "is it definite", we have to manually do the standard CSS check, using ResolveDefiniteSize.
    */
   if( flexLines.Count == 1
       && d.containerAsItem != null
       && ResolveDefiniteSize( d.containerAsItem, d.axisCross, d.axisCross.Length(d.innerContentSize)) != null )
   {
    float newCross = ResolveDefiniteSize(d.containerAsItem, d.axisCross, d.axisCross.Length(d.innerContentSize)).Value;

    if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: Overriding cross-axis height for singleline with new = "+newCross );
    
    flexLineBaseCrossLengths[flexLines[0]] = newCross;
   }
   else
   {
    /**
     * ... else: Spec: 9.4.8.1, 9.4.8.2, 9.4.8.3:
     *
     * These were already handled in the CalculateFlexLines(..) call that happened earlier
     */

    /**
     * Spec 9.4.8.3:
     *
     * If there is only one flex-line, then clamp all cross-sizes to be within the container’s computed min and max cross sizes.
     */
    if( flexLines.Count == 1 )
    {
     float? minCross = null, maxCross = null;
     if( d.containerAsItem != null )
     {
      ResolveAutoBasis( d.containerAsItem, d.axisCross, d.containersParentSize );

      CSS3Length min, max;
      switch( d.axisCross )
      {
       case Axis.Horizontal:
        min = d.containerAsItem.cssMinWidth;
        max = d.containerAsItem.cssMaxWidth;
        break;

       case Axis.Vertical:
        min = d.containerAsItem.cssMinHeight;
        max = d.containerAsItem.cssMaxHeight;
        break;

       default:
        throw new Exception( "Inconceivable. C# compiler sucks. 3523ljksdflkjsz" );
      }

      minCross = min.ValueOrNull( d.axisCross.Length( d.containersParentSize ) );
      maxCross = max.ValueOrNull( d.axisCross.Length( d.containersParentSize ) );
     }

     if( (minCross.HasValue || maxCross.HasValue) )
     {
      if( container.showDebugMessages ) Debug.Log( "[" + container.name + "]: Clamping cross-axis height for singleline with min = " + minCross + ", max = " + maxCross );
     }

     float lineNewCross = flexLineBaseCrossLengths[flexLines[0]];
     if( minCross != null )
      lineNewCross = Mathf.Max( minCross.Value, lineNewCross );
     if( maxCross != null )
      lineNewCross = Mathf.Min( maxCross.Value, lineNewCross );
     flexLineBaseCrossLengths[flexLines[0]] = lineNewCross;
    }
   }

   
  if( container.alignItems == AlignItems.STRETCH )
   {
    // TODO: add support for child having alignSelf=STRETCH
    /**
     * STRETCH ONLY
     * 
     * Spec: 9.4.9: export
     *
     * Increase the size of each flex-line's height equally, if their initial total height is less than the available height,
     * and the align-content was STRETCH
     */ 
    float availableCrossHeight = d.axisCross.Length(d.innerContentSize);
    for( int flexLineIndex = 0; flexLineIndex < flexLines.Count; flexLineIndex++ )
    {
     List<FlexItem> flexLine = flexLines[flexLineIndex];
     float lineCrossHeight = flexLineBaseCrossLengths[flexLine];
     availableCrossHeight -= lineCrossHeight;
    }

    if( availableCrossHeight > 0 )
    {
     if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: Spare cross-axis height = " + availableCrossHeight + ", distributing equally among " + flexLines.Count + " lines");
     
     for( int flexLineIndex = 0; flexLineIndex < flexLines.Count; flexLineIndex++ )
     {
      List<FlexItem> flexLine = flexLines[flexLineIndex];
      flexLineBaseCrossLengths[flexLine] += availableCrossHeight / (float) flexLines.Count;
      if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: .. line-"+flexLineIndex+" now has new cross-height = "+flexLineBaseCrossLengths[flexLine] );
     }
    }
    
    //#if UNIT_TEST_FAILS_YET
    /**
     * STRETCH ONLY
     * 
    * Spec: 9.4.11: https://www.w3.org/TR/css-flexbox-1/#algo-stretch
    *
    * If neither margin is AUTO // TODO: Auto-margins currently not supported/implemented
    * ...cross size is the used cross size of its flex line, clamped according to the item’s used min and max cross sizes
    *
    */
    for( int flexLineIndex = 0; flexLineIndex < flexLines.Count; flexLineIndex++ )
    {
     List<FlexItem> flexLine = flexLines[flexLineIndex];
     float lineCross = flexLineBaseCrossLengths[flexLine];
     foreach( var child in flexLine )
     {
      if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: Expanding CROSS-length of stretch-child = "+child+ " in line "+flexLineIndex+" to line-cross = "+lineCross );
       childHypotheticalCrossLengths[child] = CSS3Length.Clamp( child.cssMinWidth, child.cssMaxWidth, lineCross, d.axisCross.Length( d.containersParentSize ) );
     }
    }
//#endif
   }

  /*********************
   * MAJOR BUG IN THE CSS OFFICIAL SPECIFICATION
   *
   * 9.4.x, specifically: 9.4.11, fails to EVER clamp the cross-sizes to the flexline sizes.
   *
   * This is ridiculous (but it's how the section is written), and NO BROWSER VENDOR HAS IMPLEMENTED what the Spec describes, they all
   * did the more sensible thing of re-calculating the cross-sizes using the new flexline values, or, possibly,
   * of clamping the cross-sizes.
   *
   * NB: the whole of section 9.4.x is lazily written, and is embarassing. It includes undefined terms (such as "fit-content")
   * is unclear, imprecise, and incorrect.
   *
   * So, here's my guess, based on browser vendors implementations:
   *
   * For each item in each line
   * 1. If it has a cross-size GREATER than its line-size ...
   * 2. ... and it DOESN'T have a definite cross-size (i.e. it was laid-out using fit-content)
   * 3.    ... then: clamp it to (whatever is left after subtracting its margins)
   * 4.    ... and floor that at 0.
   *
   * In practice, since margins are included in the cross-sizes, we simply set the new child-cross
   *  to be equal to the line-cross, and leave the final-size-setting code to subtract margins and floor-0 as needed.
   * 
   */
  for( int flexLineIndex = 0; flexLineIndex < flexLines.Count; flexLineIndex++ )
  {
   List<FlexItem> flexLine = flexLines[flexLineIndex];
   float lineCross = flexLineBaseCrossLengths[flexLine];
   
   foreach( var child in flexLine )
   {
    float childCross = childHypotheticalCrossLengths[child];
    if( childCross > lineCross
        && ResolveDefiniteSize(child, d.axisCross, lineCross) == null )
    {
     if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: Clamping CROSS-length of over-sized child that was cross-sized using fit-content = "+child+ " in line "+flexLineIndex+" to line-cross = "+lineCross );
     childHypotheticalCrossLengths[child] = lineCross;
    }
   }
  }
  
   /**
    * Spec: 9.7.5: https://www.w3.org/TR/css-flexbox-1/#resolve-flexible-lengths (SPEC has a bug, it doesn't hotlink the sub-bullets here)
    *
    * Step 7b: Set the final cross-sizes using the output of grown-and-shrunk sizing
    * (almost all sizing is now complete)
    *
    * Note that the deltas of grow/shrink are to be applied to the INNER main/cross (excluding margin, and for CONTENT also excluding padding)
    */
   Dictionary<FlexItem, Vector2> finalSizes = new Dictionary<FlexItem, Vector2>();
   for( int flexLineIndex = 0; flexLineIndex < flexLines.Count; flexLineIndex++ )
   {
    List<FlexItem> flexLine = flexLines[flexLineIndex];
    Vector2 lineInnerContentSize = d.innerContentSize; // TODO: fix this!
    foreach( var child in flexLine )
    {
     Vector2 newSize;
     float mainLength = finalLengths[child];
     float crossLength = childHypotheticalCrossLengths[child];
     if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: line=" + flexLineIndex + ", child (" + child + ") initially has main, cross = (" + mainLength + ", " + crossLength + ")"); 

     /** Special alteration: in STRETCH mode, all 'indefinite' cross-lengths get overridden by the container's cross-length available, and their margins pre-subtracted */
     if( container.alignItems == AlignItems.STRETCH )
     {
      //TODO: if containerAsItem is set to CONTENT, use: crossLength = largestHypotheticalCrossLength;
      // TODO: only apply this when the crosslength was already indefinite
      if( !child.cssDefault(d.axisCross).hasValue
#if LITE_VERSION
#else
          && child.flexBasis.mode != FlexBasis.ASPECT_FIT
#endif
      )
      {
       crossLength = flexLineBaseCrossLengths[flexLine]; // TODO: only apply this when the crosslength was already indefinite

       /** re-apply any min/max width/height constraints EVEN AFTER stretching (stretching gets overridden by min/max in CSS3 in Firefox in 2020) */
       crossLength = child.cssClampLength(crossLength, d.axisCross, lineInnerContentSize );
      }
     }
//too verbose, duplicated from what we've already logged     if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: Setting cross-axis for line = "+flexLineIndex+" of flexbox (" + container.name + ") = "+crossLength );

     /**
      * Subtract the margins before calling setSize
      * 
      * (margins are included in all flex layout, but the actual RectTransforms should not include them)
      */
     if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: line="+flexLineIndex+", shrinking child ("+child+") by main margins = "+child.cssMargins.ValueOrZero(d.axisMain, lineInnerContentSize)+", crossMargins = "+child.cssMargins.ValueOrZero(d.axisCross, lineInnerContentSize) );
     mainLength -= child.cssMargins.ValueOrZero(d.axisMain, lineInnerContentSize);
     crossLength -= child.cssMargins.ValueOrZero(d.axisCross, lineInnerContentSize);

     mainLength = Mathf.Max(0, mainLength);
     crossLength = Mathf.Max(0, crossLength);

     newSize = (d.axisMain == Axis.Horizontal)
      ? new Vector2(mainLength, crossLength)
      : new Vector2(crossLength, mainLength);

     finalSizes[child] = newSize;
    }
   }


   /** ---- TODO: re-implement ASPECT_FIT (maybe even merge it better with the official CSS3 spec) */

   /***** Update all the child's RectTransform.size all in one go *****/
#if UNITY_EDITOR
#else
   if( Application.isEditor )
   {
    //Debug.Log("UnityEditor core bug (please complain to Unity!): you are about to get your console spammed with fake WARNING messages generated by a Unity employee who incorrectly added them in Unity 2017, and failed to patch them out again. With the source-code version of this plugin, there is a hack that can workaround this UnityEngine bug, but it is impossible in compiled code.");
   }
#endif
   //if( false ) // TODO: this is disabling all resizing!
   foreach( var child in d.childItems )
   {
    var childTransform = child.transform as RectTransform;

    if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: --- Final size for child: [["+child.name+"]] = "+finalSizes[child]+" (was:"+childTransform.rect.size+")" );
    
    childTransform.SetSize(finalSizes[child]); // recap: if CONTENT_BOX, this is "base + padding", otherwise it's just "base"

    /** Update the anchors to sensible values, allowing us to implement positioning with a sensible amount of code instead of a huge number of switches and extra unnecessary calcs */
    childTransform.anchoredPosition = Vector2.zero;
    childTransform.anchorMax = childTransform.anchorMin = 0.5f * Vector2.one; // cleans up the anchors and puts them somewhere more sensible than Unity's HorizontalLayoutGroup which sticks them at 0,0
   }

   return finalSizes;
  }

  /**
   * @param preAdvance - the pixels of space to insert before EACH item
   * @param postAdvance - the pixels of space to insert after EACH item
   */
  private void CalculateAdvances( int numChildItems, FlexContainer container, float unusedMainSpace, out float preAdvance, out float postAdvance)
  {
   switch( container.justifyContent )
   {
    case FlexJustify.SPACE_BETWEEN:
     preAdvance = (numChildItems < 2) ? 0.5f * unusedMainSpace : 0f;
     postAdvance = (numChildItems < 2) ? 0 : unusedMainSpace / (numChildItems - 1); // final postAdvance is ignored anyway
     break;
    case FlexJustify.SPACE_AROUND:
     postAdvance = 0.5f * unusedMainSpace / numChildItems;
     preAdvance = postAdvance;
     break;
    case FlexJustify.SPACE_EVENLY:
     preAdvance = unusedMainSpace / (numChildItems + 1);
     postAdvance = 0f; // we ignore it, technically there's a postadvance on the last item but it's not needed, already accounted for 
     break;
    
    /** These three all DO NOT distribute space - they shunt all together, and then change the GLOBAL preadvance */
    case FlexJustify.START:
    case FlexJustify.CENTER:
    case FlexJustify.END:
    default: // only because C# compiler sucks and requires it
     preAdvance = postAdvance = 0;
     break;
   }
  }

  private static Dictionary<FlexItem, float> ResolveGrowSizes( List<FlexItem> items, float excessToFill, Dictionary<FlexItem, float> baseLengths, Axis mainAxis, float maxMainAxisLength, out float unusedSpace )
  {
   Dictionary<FlexItem, float> growDeltas = new Dictionary<FlexItem, float>();

   //TODO:  Debug.LogError("Grow is ignoring the min-height: adding a min-height bigger than the assigned height, in a place where some Grow is possible, is causing the height-based Spare space to be allocated, instead of the height-modified-by-minheight-based Spare space");

   /** Allocate all remaining space as padding around items, proportional to flex-grow */
   float totalGrowWeight = 0;
   foreach( var child in items )
   {
    totalGrowWeight += Mathf.Max(0f, child.flexGrow);
    growDeltas[child] = 0;
   }

   if( totalGrowWeight <= 0f ) // all items refused to grow
   {
    unusedSpace = excessToFill;
    return growDeltas;
   }

   /** Re-allocate any excesses that go above a child's max (width|height) */
   float allocatingExcess = excessToFill;
   float unusedExcess = 0;
   List<FlexItem> growingChildren = new List<FlexItem>(items);
   /** ... looping until either all excess has found a home, or all children have hit their MAX width|height limits */
   while( growingChildren.Count > 0 && allocatingExcess > 0 )
   {
    for( int i = 0; i < growingChildren.Count; i++ )
    {
     var child = growingChildren[i];
     float fractionToConsume = Mathf.Max(0f, child.flexGrow) / totalGrowWeight;
     float offeredExcess = fractionToConsume * allocatingExcess;
     float acceptedExcess;
     
     /**
      * TODO: CHECK if this is correct: this implementation is much simpler than the CSS spec for flexbox, yet appears to converge on same end values? 
      */
     CSS3Length cssMax = child.cssMax( mainAxis );
       if( cssMax.mode == CSS3LengthType.NONE )
        acceptedExcess = offeredExcess;
       else
       {
        float? maxAllowedLength = cssMax.ValueOrNull( maxMainAxisLength );
        float baseLength = baseLengths[child];
        acceptedExcess = maxAllowedLength == null ? offeredExcess : Mathf.Min(Mathf.Max( 0, (maxAllowedLength.Value - baseLength)), offeredExcess);
       }
       
       growDeltas[child] += acceptedExcess;
       
     //ULTRA debug: if( container.showDebugMessages ) Debug.Log("[" + this.name + "." + child.name + "]: grow, excess offered (frac:" + fractionToConsume + ") = " + offeredExcess + ", accepted = " + acceptedExcess + ", max-(w,h) = " + child.maxWidth + ", " + child.maxHeight); 

     if( acceptedExcess < offeredExcess ) //if we hit a "max violation" as spec calls it, we return some of the excess, and "freeze" (spec's wording) the flexitem
     {
      unusedExcess += (offeredExcess - acceptedExcess);
      growingChildren.RemoveAt(i);
      i--; // because we just removed one
     }
    }

    allocatingExcess = unusedExcess; // unusedExcess defaults to 0, and only gets incremented if 1 or more items couldn't accept the full space allocated to them, and had to return some
    unusedExcess = 0;
   }

   unusedSpace = allocatingExcess; // maybe all items hit their MAX size, or maybe none of them are set to GROW, or ... etc
   return growDeltas;
  }
  
  /** Note that Shrink calculations are different from Grow:
   *
   * Grow only looks at the weights in the items,
   * Shrink does looks at weights AND ALSO premultiplies by the relative INNER baseSizes!
   * 
   * Spec: 9.7.4.c: https://www.w3.org/TR/css-flexbox-1/#resolve-flexible-lengths and https://www.w3.org/TR/css-sizing-3/#inner-size
   */
  private static float _ScaledShrinkFactor(FlexItem child, float baseLength, Axis axis, Vector2? containerInnerSize )
  {
   float childShrinkWeight = Mathf.Max(0f, child.flexShrink);
   
   float subtractPadding = child.cssPadding.ValueOrNull( axis, containerInnerSize) ?? 0;
   float subtractMargin = child.cssMargins.ValueOrNull( axis, containerInnerSize) ?? 0;

   return childShrinkWeight * ( baseLength - (subtractMargin + subtractPadding) );
  }
  
  /**
   * The Official CSS3 Flexbox Algorithm Specification is strange.
   *
   * It takes items that have a size and a smaller MAX size, and when shrinking it starts with them at their SIZE, and
   * then shrinks them down proportionally with other items in the line. (Spec: 9.7.2 : https://www.w3.org/TR/css-flexbox-1/#resolve-flexible-lengths )
   *
   * This seems ... wrong. The net effect is that there will be empty space AFTER shrinking (Because the actual
   * size used for the "item that was bigger than its own MAX size" will be the MAX size).
   *
   * Either way, it's very badly worded, so ... WE IGNORE THE SPECIFICATION HERE AND IMPLEMENT AN ALGORITHM THAT MAKES SENSE
   */
  private static Dictionary<FlexItem, float> ResolveShrinkSizes( List<FlexItem> childItems, float deficitToFill, Dictionary<FlexItem, float> baseLengths, Axis axisMain, Vector2 parentInnerContentSize )
  {
   Dictionary<FlexItem, float> growDeltas = new Dictionary<FlexItem, float>();

   // Initialize
   foreach( var child in childItems )
   {
    growDeltas[child] = 0;
   }

   /**
    * SHRINK ONLY: you cannot pre-calculate the grow factors, because the official algorithm requires you to re-calc
    * them on the fly on every loop iteration
    */

   /** Re-allocate any excesses that go above a child's max (width|height) */
   float allocatingDeficit = deficitToFill;
   float unusedDeficit = 0;

   Dictionary<FlexItem, float> scaledShrinkFactors = new Dictionary<FlexItem, float>();
   List<FlexItem> unfrozenItems = new List<FlexItem>(childItems);
   
   /**
    * Pre-freeze items that are already smaller than their acceptable minimum size
    */
   for( int i = 0; i < unfrozenItems.Count; i++ )
   {
    var child = unfrozenItems[i];
    CSS3Length cssMin = child.cssMin(axisMain);
    float? minAllowedLength = cssMin.ValueOrNull( axisMain.Length(parentInnerContentSize));
    if( cssMin.mode != CSS3LengthType.NONE
     && minAllowedLength != null
     && minAllowedLength.Value > baseLengths[child])
    {
     unfrozenItems.RemoveAt(i);
     i--; // because we just removed one
    }
   }
   
   float currentTotalShrinkWeight = 1;
   /** ... looping until either all excess has found a home, or all children have hit their MAX width|height limits */
   while( unfrozenItems.Count > 0 && allocatingDeficit > 0 && currentTotalShrinkWeight > 0 )
   {
    /** Recalculate the total of the scaleShrinkWeights for the items we're still shrinking */
    scaledShrinkFactors.Clear();
    foreach( var child in unfrozenItems )
    {
     float ssf = _ScaledShrinkFactor(child, baseLengths[child], axisMain, parentInnerContentSize);
     scaledShrinkFactors[child] = ssf;
     currentTotalShrinkWeight += Mathf.Max(0f, ssf);
    }

    for( int i = 0; i < unfrozenItems.Count; i++ )
    {
     var child = unfrozenItems[i];
     float fractionToConsume = scaledShrinkFactors[child] / currentTotalShrinkWeight;
     float offeredDeficit = fractionToConsume * allocatingDeficit;
     
     /**
      * TODO: CHECK if this is correct: this implementation is much simpler than the CSS spec for flexbox, yet appears to converge on same end values? 
      */
     float acceptedDeficit;
     CSS3Length cssMin = child.cssMin(axisMain);
     float? minAllowedLength = cssMin.ValueOrNull( axisMain.Length(parentInnerContentSize));
     if( cssMin.mode == CSS3LengthType.NONE || minAllowedLength == null )
      acceptedDeficit = offeredDeficit;
     else
      acceptedDeficit = Mathf.Min((baseLengths[child] - minAllowedLength.Value), offeredDeficit);
     
     /** Clamp to zero */
     acceptedDeficit = Mathf.Min(baseLengths[child], acceptedDeficit);
     
     /** Apply the reduction to the delta */
     growDeltas[child] -= acceptedDeficit;

     //ULTRA debug: if( container.showDebugMessages ) Debug.Log("[" + this.name + "." + child.name + "]: grow, excess offered (frac:" + fractionToConsume + ") = " + offeredExcess + ", accepted = " + acceptedExcess + ", max-(w,h) = " + child.maxWidth + ", " + child.maxHeight); 

     if( acceptedDeficit < offeredDeficit ) //if we hit a "min violation" as spec calls it, we return some of the excess, and "freeze" (spec's wording) the flexitem
     {
      unusedDeficit += (offeredDeficit - acceptedDeficit);
      unfrozenItems.RemoveAt(i);
      i--; // because we just removed one
     }
    }

    allocatingDeficit = unusedDeficit; // unused defaults to 0, and only gets incremented if 1 or more items couldn't accept the full space allocated to them to grow/shrink, and had to return some
    unusedDeficit = 0;
   }

   return growDeltas;
  }

  private static Axis MainAxis(FlexDirection direction)
  {
   switch( direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.COLUMN_REVERSED:
     return Axis.Vertical;
				
    case FlexDirection.ROW:
    case FlexDirection.ROW_REVERSED:
     return Axis.Horizontal;
    
    default:
     throw new Exception("C# Impossible: 24151242132");
   }
  }
  
  private static Axis CrossAxis(FlexDirection direction)
  {
   switch( direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.COLUMN_REVERSED:
     return Axis.Horizontal;
     
    case FlexDirection.ROW:
    case FlexDirection.ROW_REVERSED:
     return Axis.Vertical;
     
    default:
     throw new Exception("C# Impossible: 251242132");
   }
  }

  private static Axis CrossFromMain(Axis mainAxis)
  {
   if( mainAxis == Axis.Vertical )
    return Axis.Horizontal;
   else
    return Axis.Vertical;
  }

  private static float HypotheticalMainLength(FlexItem item, FlexDirection direction, float length, VectorNullable2? parentSize)
  {
   Axis mainAxis = MainAxis(direction);
   return item.cssClampLength(length, mainAxis, parentSize);
  }
  
  private static float HypotheticalCrossLength(FlexItem item, FlexDirection direction, float length, VectorNullable2? parentSize)
  {
   Axis crossAxis = CrossAxis(direction);
   return item.cssClampLength(length, crossAxis, parentSize);
  }
  /*
  private static float HypotheticalCrossLength(FlexItem item, FlexDirection direction, float length, float parentLineCrossLength )
  {
   Axis crossAxis = CrossAxis(direction);
   return CSS3Length.Clamp( item.cssMin(crossAxis), item.cssMax(crossAxis), length, parentLineCrossLength );
  }*/
 }
}
#pragma warning restore 0642
//NB: blank line REQUIRED here by Unity v2020.x ! Major bug in UnityEditor only fixed in 2020.2
