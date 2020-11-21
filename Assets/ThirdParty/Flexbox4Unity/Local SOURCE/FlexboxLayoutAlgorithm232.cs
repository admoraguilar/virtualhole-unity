#define PROPRIETARY_ASPECT_FLEXBASIS // This is not in CSS-3, added specifically for Unity and game-developers

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Axis = UnityEngine.RectTransform.Axis;

namespace Flexbox4Unity
{
 /**
  * LayoutAlgorithm v2.3.2
  *
  * Main changes:
  *  - fixed the flexOrder calculation (was using wrong algorithm)
  *  - added support for more UnityUI classes, laying them out automatically - better than Unity does!
  */
 [CreateAssetMenu(menuName = "Flexbox/Layout Algorithms/v2.3.2", fileName = "Layout Algorithm v2.3.2.asset", order=103)]
 public class FlexboxLayoutAlgorithm232 : IFlexboxLayoutAlgorithm
 {
  public override string defaultAssetName
  {
   get { return "Layout Algorithm v2.3.2"; }
  }

  /** UnityEditor's built-in API for stating if Layout is in progress is broken in all versions up to 2019, so we have to track it manually */
  public override bool isLayoutInProgress
  {
   get { return _layoutsStartedUnfinished > 0; }
  }

  /** Required to auto-reset the _layoutsStartedUnfinished whenever scene or editor restarts,
   * fixing any corruption due to crashed Editor instances (e.g. if you lose power to your
   * machine, Editor will permanently think an old instance of layout routine is still running,
   * and the layout algorithm will only run in partial updates forever more ... which we don't
   * want to happen :))
   */
  public void OnEnable()
  {
   //Debug.Log("SO being re-enabled");
   _layoutsStartedUnfinished = 0;
  }

 private int _layoutsStartedUnfinished = 0;
  public override void Layout(FlexContainer fc)
  {
   if( fc.settings.debugRelayoutCalls || fc.showDebugMessages ) Debug.Log("["+fc.name+"] Layout( self )" );
   if( fc.showDebugMessages )
   Debug.Log("Starting layout of "+fc.hierarchicalName+" using Flexbox Algorithm v2.3 (num already running: "+_layoutsStartedUnfinished);
   _layoutsStartedUnfinished++;
   try
   {
    LayoutWithinCurrentSize(fc);
   }
   finally
   {
    _layoutsStartedUnfinished--;
   }
   
   if( fc.showDebugMessages )
   Debug.Log("-- finished layout of "+fc.hierarchicalName+" using Flexbox Algorithm v2.3 (num layouts still running: "+_layoutsStartedUnfinished);
  }

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

  
  private FlexLayoutData FlexLayoutSetup( FlexContainer container )
  {
   FlexLayoutData result = new FlexLayoutData();

   result.containerAsItem = container.gameObject.GetComponent<FlexItem>();
   result.containerTransform = container.transform as RectTransform;
   
   /** We need to know the container's own container, so that we can calculate the container's padding (used to layout container's children)
    */
   result.containersParent = (container.transform.parent == null) ? null : container.transform.parent.gameObject.GetComponent<FlexContainer>();
   result.containersParentSize = (result.containersParent == null) ? (Vector2?) null : (result.containersParent.transform as RectTransform).rect.size;
   
   result.outerContentSize = result.innerContentSize = result.containerTransform.rect.size;
   if( result.containerAsItem != null ) // it may have internal margins that we need to offset to generate the true innerContentSize
   {
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

  private Vector2? ContentSizeOfChildItemInsideContainer(FlexItem child, Vector2? containerInnerSize, Axis axis)
  {
   /********************************************************
    *
    * 
    * ATTACHED components take priority over CHILDED ones
    *
    *
    ********************************************************/
   
   /**
    * ATTACHED components:
    *   1. Text
    */
   Text childAsText = child.GetComponent<Text>();
   if( childAsText != null )
    return ContentSizeCalculators.ContentSizeFor(childAsText );
   
   /**
    * ATTACHED components:
    *   2. Button
    */
   Button childAsButton = child.GetComponent<Button>();
   if( childAsButton != null )
    return ContentSizeCalculators.ContentSizeFor(childAsButton);
   
   /**
    * ATTACHED components:
    *   3. Toggle
    */
   Toggle childAsToggle = child.GetComponent<Toggle>();
   if( childAsToggle != null )
    return ContentSizeCalculators.ContentSizeFor(childAsToggle);
   
   /**
    * ATTACHED components:
    *   4. Input Textfield
    */
   InputField childAsInputField = child.GetComponent<InputField>();
   if( childAsInputField != null )
    return ContentSizeCalculators.ContentSizeFor(childAsInputField);
   
   /**
    * ATTACHED components:
    *   100. Image (generic)
    *   101. RawImage (generic)
    */
   Image asImage = child.GetComponent<Image>();
   if( asImage != null )
    return ContentSizeCalculators.ContentSizeFor(asImage);
   RawImage asImageRaw = child.GetComponent<RawImage>();
   if( asImageRaw != null )
    return ContentSizeCalculators.ContentSizeFor(asImageRaw);

   /**
    * ATTACHED components:
    *   999. [ILayoutElement subclass]
    */
   ILayoutElement[] layoutElementSubclasses = child.GetComponents<ILayoutElement>();
   if( layoutElementSubclasses.Length > 0 )
   {
    foreach( var element in layoutElementSubclasses )
    {
     /** if it's one of ours, skip it (the child itself will certainly match!) ... no others of ours should be there, but just in case... */
     if( element is FlexItem || element is FlexContainer )
      continue;

     /* DEBUG:
     Vector2 min = new Vector2(element.minWidth, element.minHeight);
     Vector2 flexi = new Vector2(element.flexibleWidth, element.flexibleHeight);
     Vector2 pref = new Vector2(element.preferredWidth, element.preferredHeight);
     
     Debug.LogError("Element: "+element+" has min/pref/flexi: "+min+" / "+pref+" / "+flexi);
     */
     return new Vector2(element.preferredWidth, element.preferredHeight);
    }
   }
   
   /**
    * CHILDED components:
    *   1. Button -- NB: we do this BEFORE CHILDED.Text, because both will fire when there's a button, and this should take priority (but same does NOT happen with ATTACHED components)
    */
   if( child.GetComponentsInChildren<Button>().Length == 1 )
    return ContentSizeCalculators.ContentSizeFor(child.GetComponentsInChildren<Button>()[0]);
   /**
    * CHILDED components:
    *   2. Text -- NB: this would conflict with the search for a Button (which has an embedded Text), which is why we only do it AFTER checking the Button
    */
   if( child.GetComponentsInChildren<Text>().Length == 1 )
   {
    return ContentSizeCalculators.ContentSizeFor(child.GetComponentsInChildren<Text>()[0]);
   }
   
   /**
    * CHILDED components:
    *   100. Image (generic)
    *   101. RawImage (generic)
    */
   if( child.GetComponentsInChildren<Image>().Length > 0 )
    return ContentSizeCalculators.ContentSizeFor( child.GetComponentsInChildren<Image>()[0]);
   if( child.GetComponentsInChildren<RawImage>().Length > 0 )
    return ContentSizeCalculators.ContentSizeFor( child.GetComponentsInChildren<RawImage>()[0]);

   return null;
  }
  
  /**
   * TODO: merge this method with FitContentsLength and cache the results so that the calc is only done once not twice
   */
  private float CalculateBaseChildLengthInContentModeInsideContainer( FlexItem child, Vector2? containerInnerSize, Axis axis )
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
   
   FlexContainer childAsContainer = child.transform.parent == null ? null : child.GetComponent<FlexContainer>();

   if( childAsContainer != null ) // It's a FlexContainer declaring that its size is the size required by its children flex-items
   {
    List<FlexItem> grandChildItems = new List<FlexItem>(); 
    foreach( Transform t in childAsContainer.transform )
    {
     if( t.gameObject.activeSelf )
     {
      FlexItem component = t.gameObject.GetComponent<FlexItem>();
      if( component != null )
       grandChildItems.Add(component);
     }
    }

    float ownBaseLength = 0;
    foreach( var grandChild in grandChildItems )
    {
     if( MainAxis(childAsContainer.direction) == axis ) // we check the grandchild MAIN lengths; i.e. as defined by their flexBasis's
     {
      float grandchild_baseMainLength = CalculateBaseMainLengthInsideContainer(grandChild, axis, null);
      float grandchild_finalCrossLength = HypotheticalMainLength(grandChild, childAsContainer.direction, grandchild_baseMainLength, null);
      ownBaseLength += grandchild_finalCrossLength; // MAIN: we sum the values
     }
     else // we check the grandchild CROSS lengths;
     {
      float grandchild_baseCrossLength = CalculateBaseCrossLengthInsideContainer(grandChild, axis, null);
      float grandchild_finalCrossLength = HypotheticalCrossLength( grandChild, childAsContainer.direction, grandchild_baseCrossLength, null );
      ownBaseLength = Mathf.Max( ownBaseLength, grandchild_finalCrossLength ); // CROSS: we look for MAX
     }
    }

    return ownBaseLength;
   }
   
   Vector2? autoCalculatedChildContentSize = ContentSizeOfChildItemInsideContainer( child, containerInnerSize, axis );
   if( autoCalculatedChildContentSize != null )
     return axis.Length( autoCalculatedChildContentSize ) ?? 0;
   else
   {
    // Finally, if nothing else already matched + returned...
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

  /**
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
  private float CalculateBaseMainLengthInsideContainer(FlexItem child, Axis axis, Vector2? containerInnerSize )
  {
   float rawBaseLength;
   
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
     float? attemptedLength = axis.Length(containerInnerSize); // TODO: when we have VectorNullable2, we can upgrade Length?(..) to give non-null in some cases
     if( attemptedLength != null )
      rawBaseLength = child.flexBasis.value * attemptedLength.Value / 100f;
     else
      throw new FlexboxLayoutException("Cannot layout: cannot calculate the PERCENT baseLength of child ("+child.hierarchicalName+") using parent's indefinite inner size ("+containerInnerSize+")");
     break;

    case FlexBasis.AUTO:
     rawBaseLength = ResolveAutoBasis(child, axis,containerInnerSize) ?? CalculateBaseChildLengthInContentModeInsideContainer(child,containerInnerSize,axis); // i.e. if null: skip to what we do for CONTENT below
     break;
    
    case FlexBasis.CONTENT:
     rawBaseLength = CalculateBaseChildLengthInContentModeInsideContainer(child,containerInnerSize,axis);
     break;
    
    #if PROPRIETARY_ASPECT_FLEXBASIS
#if LITE_VERSION
    case FlexBasis.ASPECT_FIT:
     rawBaseLength = 0;
     break;
#else
    case FlexBasis.ASPECT_FIT:
     if( containerInnerSize.HasValue )
     {
      float containerWidth = Axis.Horizontal.Length( containerInnerSize.Value );
      float containerHeight = Axis.Vertical.Length( containerInnerSize.Value );
      
      float containerRatio = containerWidth / containerHeight;

      if( child.flexBasis.value < containerRatio )
      {
       if( axis == Axis.Vertical)
        rawBaseLength = containerHeight;
       else
        rawBaseLength = child.flexBasis.value * containerHeight;
      }
      else
      {
       if( axis == Axis.Vertical )
        rawBaseLength = containerWidth / child.flexBasis.value;
       else
        rawBaseLength = containerWidth;
      }
      
     }
     else
     {
      rawBaseLength = 0;
     }
     break;
    #endif
    #endif
    
     default:
      throw new Exception("Impossible: 26kdlkjfs");
   }
   
   /**
    * Firefox in 2020 does this:
    *   - for CONTENTBOX, the item is laid out as "padding + flexBasis"
    *   - for BORDERBOX, the item is laid out as "flexBasis"
    *   - for [all modes], the item is laid out as "[above] + margin"
    */
   float addPadding = child.boxSizing == BoxSizing.CONTENT_BOX ? 0 : child.cssPadding.ValueOrNull(axis, containerInnerSize) ?? 0;
   float addMargin = child.cssMargins.ValueOrNull(axis,containerInnerSize)??0;

   return rawBaseLength + addPadding + addMargin;
  }

  /**
   * This method needs to calculate the width or height that would precisely fit the item's content, but the CSS spec
   * IS INCAPABLE OF working with auto-resizing text (which, by definition, has no width/height until the algorithm is
   * complete), so we fudge it a little: we give an estimate, and then for items that are "dynamicallyCrossSized", we
   * do a post-processing pass on the cross-lengths and set their cross-lengths based on their main-lengths.
   *
   * Worse: the CSS Specification IS WRONG and never defines "fit-content". Not only is the Flexbox specification WRONG
   * but the CSS3 Core Specification IS ALSO WRONG - google proves that "fit-content" is never defined in CSS! It is
   * "assumed" that "everyone knows" what it means (implementation details of current browsers document it as a
   * typical Clamp formula: fitContent( X ) == min( [maximum available], max( [minimum available], X ).
   *
   * TODO: merge this method with CalculateBaseChildLengthInContentModeInsideContainer and cache the results so that the calc is only done once not twice
   * TODO: add dynamicallyCrossSized as an overrideable property of FlexItem, somehow modular and extensible!
   */
  private float FitContentsLength(Axis axis, FlexItem item, Vector2? availableSpace )
  {
   float contentEstimate; // TODO: implement this

   /** Does the flexItem have an explicit "width = X" or "height = X" (whichever is appropriate)? */
   float? selfDefinedValue = item.cssDefault(axis).ValueOrNull(axis.Length(availableSpace));

   if( selfDefinedValue == null )
   {
      contentEstimate = axis.Length(ContentSizeOfChildItemInsideContainer( item, availableSpace, axis )) ?? 0;
   }
   else
    contentEstimate = selfDefinedValue.Value;

   /**
    * FINALLY: clamp it to any min/max constraints on the item (this is the "fit" part of "fit-contents")
    */
   contentEstimate = item.cssClampLength(contentEstimate, axis, availableSpace);

   return contentEstimate;
  }
  
  /**
     * CONTENT-BOX: base cross-length = fit-content + padding + margin
     * BORDER-BOX: base cross-length = fit-content + margin
     */
  private float CalculateBaseCrossLengthInsideContainer(FlexItem child, Axis crossAxis, Vector2? containerInnerSize )
  {
#if PROPRIETARY_ASPECT_FLEXBASIS
   float rawCrossLength;
#if LITE_VERSION
#else
   if( child.flexBasis.mode == FlexBasis.ASPECT_FIT)
   {
    if( containerInnerSize.HasValue )
    {
     float containerWidth = Axis.Horizontal.Length(containerInnerSize.Value);
     float containerHeight = Axis.Vertical.Length(containerInnerSize.Value);
     
     float containerRatio = containerWidth / containerHeight;

     if( child.flexBasis.value < containerRatio )
     {
      if( crossAxis == Axis.Vertical)
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
    {
     rawCrossLength = 0;
    }
   }
   else
#endif
   {
    rawCrossLength = FitContentsLength( crossAxis, child, containerInnerSize );
   }
#else
   float rawCrossLength = FitContentsLength( crossAxis, child, containerInnerSize );
#endif
   
   /**
    * Firefox in 2020 does this:
    *   - for CONTENTBOX, the item is laid out as "padding + flexBasis"
    *   - for BORDERBOX, the item is laid out as "flexBasis"
    *   - for [all modes], the item is laid out as "[above] + margin"
    */
   float addPadding = child.boxSizing == BoxSizing.CONTENT_BOX ? 0 : child.cssPadding.ValueOrNull( crossAxis, containerInnerSize) ?? 0;
   float addMargin = child.cssMargins.ValueOrNull( crossAxis,containerInnerSize)??0;

   return rawCrossLength + addPadding + addMargin;
  }

  private float CalculateSpaceForGrowShrinkLine( FlexLayoutData d, float totalOuterHypotheticalMainLength )
  {
   float leftoverSpace = 0f;
   
   float mainLengthInContainerAvailableForChildContent = d.axisMain.Length(d.innerContentSize);
   leftoverSpace = mainLengthInContainerAvailableForChildContent - totalOuterHypotheticalMainLength /** NB: this includes all margins (and some padding)*/;
   
   return leftoverSpace;
  }

  /**
   * Main method for calculating FlexItem *SIZES*
   *
   * (called by the layout algorithm internally, before it does the calculation of *POSITIONS*)
   */
  private Dictionary<FlexItem,Vector2> CalculateAndSetAllChildSizes( FlexLayoutData d, FlexContainer container, out float unusedMainSpace )
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
   foreach( var child in d.childItems )
   {
    childBaseMainLengths[child] = CalculateBaseMainLengthInsideContainer(child, d.axisMain, d.innerContentSize);
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "] child: " + child.name + "-BaseLength = " + childBaseMainLengths[child]);
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
   float totalOuterHypotheticalMainLength = 0f; // W3 spec defines "outer hypothetical main size" as "the base size, clamped by min|max width|height". It's a rather bad name, makes the spec much harder to read.
   foreach( var child in d.childItems )
   {
    if( !childBaseMainLengths.ContainsKey(child) )
     Debug.LogError("Impossible: 235234l23kj");

    //ULTRA debug:if( container.showDebugMessages ) Debug.Log("child [" + child + "] y adding to totalHypotheticalLength (child's baseLength = " + baseSizes[child] + ") : " + child.HypotheticalLength(container.direction, baseSizes[child], parentSizeOrNull));
    childHypotheticalMainLengths[child] = HypotheticalMainLength( child, container.direction, childBaseMainLengths[child], d.innerContentSize);
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "] child: " + child.name + "-HypotheticalMainLength = " + childHypotheticalMainLengths[child]);
    totalOuterHypotheticalMainLength += childHypotheticalMainLengths[child];
   }
   
   /****************
    *
    *
    * 
    * TODO: Flex-wrap implementation will happen here, when support is added for it
    *
    * 
    * 
    */
   
   /**
    * Spec: 9.4.7: https://www.w3.org/TR/css-flexbox-1/#algo-cross-item
    * 
    * Step 3a: base cross lengths
    */
   Dictionary<FlexItem, float> childBaseCrossLengths = new Dictionary<FlexItem, float>();
   foreach( var child in d.childItems )
   {
    childBaseCrossLengths[child] = CalculateBaseCrossLengthInsideContainer(child, d.axisCross, d.innerContentSize);
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "] child: " + child.name + "-CrossLength = " + childBaseCrossLengths[child]);
   }
   
   
   /**
    * Spec: 9.4.7: https://www.w3.org/TR/css-flexbox-1/#algo-cross-item
    * 
    * Step 3b: hypothetical cross lengths
    *
    *  Hypothetical lengths = base-length constrained by minWidth/maxWidth and minHeight/maxHeight
    *
    * Spec: 9.4.8: https://www.w3.org/TR/css-flexbox-1/#algo-cross-line
    * 
    * Step 4: Calculate total of all children's hypothetical-cross-lengths
    *    (MAY include padding for SOME children depending upon their box-sizing settings) 
    */
   Dictionary<FlexItem, float> childHypotheticalCrossLengths = new Dictionary<FlexItem, float>();
   float largestHypotheticalCrossLength = 0f; // W3 spec defines "outer hypothetical main size" as "the base size, clamped by min|max width|height". It's a rather bad name, makes the spec much harder to read.
   foreach( var child in d.childItems )
   {
    if( !childBaseCrossLengths.ContainsKey(child) )
     Debug.LogError("Impossible: j324kjlsdf");

    //ULTRA debug:if( container.showDebugMessages ) Debug.Log("child [" + child + "] y adding to totalHypotheticalLength (child's baseLength = " + baseSizes[child] + ") : " + child.HypotheticalLength(container.direction, baseSizes[child], parentSizeOrNull));
    childHypotheticalCrossLengths[child] = HypotheticalCrossLength( child, container.direction, childBaseCrossLengths[child], d.innerContentSize);
    largestHypotheticalCrossLength = Math.Max( largestHypotheticalCrossLength, childHypotheticalCrossLengths[child] );
   }
   
   /**
    *
    *
    * Theoretically, we now possess ALL the information needed to set self.size, if self is in basis = CONTENT (and was
    * therefore unsizable until now).
    *
    *
    */
   
   if( container.showDebugMessages ) Debug.Log("Total base: " + totalOuterHypotheticalMainLength + ", largest Cross: " + largestHypotheticalCrossLength );  

   /**
    * Step 5: Grow-or-Shrink sizes
    */
   float leftoverSpace = CalculateSpaceForGrowShrinkLine(d, totalOuterHypotheticalMainLength);
   
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] leftoverspace = "+leftoverSpace+" will ... "+(leftoverSpace<0?"SHRINK":(leftoverSpace>0?"GROW":"DO NOTHING")));
   
   Dictionary<FlexItem, float> grownAndShrunkDeltaLengths = null;
   if( leftoverSpace > 0 )
   {
    grownAndShrunkDeltaLengths = ResolveGrowSizes(d, leftoverSpace, childBaseMainLengths, out unusedMainSpace ); // NOTE: leftoverspace was pre-calc'd using HYPOTHETICAL lengths, but grow/shrink only uses the BASE ones
   }
   else if( leftoverSpace < 0 )
   {
    grownAndShrunkDeltaLengths = ResolveShrinkSizes(d,-1f * leftoverSpace, childBaseMainLengths ); // NOTE: leftoverspace was pre-calc'd using HYPOTHETICAL lengths, but grow/shrink only uses the BASE ones
    unusedMainSpace = 0;
   }
   else
   {
    grownAndShrunkDeltaLengths = new Dictionary<FlexItem, float>();
    foreach( var child in d.childItems )
     grownAndShrunkDeltaLengths[child] = 0;
    unusedMainSpace = leftoverSpace;
   }
   
   /**
    * Spec: 9.7.5: https://www.w3.org/TR/css-flexbox-1/#resolve-flexible-lengths (SPEC has a bug, it doesn't hotlink the sub-bullets here)
    *
    * Step 6a: Set the final main-sizes using the output of grown-and-shrunk sizing
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
    * Spec: 9.7.5: https://www.w3.org/TR/css-flexbox-1/#resolve-flexible-lengths (SPEC has a bug, it doesn't hotlink the sub-bullets here)
    *
    * Step 6b: Set the final cross-sizes using the output of grown-and-shrunk sizing
    * (almost all sizing is now complete)
    *
    * Note that the deltas of grow/shrink are to be applied to the INNER main/cross (excluding margin, and for CONTENT also excluding padding)
    */
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: Setting cross-axis of flexbox (" + container.name + ") using internal content size: " + d.innerContentSize );
   
   Dictionary<FlexItem, Vector2> finalSizes = new Dictionary<FlexItem, Vector2>();
   foreach( var child in d.childItems )
     {
      Vector2 newSize;
      float mainLength = finalLengths[child];
      float crossLength = childHypotheticalCrossLengths[child];
      
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
        crossLength = d.axisCross.Length(d.innerContentSize); // TODO: only apply this when the crosslength was already indefinite
        
        /** re-apply any min/max width/height constraints EVEN AFTER stretching (stretching gets overridden by min/max in CSS3 in Firefox in 2020) */
        crossLength = child.cssClampLength(crossLength, d.axisCross, d.innerContentSize);
       }
      }
      
      /**
       * Subtract the margins before calling setSize
       * 
       * (margins are included in all flex layout, but the actual RectTransforms should not include them)
       */
      mainLength -= child.cssMargins.ValueOrZero(d.axisMain,d.innerContentSize);
      crossLength -= child.cssMargins.ValueOrZero(d.axisCross, d.innerContentSize);

      newSize = (d.axisMain == Axis.Horizontal)
       ? new Vector2(mainLength, crossLength)
       : new Vector2(crossLength, mainLength);
      
      finalSizes[child] = newSize;
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
   foreach( var child in d.childItems )
   {
    var childTransform = child.transform as RectTransform;
    
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
  
  /**
   * Main layout method: decides FlexItem *SIZES then *POSITIONS*
   */
  private void LayoutWithinCurrentSize( FlexContainer container )
  {
   FlexLayoutData d = FlexLayoutSetup(container);
   
   if( container.settings.debugRelayoutCalls || container.showDebugMessages ) Debug.Log("["+container.name+"] LayoutWithinCurrentSize()" );
   
   /**
    *
    * Phase 1:
    *
    * Calculate sizes of all children, and fit them into the available space, ready to be positioned
    * 
    * NOTE: the "finalSizes" here do NOT include margins; they are the flex INNER sizes, aka the box-sizes
    */
   float unusedMainSpace; // NOTE: this is calculated using children OUTER sizes (content + padding + margin), so it has already allocated space for margins and everything else too!
   Dictionary<FlexItem, Vector2> finalSizes = CalculateAndSetAllChildSizes(d, container, out unusedMainSpace );
   float containerUsedMainLength = d.axisMain.Length(d.innerContentSize) - unusedMainSpace;

   /*********** DEBUG AT END OF SIZING ROUTINES *************/
   if( container.showDebugMessages )
   {
    foreach( var child in d.childItems )
    {
     var childTransform = child.transform as RectTransform;
     Debug.Log("[" + container.name + "]: Sizing complete, child (" + child.name + ") has size: " + childTransform.rect.size);
    }
   }

   /**
    *
    *
    * ------------- Sizing complete, now position ---------------------
    *
    *
    * 
    */
   
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
     throw new Exception("Impossible: j352sdflkj");
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
     throw new Exception("Impossible: j352ssd324234kj");
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
     throw new Exception("Impossible: j352ssd324234kj");
   }
   
   /** ... main axis: calculate advance to support different "justify" settings */
   float preAdvance, postAdvance;
   CalculateAdvances( d.childItems.Count, container, unusedMainSpace, out preAdvance, out postAdvance);
   
   if( container.showDebugMessages ) Debug.LogFormat("[" + container.name + "] Advances calc'd: preAdvance ={0}, postAdvance = {1}", preAdvance, postAdvance);

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
   float mainAxisContainerPaddedLength = d.axisMain.Length(d.outerContentSize);
   float crossAxisContainerPaddedLength = d.axisCross.Length(d.outerContentSize);
   float justifiedStartMainAxis = 0;
   switch( container.justifyContent )
   {
    case FlexJustify.SPACE_BETWEEN:
    case FlexJustify.SPACE_AROUND:
    case FlexJustify.SPACE_EVENLY:
     justifiedStartMainAxis = -1f * mainAxisContainerPaddedLength / 2f;
     if( (flipYAbsoluteMeasures*mainAxisAdvanceDirection) < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
      justifiedStartMainAxis += mainAxisContainerPaddedLength; // note this is CONTAINER length
     break;
    
    case FlexJustify.START:
     justifiedStartMainAxis = -1f * mainAxisContainerPaddedLength / 2f;
     if( (flipYAbsoluteMeasures*mainAxisAdvanceDirection) < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
      justifiedStartMainAxis += containerUsedMainLength; // note this is CHILDREN USED length
     break;
    
    case FlexJustify.CENTER:
     justifiedStartMainAxis = 0 - containerUsedMainLength / 2f;
     if( (flipYAbsoluteMeasures*mainAxisAdvanceDirection) < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
      justifiedStartMainAxis += containerUsedMainLength; // note this is CHILDREN USED length
     break;
    
    case FlexJustify.END:
     justifiedStartMainAxis = mainAxisContainerPaddedLength / 2f - containerUsedMainLength;
     if( (flipYAbsoluteMeasures*mainAxisAdvanceDirection) < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
      justifiedStartMainAxis += containerUsedMainLength;
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

   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Layout(" + container.justifyContent + ") -- justifiedStartMainAxis = " + justifiedStartMainAxis + "     (ContainerPaddedLength = " + mainAxisContainerPaddedLength + ")");
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Layout(" + container.justifyContent + "). Advances: pre=" + preAdvance + ", post=" + postAdvance + " --  Unity first offset = " + justifiedStartMainAxis);
   
   float justifiedStartCrossAxis = 0;
   switch( container.alignItems )
   {
    case AlignItems.START:
    case AlignItems.STRETCH:
     justifiedStartCrossAxis = -1f * crossAxisContainerPaddedLength / 2f;
     if(crossAxisAdvanceDirection < 0 ) // if we're going backwards, we start at the end, so have to add on the total used length
      justifiedStartCrossAxis += crossAxisContainerPaddedLength;
     break;
        
    case AlignItems.CENTER:
     justifiedStartCrossAxis = 0f;//-1f * (crossAxisContainerPaddedLength) / 2f;
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
      throw new Exception("Impossible:52jklj");
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
   foreach( var child in d.childItems )
   {
    /*
     * ...now iterating over each child...
     *
     * Define position as:
     *
     *     position = mainOffsetFromParentPadding + justifiedStartMainAxis + offsetAccumulatedFromPreviousItems + (preAdvance + itemLeadingMargin + childFinalLength / 2f);
     */
    
    
     /** 5b. the child's length along main axis */
     childFinalLength = d.axisMain.Length(finalSizes[child]);
    
    
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
      throw new Exception("Impossible: w3dfg32lr");
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
    /**
     * ACCUMULATE the total advance from this child:
     */
    offsetAccumulatedFromPreviousItems += preAdvance + (itemLeadingMargin + childFinalLength + itemTrailingMargin) + postAdvance;
    
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "] position will be: **" + mainAxisPosition + "** parentpadding:" + mainOffsetFromParentPadding + " + mainaxisJustification:" + justifiedStartMainAxis + " +offset:" + offsetAccumulatedFromPreviousItems + " + preadv:" + preAdvance + " + spacingLeft: " + childMarginLeft + " + 0.5*objectsize:" + childFinalLength + "");


    /** Cross Axis */
    /** Cross-axis margin handling is bizarre and complicated (many strange behaviours due to the CSS3 spec).
     *  ... first you position according to center/start/end/etc
     *  ... then you SELECTIVELY IGNORE margins based on whichever align-items value you had (row + center honours top + bottom; row + start IGNORES bottom, etc)
     */
    //float crossFullLength = crossAxisContainerContentLength;
    float crossChildLength = d.axisCross.Length(finalSizes[child]);
    
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "] ... cross-axis child-length: " + crossChildLength);

    /** ... cross-axis: calculate center point along cross axis */
    bool mainIsVertical = d.axisMain == Axis.Vertical;
    float crossAxisPosition = 0f;
    float crossAxisChildStartOffset = 0f; // Unlike Main, Cross has a different ABSOLUTE start point PER-CHILD
    float itemCrossLeadingMargin = 0f;
    switch( container.alignItems )
    {
     case AlignItems.CENTER:
     {
      crossAxisChildStartOffset = flipYAbsoluteMeasures * crossChildLength/2f;
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
      Debug.LogError("FlexBox layouts: 'alignItems=Baseline' not supported");
     }
      break;
    }

    /** ... cross-axis: calculate final position on cross-axis dimension */
    crossAxisPosition = justifiedStartCrossAxis + crossAxisChildStartOffset
    +
    crossAxisAdvanceDirection * (crossOffsetFromParentPadding + itemCrossLeadingMargin + crossChildLength / 2f);
    
    /** Position it */
    (child.transform as RectTransform).anchoredPosition = (d.axisMain == Axis.Horizontal)
     ? new Vector2( mainAxisPosition, crossAxisPosition )
     : new Vector2( crossAxisPosition, mainAxisPosition );
    
     //if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "] ...cross-axis changes to position: (" +  + "," + positionY + ")");
   }

  }

  private static Dictionary<FlexItem, float> ResolveGrowSizes( FlexLayoutData d, float excessToFill, Dictionary<FlexItem, float> baseLengths, out float unusedSpace )
  {
   Dictionary<FlexItem, float> growDeltas = new Dictionary<FlexItem, float>();

   //TODO:  Debug.LogError("Grow is ignoring the min-height: adding a min-height bigger than the assigned height, in a place where some Grow is possible, is causing the height-based Spare space to be allocated, instead of the height-modified-by-minheight-based Spare space");

   /** Allocate all remaining space as padding around items, proportional to flex-grow */
   float totalGrowWeight = 0;
   foreach( var child in d.childItems )
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
   List<FlexItem> growingChildren = new List<FlexItem>(d.childItems);
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
     CSS3Length cssMax = child.cssMax(d.axisMain);
       if( cssMax.mode == CSS3LengthType.NONE )
        acceptedExcess = offeredExcess;
       else
       {
        float? maxAllowedLength = cssMax.ValueOrNull( d.axisMain.Length(d.innerContentSize) );
        float baseLength = baseLengths[child];
        acceptedExcess = maxAllowedLength == null ? offeredExcess : Mathf.Min((maxAllowedLength.Value - baseLength), offeredExcess);
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
  private static Dictionary<FlexItem, float> ResolveShrinkSizes(FlexLayoutData d, float deficitToFill, Dictionary<FlexItem, float> baseLengths)
  {
   Dictionary<FlexItem, float> growDeltas = new Dictionary<FlexItem, float>();

   // Initialize
   foreach( var child in d.childItems )
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
   List<FlexItem> unfrozenItems = new List<FlexItem>(d.childItems);
   float currentTotalShrinkWeight = 1;
   /** ... looping until either all excess has found a home, or all children have hit their MAX width|height limits */
   while( unfrozenItems.Count > 0 && allocatingDeficit > 0 && currentTotalShrinkWeight > 0 )
   {
    /** Recalculate the total of the scaleShrinkWeights for the items we're still shrinking */
    scaledShrinkFactors.Clear();
    foreach( var child in unfrozenItems )
    {
     float ssf = _ScaledShrinkFactor(child, baseLengths[child], d.axisMain, d.innerContentSize);
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
     CSS3Length cssMin = child.cssMin(d.axisMain);
     float? minAllowedLength = cssMin.ValueOrNull(d.axisMain.Length(d.innerContentSize));
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

  private static float HypotheticalMainLength(FlexItem item, FlexDirection direction, float length, Vector2? parentSize)
  {
   Axis mainAxis = MainAxis(direction);
   return item.cssClampLength(length, mainAxis, parentSize);
  }
  
  private static float HypotheticalCrossLength(FlexItem item, FlexDirection direction, float length, Vector2? parentSize)
  {
   Axis crossAxis = CrossAxis(direction);
   return item.cssClampLength(length, crossAxis, parentSize);
  }
 }
}