#define TRUE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Flexbox4Unity
{
 [CreateAssetMenu(menuName = "Flexbox/Layout Algorithms/v2.0", fileName = "Layout Algorithm v2.0.asset", order = 106)]
 public class FlexboxLayoutAlgorithm20 : IFlexboxLayoutAlgorithm
 {
  public override string defaultAssetName
  {
   get { return "Layout Algorithm v2.0"; }
  }
  
  /** UnityEditor's built-in API for stating if Layout is in progress is broken in all versions up to 2019, so we have to track it manually */
  public override bool isLayoutInProgress
  {
   get { return _layoutsStartedUnfinished > 0; }
  }

  private int _layoutsStartedUnfinished = 0;

  public override void Layout(FlexContainer fc)
  {
   //Debug.Log("Using Flexbox Algorithm v2.0");
   _layoutsStartedUnfinished++;
   LayoutResult r = SizeSelfAndLayoutChildren(fc);
   _layoutsStartedUnfinished--;
  }

  public LayoutResult LayoutFetchSize_Obsolete(FlexContainer fc, bool b)
  {
   _layoutsStartedUnfinished++;
   LayoutResult r = SizeSelfAndLayoutChildren(fc, b);
   _layoutsStartedUnfinished--;
   return r;
  }

  public bool SizeSelfAndLayoutChildren(FlexContainer container, out Vector2 resolvedSize, bool bbbbb)
  {
   _layoutsStartedUnfinished++;
   LayoutResult r = SizeSelfAndLayoutChildren(container, bbbbb);
   _layoutsStartedUnfinished--;

   if( r.resolvedSize == null )
   {
    resolvedSize = Vector2.zero;
    return false;
   }
   else
   {
    resolvedSize = (Vector2) r.resolvedSize;
    return true;
   }
  }

  private static List<FlexItem> ChildFlexItems(FlexContainer container)
  {
    /**
      * Gather the direct child layout elements
      * 
      * NB: Unity's GetComponentsInChildren API does NOT do what the container.name suggests it does: instead it returns components on SELF AS WELL AS on children with this call. Also, we need a List,
      * which Unity still (in 2019) does not provide in their API.
      */
    var childItems = new List<FlexItem>();
    FlexItem component = null;
    foreach( Transform t in container.transform )
    {
     if( t.gameObject.activeSelf )
     {
      component = t.gameObject.GetComponent<FlexItem>();
      if( component != null )
       childItems.Add(component);
     }
    }

    return childItems;
  }
  
  /**
   * Calculates the base-size of a FlexContainer iff it is set to basis=CONTENT; since this is dependent upon the
   * specific layout-algorithm used, it is embedded here instead of being embedded inside FlexItem.ContentSize(..)
   * or FlexContainer.ContentSize(..).
   *
   * Callers are expected to use this as a base-size for this container as a child in a parent-container; the
   * parent-container is expected to then apply shrink, grow, etc as required ON TOP OF whatever value this method
   * returns.
   *
   * i.e. this method ONLY CALCULATES THE SELF-DEFINED SIZE (self + children), it DOES NOT LOOK AT the parent
   * in any way
   */
  private static Vector2? BaseSizeOfBox(FlexContainer container)
  {
   Vector2? resolvedSize = null;

   FlexItem containerAsItem = container.gameObject.GetComponent<FlexItem>();

   //May need to handle AUTO as well as CONTENT

   var childItems = ChildFlexItems( container );

   RectTransform.Axis axisMain = (container.direction == FlexDirection.ROW || container.direction == FlexDirection.ROW_REVERSED) ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;

   HashSet<FlexItem> unResolvedChildren = new HashSet<FlexItem>(childItems);
   HashSet<FlexItem> resolvedChildren = new HashSet<FlexItem>();

   /** Definitions:
    *
    * For each child:
    *  padding? = 0 if BORDER_BOX ... or padding? = (padding) if CONTENT_BOX
    *  base-size = specified by flex-basis, AND/OR width AND/OR height.
    *  unpadded-final-size = same as final-size, but without the external padding
    *  final-size = applied to child transform. final = (unpadded-final) + (padding?)
    *  main-length = base + (padding?) + margin (how much space the child takes out of parent)
    *  cross-length = base + (padding?) + margin (how much space the child takes out of parent)
    */
   Dictionary<FlexItem, Vector2> baseChildSizes = new Dictionary<FlexItem, Vector2>();

   /** If self is resolvable, resolve self first */
#pragma warning disable 0219
   bool isSelfSizeResolvedYet = false;

   /** Calculate the sizes up and down the full tree */
   float totalOuterHypotheticalMainLength = 0; // W3 spec defines "outer hypothetical main size" as "the base size, clamped by min|max width|height". It's a bad container.name, makes the spec much harder to read.
   float largestBaseWidth = 0; // VALUE should NOT be set, but we have to workaround SEVERE bugs in Microsoft's C# compiler (it has no static-analysis step)
   Vector2 totalChildMargins = Vector2.zero, totalChildExternalPaddings /** only used in CONTENT_BOX mode; otherwise is always Vector2.zero */; // VALUE should NOT be set, but we have to workaround SEVERE bugs in Microsoft's C# compiler (it has no static-analysis step)

   int numResolveTotal = 0;
   int numResolvedOnLatestPass;
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Performing FIRST ResolveChildren pass...");

   Vector2 paddingAddsToContentSize = Vector2.zero;
   if( containerAsItem != null )
    switch( containerAsItem.boxSizing )
    {
     case BoxSizing.BORDER_BOX:
      /** Padding is defined as INTERNAL, so has zero effect on the total content-box of the parent item */
      break;

     case BoxSizing.CONTENT_BOX:
      /** Padding is defined as EXTERNAL, so it extends the content-size of the contents */
      paddingAddsToContentSize = new Vector2(containerAsItem.cssPadding.HorizontalValueOrZero(ParentSizeOf(containerAsItem)), containerAsItem.cssPadding.VerticalValueOrZero(ParentSizeOf(containerAsItem)));
      break;
    }

   do
   {
    if( numResolveTotal > 0 )
    {
     switch( container.direction )
     {
      case FlexDirection.COLUMN:
      case FlexDirection.COLUMN_REVERSED:
       resolvedSize = new Vector2(largestBaseWidth, totalOuterHypotheticalMainLength) + totalChildMargins;
       break;
      case FlexDirection.ROW:
      case FlexDirection.ROW_REVERSED:
       resolvedSize = new Vector2(totalOuterHypotheticalMainLength, largestBaseWidth) + totalChildMargins;
       break;
      default:
       Debug.LogError("[" + container.name + "] Impossible, there are no other enum values. C# sucks - compiler should prevent this from being needed");
       resolvedSize = Vector2.zero;
       break;
     }

     resolvedSize += paddingAddsToContentSize;

     if( container.showDebugMessages ) Debug.Log("[" + container.name + "] ...late-resolved (Flex-basis: " + (containerAsItem == null ? "" : "" + containerAsItem.flexBasis.mode) + ") container LayoutGroup to size: " + resolvedSize + " (child margins included: " + totalChildMargins + ")");

     isSelfSizeResolvedYet = true;
    }

    /** ... ... re-attempt resolution of all children */
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Performing SECONDARY ResolveChildren pass...");
    // BUG: need to check this against spec, but ... it seems this is a bug: children that were previously resolved DEPENDENT UPON the container size need to be resized now - but we don't give them a chance! We told them isSelfSizeResolvedYet=true, and then (probably) changed it afterwards based on their actions 
    numResolvedOnLatestPass = ResolveAsManyChildrenAsPossible(container, axisMain, resolvedChildren, unResolvedChildren, baseChildSizes, childItems, resolvedSize, out totalOuterHypotheticalMainLength, out largestBaseWidth, out totalChildMargins, out totalChildExternalPaddings);
    numResolveTotal += numResolvedOnLatestPass;

   } while( numResolvedOnLatestPass > 0 ); // any pass where numResolved increases === selfLayoutItem needs re-calculating

   if( numResolveTotal < 1 )
   {
    Debug.LogError("Cannot lay-out this situation: parent depends on child sizes, and all children depend on parent, parent = "+container.hierarchicalName, container.gameObject );
    resolvedSize = null;
   }

   return resolvedSize;
  }

  /**
   * The core of the Flexbox layout algorithm
   * 
   * @param resolvedSizeOrZero contains zero, or the resolved-size of self (if this method returned true)
   * @param resolveAsPreferredContentSize indicates whether to ignore provided size (from parent GameObject) and calculate
   * its "ideal" size. This is only relevant/used when basis = CONTENT, and the parent needs to tell this object to
   * figure out its ideal size so parent can adjust appropriately.
   * @return true if managed to resolve self-size (implicitly: resolved all children), false otherwise.
   */
  private LayoutResult SizeSelfAndLayoutChildren(FlexContainer container, bool resolveAsPreferredContentSize = false)
  {
   LayoutResult result = new LayoutResult();

   FlexItem containerAsItem = container.gameObject.GetComponent<FlexItem>();

   //if( container.showDebugMessages ) Debug.Log("Starting self-layout; total active self-layout calls was: "+FlexboxElement.totalActiveLayoutSelfAndChildren);

   Vector2 containerPaddedSize = Vector2.zero; // force a value to allow C# compiler to compile, WITHOUT having to write every line of code in next 1000 lines using a nullable (huge amount of unnecessary typing)
   Vector2 containerContentSize = Vector2.zero; // force a value to allow C# compiler to compile, WITHOUT having to write every line of code in next 1000 lines using a nullable (huge amount of unnecessary typing)
   //Vector2 containerContentOffset = Vector2.zero; // force a value to allow C# compiler to compile, WITHOUT having to write every line of code in next 1000 lines using a nullable (huge amount of unnecessary typing)
   Rect containerPaddingInternal = Rect.zero; // force a value to allow C# compiler to compile, WITHOUT having to write every line of code in next 1000 lines using a nullable (huge amount of unnecessary typing)

   /**
    * Gather the direct child layout elements
    * 
    * NB: Unity's GetComponentsInChildren API does NOT do what the container.name suggests it does: instead it returns components on SELF AS WELL AS on children with this call. Also, we need a List,
    * which Unity still (in 2019) does not provide in their API.
    */
   var childItems = new List<FlexItem>();
   FlexItem component = null;
   foreach( Transform t in container.transform )
   {
    if( t.gameObject.activeSelf )
    {
     component = t.gameObject.GetComponent<FlexItem>();
     if( component != null )
      childItems.Add(component);
    }
   }

   /**
    * Global vars during the layout algorithm
    */
   var selfTransform = container.transform as RectTransform;


#pragma warning disable 0219 // c# compiler complains incessantly, and Unity's compiler fails to honour global disables
   RectTransform.Axis axisMain = (container.direction == FlexDirection.ROW || container.direction == FlexDirection.ROW_REVERSED) ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
   RectTransform.Axis axisCross = (container.direction == FlexDirection.ROW || container.direction == FlexDirection.ROW_REVERSED) ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;

   HashSet<FlexItem> unResolvedChildren = new HashSet<FlexItem>(childItems);
   HashSet<FlexItem> resolvedChildren = new HashSet<FlexItem>();

   /** Definitions:
    *
    * For each child:
    *  padding? = 0 if BORDER_BOX ... or (padding) if CONTENT_BOX
    *  base-size = specified by flex-basis, AND/OR width AND/OR height.
    *  unpadded-final-size = same as final-size, but without the external padding
    *  final-size = applied to child transform. final = (unpadded-final) + (padding?)
    *  main-length = base + (padding?) + margin (how much space the child takes out of parent)
    *  cross-length = base + (padding?) + margin (how much space the child takes out of parent)
    */
   Dictionary<FlexItem, Vector2> baseChildSizes = new Dictionary<FlexItem, Vector2>();

   /** If self is resolvable, resolve self first */
   bool isSelfSizeResolvedYet = false;

   if( containerAsItem != null
       && containerAsItem.flexBasis.mode == FlexBasis.CONTENT
       && resolveAsPreferredContentSize )
   {
    // do nothing, size will be calculated a few steps later
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "] ...UNresolved (Flex-basis: CONTENT) container LayoutGroup delaying set-size");
   }
   else
   {
    /** honour the size that UnityEditor has provided (makes Flexbox4Unity seamlessly compatible with UnityUI parent containers) */
    Vector2 resolvedSize = selfTransform.rect.size;
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "] ...resolved (Flex-basis: " + (containerAsItem == null ? "" : "" + containerAsItem.flexBasis.mode) + ") container LayoutGroup to size: " + resolvedSize);

    /**
     * padding reduces INTERNAL area, but does NOT affect total size
     */
    Rect paddingAdded = CalculateInternalPaddingAndChildOffset(containerAsItem, container.showDebugMessages);
    containerPaddedSize = resolvedSize;
    containerContentSize = resolvedSize - paddingAdded.size;
    containerPaddingInternal = paddingAdded;

    if( containerAsItem != null
        && containerAsItem.boxSizing == BoxSizing.CONTENT_BOX )
    {
     Debug.LogError("Not supported: CONTENT_BOX");
    }


    //Not needed because we're reading from it, but implicitly we should have: selfTransform.SetSize(resolvedSize);
    isSelfSizeResolvedYet = true;
   }

   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] self resolved? " + isSelfSizeResolvedYet + " to contentsize/padded size: " + containerContentSize + " (" + containerPaddedSize + ")"); //"   offset for children = "+containerContentOffset);

   /** Calculate the sizes up and down the full tree */
   float totalOuterHypotheticalMainLength; // W3 spec defines "outer hypothetical main size" as "the base size, clamped by min|max width|height". It's a bad container.name, makes the spec much harder to read.
   float largestBaseWidth;
   Vector2 totalChildMargins, totalChildExternalPaddings /** only used in CONTENT_BOX mode; otherwise is always Vector2.zero */;

   /** ... this next line MUST be a function (it was easier to read when it was inline), because
    * when the parent depends on children, it has to attempt a partial-layout, then RE-RUN the
    * exact same code 0, 1, or 2 times depending on the amount of cross-dependencies among children
    */
   int numResolvedOnLatestPass;
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Performing FIRST ResolveChildren pass...");
   numResolvedOnLatestPass = ResolveAsManyChildrenAsPossible(container, axisMain, resolvedChildren, unResolvedChildren, baseChildSizes, childItems, containerContentSize, out totalOuterHypotheticalMainLength, out largestBaseWidth, out totalChildMargins, out totalChildExternalPaddings);

   /** ... 2c: If self wasn't resolved, we now have enough data to resolve it */
   if( !isSelfSizeResolvedYet )
   {
    if( numResolvedOnLatestPass < 1 )
    {
     Debug.LogError("Cannot lay-out this situation: parent depends on child sizes, and all children depend on parent");
     result.resolvedSize = null;
     //if( container.showDebugMessages ) Debug.Log("..Ending self-layout; total active self-layout calls will be: "+(FlexboxElement.totalActiveLayoutSelfAndChildren-1));
     return result;
    }

    while( numResolvedOnLatestPass > 0 ) // any pass where numResolved increases === selfLayoutItem needs re-calculating
    {
     Vector2 resolvedSize; // we'll calculate it here since we're wrapped in an outer IF() that only fires if "NOT isSelfSizeResolvedYet", meaning we're in a size=CONTENT
     switch( container.direction )
     {
      case FlexDirection.COLUMN:
      case FlexDirection.COLUMN_REVERSED:
       resolvedSize = new Vector2(largestBaseWidth, totalOuterHypotheticalMainLength) + totalChildMargins;
       break;
      case FlexDirection.ROW:
      case FlexDirection.ROW_REVERSED:
       resolvedSize = new Vector2(totalOuterHypotheticalMainLength, largestBaseWidth) + totalChildMargins;
       break;
      default:
       Debug.LogError("[" + container.name + "] Impossible, there are no other enum values. C# sucks - compiler should prevent this from being needed");
       resolvedSize = Vector2.zero;
       break;
     }

     if( container.showDebugMessages ) Debug.Log("[" + container.name + "] ...late-resolved (Flex-basis: " + (containerAsItem == null ? "" : "" + containerAsItem.flexBasis.mode) + ") container LayoutGroup to size: " + resolvedSize + " (child margins included: " + totalChildMargins + ")");

     /**
      * padding reduces INTERNAL area, but does NOT affect total size
      */
     Rect paddingAdded = CalculateInternalPaddingAndChildOffset(containerAsItem, container.showDebugMessages);
//     containerContentOffset = paddingAdded.position;
     //    containerContentOffset.y *= -1f; // Because Unity uses y = up
     containerPaddedSize = resolvedSize;
     containerContentSize = resolvedSize - paddingAdded.size;
     containerPaddingInternal = paddingAdded;

     selfTransform.SetSize(resolvedSize);
     isSelfSizeResolvedYet = true;

     /** ... ... re-attempt resolution of all children */
     if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Performing SECONDARY ResolveChildren pass...");
     numResolvedOnLatestPass = ResolveAsManyChildrenAsPossible(container, axisMain, resolvedChildren, unResolvedChildren, baseChildSizes, childItems, containerContentSize, out totalOuterHypotheticalMainLength, out largestBaseWidth, out totalChildMargins, out totalChildExternalPaddings);
    }
   }

   //containerContentOffset = Vector2.zero;

   /** Step 3: Grow-or-Shrink sizes */
   float containerLengthMinusChildMarginsPadding = 0f; // Padding is NOT included in BORDER_BOX, but IS included in CONTENT_BOX
   float totalChildMarginsMainAxis = 0f;
   float totalChildPaddingsMainAxis = 0f;
   float leftoverSpace = 0f;
   /** ... subtract the children's margins from container length (as required by flexbox algorithm for handling margins: this happens BEFORE grow/shrink calcs)
    *
    * i.e. grow/shrink is performed as though the margins had already been "guaranteed" for every item. If an item can shrink,
    * and there is only room for its margins not for itself, it shrinks to nothing, leaving its empty margins taking up the space!
    *
    * NB: this *ONLY APPLIES* to main-axis; cross-axis in flexbox bizarrely discards margins / ignores them whenever inconvenient.
    */
   switch( container.direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.COLUMN_REVERSED:
     totalChildMarginsMainAxis = totalChildMargins.y;
     totalChildPaddingsMainAxis = totalChildExternalPaddings.y;
     containerLengthMinusChildMarginsPadding = containerContentSize.y - totalChildMarginsMainAxis - totalChildPaddingsMainAxis;
     break;
    case FlexDirection.ROW:
    case FlexDirection.ROW_REVERSED:
     totalChildMarginsMainAxis = totalChildMargins.x;
     totalChildPaddingsMainAxis = totalChildExternalPaddings.x;
     containerLengthMinusChildMarginsPadding = containerContentSize.x - totalChildMarginsMainAxis - totalChildPaddingsMainAxis;
     break;
   }

   leftoverSpace = containerLengthMinusChildMarginsPadding - totalOuterHypotheticalMainLength;
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: Step3: preparing to allocate/clawback " + leftoverSpace + " space (container has: " + containerLengthMinusChildMarginsPadding + ", children need: " + totalOuterHypotheticalMainLength + ") in container (" + container.name + ")");

   Dictionary<FlexItem, Vector2> grownAndShrunkSizes = null;
   if( leftoverSpace > 0 )
   {
    grownAndShrunkSizes = ResolveGrowSizes(container, childItems, leftoverSpace, baseChildSizes, containerContentSize);
   }
   else if( leftoverSpace < 0 )
   {
    grownAndShrunkSizes = ResolveShrinkSizes(container, childItems, -1f * leftoverSpace, baseChildSizes, containerContentSize);
   }
   else
   {
    grownAndShrunkSizes = new Dictionary<FlexItem, Vector2>(baseChildSizes);
   }

   /** Set the final-sizes using the output of grown-and-shrunk sizing
    * (almost all sizing is now complete)
    */
   Dictionary<FlexItem, Vector2> finalUnpaddedSizes = new Dictionary<FlexItem, Vector2>(grownAndShrunkSizes);
   Dictionary<FlexItem, Vector2> finalSizes = new Dictionary<FlexItem, Vector2>(finalUnpaddedSizes);

   /** Step4: cross-axis sizing (overrides existing size in ONE dimension only) */
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "]: Setting cross-axis of flexbox (" + container.name + ") using self- full size = " + containerPaddedSize + ", and internal content size: " + containerContentSize);
   switch( container.alignItems )
   {
    case AlignItems.STRETCH:
    {
     foreach( var child in childItems )
     {
      Vector2 childSize = finalSizes[child];

      switch( container.direction )
      {
       case FlexDirection.COLUMN:
       case FlexDirection.COLUMN_REVERSED:
       {
        float marginDelta = child.cssMargins.HorizontalValueOrZero(containerPaddedSize);
        childSize.x = containerContentSize.x - marginDelta;
       }
        break;
       case FlexDirection.ROW:
       case FlexDirection.ROW_REVERSED:
       {
        float marginDelta = child.cssMargins.VerticalValueOrZero(containerPaddedSize);
        childSize.y = containerContentSize.y - marginDelta;
       }
        break;
      }

      finalSizes[child] = childSize;
     }
    }
     break;
   }

   /** ---- TODO: cross-axis sizing and grow/shrink above can cause any ASPECT_FILL children to need a second
    * run of all the code to this point, since one of their dimensions has changed, causing the other to need to also
    * change.
    */

   /***** Update all the child's RectTransform.size all in one go *****/
#if UNITY_EDITOR
#else
   if( Application.isEditor )
   {
    //Debug.Log("UnityEditor core bug (please complain to Unity!): you are about to get your console spammed with fake WARNING messages generated by a Unity employee who incorrectly added them in Unity 2017, and failed to patch them out again. With the source-code version of this plugin, there is a hack that can workaround this UnityEngine bug, but it is impossible in compiled code.");
   }
#endif
   foreach( var child in childItems )
   {
    var childTransform = child.transform as RectTransform;

    /** NB: min/max were already used in some of algorithm calculations earlier on - e.g. the grow/shrink phase - so
     * we could theoretically optimize very very slightly by only doing the calc there (but would have to ALSO move this
     * code into the "no grow, no shrink" codepath that is otherwise not doing min/max at all) ... but the cost of these
     * compares here is tiny, and it makes the overall algorithm clearer / easier to read / more obviously "guarantees"
     * that min/max will be respected.
     */
    float w = finalSizes[child].x;
    if( child.cssMinWidth.mode != CSS3LengthType.NONE ) w = Mathf.Max(child.cssMinWidth.ValueOrZero(containerContentSize.x), w);
    if( child.cssMaxWidth.mode != CSS3LengthType.NONE ) w = Mathf.Min(child.cssMaxWidth.ValueOrZero(containerContentSize.x), w);

    float h = finalSizes[child].y;
    if( child.cssMinHeight.mode != CSS3LengthType.NONE ) h = Mathf.Max(child.cssMinHeight.ValueOrZero(containerContentSize.y), h);
    if( child.cssMaxHeight.mode != CSS3LengthType.NONE ) h = Mathf.Min(child.cssMaxHeight.ValueOrZero(containerContentSize.y), h);

    /** Update the finalUnpaddedSizes */
    if( child.boxSizing == BoxSizing.CONTENT_BOX
        && child.cssPadding.isActive )
    {
     Vector2 v;
     v.x = w - child.cssPadding.HorizontalValueOrZero(containerContentSize);
     v.y = h - child.cssPadding.VerticalValueOrZero(containerContentSize);
     finalUnpaddedSizes[child] = v; // so that the POSITIONING algorithm that follows is guaranteed to be using these values
    }
    else
     finalUnpaddedSizes[child] = new Vector2(w, h); // so that the POSITIONING algorithm that follows is guaranteed to be using these values

    /** Update the finalSizes */
    finalSizes[child] = new Vector2(w, h); // so that the POSITIONING algorithm that follows is guaranteed to be using these values
    childTransform.SetSize(finalSizes[child]); // recap: if CONTENT_BOX, this is "base + padding", otherwise it's just "base"

    /** Update the anchors to sensible values, allowing us to implement positioning with a sensible amount of code instead of a huge number of switches and extra unnecessary calcs */
    childTransform.anchoredPosition = Vector2.zero;
    childTransform.anchorMax = childTransform.anchorMin = 0.5f * Vector2.one; // cleans up the anchors and puts them somewhere more sensible than Unity's HorizontalLayoutGroup which sticks them at 0,0
   }

   /*********** DEBUG AT END OF SIZING ROUTINES *************/
   if( container.showDebugMessages )
   {
    foreach( var child in childItems )
    {
     var childTransform = child.transform as RectTransform;
     Debug.Log("[" + container.name + "]: Sizing complete, child (" + child.name + ") has size: " + childTransform.rect.size);
    }
   }

   /** ------------- Sizing complete, now position: --------------------- */
   /* ... NB: in what follows, use "finalSizes[child]" which ignores padding, rather than "childTransform.rect" which has been shrunk by padding already */

   /** ------ Step 1: Sort children before acting ------ */
   /** Sort them according to Order, but defaulting to Transform order where .order values are identical */
   List<FlexItem> unityArrayAsList = new List<FlexItem>(childItems);
   List<FlexItem> flexOrderedChildren = new List<FlexItem>(childItems);
   flexOrderedChildren.Sort((a, b) =>
    {
     int flexOrder = a.flexOrder.CompareTo(b.flexOrder);
     if( flexOrder != 0 )
      return flexOrder;
     else
     {
      int aIndex = unityArrayAsList.IndexOf(a);
      int bIndex = unityArrayAsList.IndexOf(b);
      if( aIndex < bIndex )
       return -1;
      else
       return 1;
     }
    }
   );

   /** ------ Step 2: Calculate empty space versus occupied space ------ */
   float totalConsumedByFlexItems = 0f; // this may be LESS THAN the available length, if no items can grow (grow = 0 for all), or they have max length (not supported yet)
   float totalConsumedByMargins = 0f;
   /** Total Consumed Length = totalConsumedByFlexItems + totalConsumedByMargins */
   float totalConsumedLength = 0;

   float unusedLeftoverSpace = 0f;
   Vector2 totalConsumed = Vector2.zero;
   Vector2 totalMargins = Vector2.zero;
#if TRUE // TODO: remove this, merge it or delete it!
   foreach( var child in flexOrderedChildren )
   {
    totalConsumed += finalSizes[child];
    if( child.cssMargins.isActive )
    {
     totalMargins += new Vector2(child.cssMargins.HorizontalValueOrZero(containerContentSize), child.cssMargins.VerticalValueOrZero(containerContentSize));
     //if( container.showDebugMessages ) Debug.Log("ULTRA-DEBUG: childMargins = "+child.margins.HorizontalValueOrZero(containerContentSize)+","+ child.margins.VerticalValueOrZero(containerContentSize) +"   for child = "+child.name);
    }
   }

   switch( container.direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.COLUMN_REVERSED:
     totalConsumedByFlexItems = totalConsumed.y;
     totalConsumedByMargins = totalMargins.y;
     unusedLeftoverSpace = containerContentSize.y - totalChildMargins.y - totalConsumed.y;
     break;
    case FlexDirection.ROW:
    case FlexDirection.ROW_REVERSED:
     totalConsumedByFlexItems = totalConsumed.x;
     totalConsumedByMargins = totalMargins.x;
     unusedLeftoverSpace = containerContentSize.x - totalChildMargins.x - totalConsumed.x;
     break;
   }

   totalConsumedLength = totalConsumedByFlexItems + totalConsumedByMargins;
#else
  foreach( var child in flexOrderedChildren )
  {
   var childTransform = child.transform as RectTransform;
   float childLength = 0f;
   float lengthAddedByMargins = 0f;
   switch( container.direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.COLUMN_REVERSED:
     childLength = finalSizes[child].y; // (includes its internal padding)
     lengthAddedByMargins = child.margins.VerticalValueOrZero(SizeMinusPadding(baseChildSizes[selfLayoutItem]));  
     break;
    case FlexDirection.ROW:
    case FlexDirection.ROW_REVERSED:
     childLength = finalSizes[child].x; // (includes its internal padding)
     lengthAddedByMargins = child.margins.HorizontalValueOrZero(SizeMinusPadding(baseChildSizes[selfLayoutItem]));
     break;
   }
   if( container.showDebugMessages ) Debug.Log( "["+container.name+"] Consumed length: adding mainlength = "+childLength+" (+"+lengthAddedByMargins+" from margin) for child = "+child+" (has sizeDelta = "+childTransform.sizeDelta );
   totalConsumedLength += childLength + lengthAddedByMargins;
  }
  unusedLeftoverSpace = (containerLengthMinusChildMarginsPadding + totalChildMarginsMainAxis) - totalConsumedLength;
#endif
   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Consumed length: total = " + totalConsumedByFlexItems + " .. leftoverspace = " + unusedLeftoverSpace + " =" + containerContentSize + " -" + totalChildMargins + " -" + totalConsumed);

   /** ------ Step 3: Calculate advances needed to maintain alignment while positioning children ------ */
   /** Main Axis */
   /** ... main axis: calculate advance to support different "justify" settings */
   float offset = 0;
   float objectAdvanceUnpadded = 0;
   float preAdvance = 0f, postAdvance = 0f;
   switch( container.justifyContent )
   {
    case FlexJustify.SPACE_BETWEEN:
     preAdvance = 0f;
     postAdvance = (childItems.Count < 2) ? 0 : unusedLeftoverSpace / (childItems.Count - 1);
     break;
    case FlexJustify.SPACE_AROUND:
     postAdvance = 0.5f * unusedLeftoverSpace / (childItems.Count);
     preAdvance = postAdvance;
     break;
    case FlexJustify.SPACE_EVENLY:
     preAdvance = unusedLeftoverSpace / (childItems.Count + 1);
     postAdvance = 0f; // we ignore it, technically there's a postadvance on the last item but it's not needed, already accounted for 
     break;
   }

   switch( container.direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.ROW_REVERSED:
     preAdvance = -1f * preAdvance;
     postAdvance = -1f * postAdvance;
     break;
    case FlexDirection.COLUMN_REVERSED:
    case FlexDirection.ROW:
     break;
   }

   if( container.showDebugMessages ) Debug.LogFormat("[" + container.name + "] Advances calc'd: preAdvance ={0}, postAdvance = {1}", preAdvance, postAdvance);

   /** ------ Step 4: Calculate start point ------ */
   /** ... main axis: calculate start point */
   float justifiedStartMainAxis = 0;
   float mainAxisContainerContentLength = 0, mainAxisContainerPaddedLength = 0, crossAxisContainerPaddedLength = 0, crossAxisContainerContentLength = 0;
   switch( container.direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.COLUMN_REVERSED:
     mainAxisContainerContentLength = containerContentSize.y;
     crossAxisContainerContentLength = containerContentSize.x;
     mainAxisContainerPaddedLength = containerPaddedSize.y;
     crossAxisContainerPaddedLength = containerPaddedSize.x;
     break;

    case FlexDirection.ROW:
    case FlexDirection.ROW_REVERSED:
     mainAxisContainerContentLength = containerContentSize.x;
     crossAxisContainerContentLength = containerContentSize.y;
     mainAxisContainerPaddedLength = containerPaddedSize.x;
     crossAxisContainerPaddedLength = containerPaddedSize.y;
     break;

    default:
     Debug.LogError("Impossible. C# compiler doesn't fully support enum-switches");
     break;
   }

   switch( container.justifyContent )
   {
    case FlexJustify.SPACE_BETWEEN:
    case FlexJustify.SPACE_AROUND:
    case FlexJustify.SPACE_EVENLY:
     switch( container.direction )
     {
      case FlexDirection.COLUMN:
      case FlexDirection.ROW:
       justifiedStartMainAxis = -1f * mainAxisContainerContentLength / 2f;
       break;
      case FlexDirection.COLUMN_REVERSED:
      case FlexDirection.ROW_REVERSED:
       justifiedStartMainAxis = mainAxisContainerContentLength / 2f;
       break;
     }

     break;
    case FlexJustify.START:
     switch( container.direction )
     {
      case FlexDirection.COLUMN:
      case FlexDirection.ROW:
       justifiedStartMainAxis = -1f * mainAxisContainerContentLength / 2f;
       break;
      case FlexDirection.COLUMN_REVERSED:
      case FlexDirection.ROW_REVERSED:
       justifiedStartMainAxis = -1f * mainAxisContainerContentLength / 2f + totalConsumedLength;
       break;
     }

     break;
    case FlexJustify.CENTER:
     switch( container.direction )
     {
      case FlexDirection.COLUMN:
      case FlexDirection.ROW:
       justifiedStartMainAxis = -1f * totalConsumedLength / 2f;
       break;
      case FlexDirection.COLUMN_REVERSED:
      case FlexDirection.ROW_REVERSED:
       justifiedStartMainAxis = totalConsumedLength / 2f;
       break;
     }

     break;
    case FlexJustify.END:
     switch( container.direction )
     {
      case FlexDirection.COLUMN:
      case FlexDirection.ROW:
       justifiedStartMainAxis = mainAxisContainerContentLength / 2f - totalConsumedLength;
       break;
      case FlexDirection.COLUMN_REVERSED:
      case FlexDirection.ROW_REVERSED:
       justifiedStartMainAxis = mainAxisContainerContentLength / 2f;
       break;
     }

     break;
   }

   switch( container.direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.COLUMN_REVERSED:
     justifiedStartMainAxis *= -1f; // flip top/bottom, because Unity counts Y going up, instead of down
     break;
   }

   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Layout(" + container.justifyContent + ") -- justifiedStartMainAxis = " + justifiedStartMainAxis + "     (ContainerContentLength = " + mainAxisContainerContentLength + ", totalConsumedByItems = " + totalConsumedByFlexItems + ", totalConsumedByMargins = " + totalConsumedByMargins + ")");

   if( container.showDebugMessages ) Debug.Log("[" + container.name + "] Layout(" + container.justifyContent + "). Advances: pre=" + preAdvance + ", post=" + postAdvance + " -- mainAxisContainerLEngth = " + mainAxisContainerPaddedLength + " Unity first offset = " + justifiedStartMainAxis);

   /** ------ Step 5: Position each item along main-axis and cross-axis ------ */

   bool mainIsVertical = container.direction == FlexDirection.COLUMN || container.direction == FlexDirection.COLUMN_REVERSED;
   foreach( var child in flexOrderedChildren )
   {
    var childTransform = child.transform as RectTransform;

    /** ... main axis: calculate advance for adding this child (including its internal padding) */
    switch( container.direction )
    {
     case FlexDirection.COLUMN:
      objectAdvanceUnpadded = -1f * finalUnpaddedSizes[child].y;
      break;
     case FlexDirection.COLUMN_REVERSED:
      objectAdvanceUnpadded = finalUnpaddedSizes[child].y;
      break;
     case FlexDirection.ROW:
      objectAdvanceUnpadded = finalUnpaddedSizes[child].x;
      break;
     case FlexDirection.ROW_REVERSED:
      objectAdvanceUnpadded = -1f * finalUnpaddedSizes[child].x;
      break;
    }

    /** Positioning: */
    float positionX, positionY;
    positionX = positionY = -1;

    /** ... pre-calculate the left/top/right/bottom margins since they're used both in main and cross axis positioning */
    // TODO: these should incorporate the AUTO and PERCENT etc values
    float spacingLeft = 0, spacingRight = 0, spacingTop = 0, spacingBottom = 0;
    if( child.cssMargins.isActive )
    {
     spacingLeft += child.cssMargins.LeftOrZero(containerContentSize);
     spacingRight += child.cssMargins.RightOrZero(containerContentSize);
     spacingTop += child.cssMargins.TopOrZero(containerContentSize);
     spacingBottom += child.cssMargins.BottomOrZero(containerContentSize);
    }

    if( child.boxSizing == BoxSizing.CONTENT_BOX
        && child.cssPadding.isActive )
    {
     spacingLeft += child.cssPadding.LeftOrZero(containerContentSize);
     spacingRight += child.cssPadding.RightOrZero(containerContentSize);
     spacingTop += child.cssPadding.TopOrZero(containerContentSize);
     spacingBottom += child.cssPadding.BottomOrZero(containerContentSize);
    }

    /** ... main axis: position it */
    float mainOffsetFromParentPadding = 0;
    if( mainIsVertical )
    {
     mainOffsetFromParentPadding = (-containerPaddingInternal.height / 2f); // reset the origin to top edge
     mainOffsetFromParentPadding += containerPaddingInternal.y; // add on the top padding from parent

     mainOffsetFromParentPadding *= -1f;
    }
    else
    {
     mainOffsetFromParentPadding = (-containerPaddingInternal.width / 2f); // reset the origin to left hand edge
     mainOffsetFromParentPadding += containerPaddingInternal.x; // add on the left-hand padidng from parent
    }

    float marginAdvance = 0;
    switch( container.direction )
    {
     case FlexDirection.COLUMN:
     case FlexDirection.COLUMN_REVERSED:
      positionY = mainOffsetFromParentPadding + justifiedStartMainAxis + offset + preAdvance - ( /* -1 because Unity does y reversed */ spacingTop) + objectAdvanceUnpadded / 2f;
      marginAdvance = /* -1 because Unity does y reversed */ -(spacingTop + spacingBottom);
      if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "] position will be: **" + positionY + "** parentpadding:" + mainOffsetFromParentPadding + " + mainaxisJustification:" + justifiedStartMainAxis + " +offset:" + offset + " + preadv:" + preAdvance + " - spacingTop: " + spacingTop + " + 0.5*objectsize:" + objectAdvanceUnpadded + "");
      break;
     case FlexDirection.ROW:
     case FlexDirection.ROW_REVERSED:
      positionX = mainOffsetFromParentPadding + justifiedStartMainAxis + offset + preAdvance + spacingLeft + objectAdvanceUnpadded / 2f;
      if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "] position will be: **" + positionX + "** parentpadding:" + mainOffsetFromParentPadding + " + mainaxisJustification:" + justifiedStartMainAxis + " +offset:" + offset + " + preadv:" + preAdvance + " + spacingLeft: " + spacingLeft + " + 0.5*objectsize:" + objectAdvanceUnpadded + "");
      marginAdvance = spacingLeft + spacingRight;
      break;

     default:
      Debug.LogError("Impossible enum value in setting main-axis position");
      break;
    }

    if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "] ...change to position: (" + positionX + "," + positionY + ") offset: " + (preAdvance + marginAdvance + objectAdvanceUnpadded + postAdvance));
    offset += preAdvance + marginAdvance + objectAdvanceUnpadded + postAdvance;


    /** Cross Axis */
    /** Cross-axis margin handling is bizarre and complicated (many strange behaviours due to the CSS3 spec).
     *  ... first you position according to center/start/end/etc
     *  ... then you SELECTIVELY IGNORE margins based on whichever align-items value you had (row + center honours top + bottom; row + start IGNORES bottom, etc)
     */
    float crossFullLength = crossAxisContainerContentLength;
    float crossChildLength = 0f;
    switch( container.direction )
    {
     case FlexDirection.COLUMN:
     case FlexDirection.COLUMN_REVERSED:
      crossChildLength = finalSizes[child].x;
      break;
     case FlexDirection.ROW:
     case FlexDirection.ROW_REVERSED:
      crossChildLength = finalSizes[child].y;
      break;
    }

    if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "] ...cross-axis full length: " + crossFullLength + ", cross-axis child-length: " + crossChildLength);

    /** ... cross-axis: calculate center point along cross axis */
    float crossAxisPosition = 0f;
    float offsetFromMargins = 0f;
    switch( container.alignItems )
    {
     case AlignItems.CENTER:
     {
      crossAxisPosition = 0f;
      if( mainIsVertical )
       offsetFromMargins = (spacingLeft - spacingRight) / 2f;
      else
       offsetFromMargins = (spacingTop - spacingBottom) / 2f;
     }
      break;

     case AlignItems.START:
     case AlignItems.STRETCH:
     {
      crossAxisPosition = 0f - crossFullLength / 2f + crossChildLength / 2f;
      if( mainIsVertical )
       offsetFromMargins = spacingLeft;
      else
       offsetFromMargins = spacingTop;
     }
      break;

     case AlignItems.END:
     {
      crossAxisPosition = 0f + crossFullLength / 2f - crossChildLength / 2f;
      if( mainIsVertical )
       offsetFromMargins = 0 - (spacingRight);
      else
       offsetFromMargins = 0 - (spacingBottom);
     }
      break;

     case AlignItems.BASELINE:
     {
      Debug.LogError("FlexBox layouts: 'alignItems=Baseline' not supported");
     }
      break;
    }

    float crossOffsetFromParentPadding = 0;
    if( mainIsVertical )
    {
     crossOffsetFromParentPadding = (-containerPaddingInternal.width / 2f); // reset the origin to left hand edge
     crossOffsetFromParentPadding += containerPaddingInternal.x; // add on the left-hand padidng from parent
    }
    else
    {
     crossOffsetFromParentPadding = (-containerPaddingInternal.height / 2f); // reset the origin to top edge
     crossOffsetFromParentPadding += containerPaddingInternal.y; // add on the top padding from parent
    }

    /** ... cross-axis: calculate final position on cross-axis dimension */
    switch( container.direction )
    {
     case FlexDirection.COLUMN:
     case FlexDirection.COLUMN_REVERSED:
      positionX = crossAxisPosition + offsetFromMargins + crossOffsetFromParentPadding;
      break;
     case FlexDirection.ROW:
     case FlexDirection.ROW_REVERSED:
      positionY = /** because y-axis is reversed in Unity coords vs flexbox coords */ -1f * (offsetFromMargins + crossAxisPosition + crossOffsetFromParentPadding);
      break;

     default:
      Debug.LogError("Impossible enum value in setting cross-axis position");
      break;
    }

    if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "] ...cross-axis changes to position: (" + positionX + "," + positionY + ")");

    /** Position it */

    //offsetFromMargins = 0;
    childTransform.anchoredPosition = /*containerContentOffset +*/ new Vector2(positionX, positionY);
   }

   //if( container.showDebugMessages ) Debug.Log("..Ending self-layout; total active self-layout calls will be: "+(FlexboxElement.totalActiveLayoutSelfAndChildren-1));

   if( isSelfSizeResolvedYet )
   {
    result.resolvedSize = containerPaddedSize;
    return result;
   }
   else
   {
    result.resolvedSize = null;
    return result;
   }
  }

  /**
   * Only a separate method because it ORIGINALLY needed to be called repeatedly (1, 2, or 3 times depending on how many cross-dependencies
   * are in the layout hierarchy)
   *
   * This method adds up: base-sizes of all children, adds their margins, and - if they have a box-model that requires adding padding on the outside - adds their padding.
   * 
   * @param parentContainerSize the EFFECTIVE internal size of parent container (i.e. AFTER subtracting any padding, depending on box-mode)
   * @return the total size of all child-content, arranged along the container's axis (FlexContainer.direction). This gives a starting point from which to process grow and shrink (or not, if container is using basis=CONTENT)
   */
  private static int ResolveAsManyChildrenAsPossible(FlexContainer container, RectTransform.Axis mainAxis, HashSet<FlexItem> resolvedItems, HashSet<FlexItem> unResolvedItems, Dictionary<FlexItem, Vector2> baseSizes, List<FlexItem> childItems, Vector2? parentSizeOrNull, out float totalOuterHypotheticalMainLength, out float largestBaseCrossAxisWidth, out Vector2 totalChildMargins, out Vector2 totalChildPaddingsGrowingSize)
  {
   HashSet<FlexItem> newlyResolvedItems;
   int totalResolved = 0;

   /** Step 1: Base sizes */
   do
   {
    newlyResolvedItems = ResolveBaseSizes(container, mainAxis, unResolvedItems, resolvedItems, baseSizes, parentSizeOrNull);
    totalResolved += newlyResolvedItems.Count;
    if( container.showDebugMessages ) Debug.Log("[" + container.name + "]:Removing " + newlyResolvedItems.Count + " resolved items from: " + unResolvedItems.Count + " unresolved, adding them to: " + resolvedItems.Count + " resolved ... ");

    foreach( var item in newlyResolvedItems )
     unResolvedItems.Remove(item);

    resolvedItems.UnionWith(newlyResolvedItems);
   } while( newlyResolvedItems.Count > 0 );

   /** Step 2: Calculate total Base size (recap: doesnt care about padding, nor margin) */
   totalOuterHypotheticalMainLength = 0f;
   foreach( var child in childItems )
   {
    if( !baseSizes.ContainsKey(child) )
     continue; // if self not resolved, child may be unresolvable

    //ULTRA debug:
    if( container.showDebugMessages ) Debug.Log("child [" + child + "] y adding to totalHypotheticalLength (child's baseLength = " + baseSizes[child] + ") : " + HypotheticalLength( child, container.direction, baseSizes[child], parentSizeOrNull));
    totalOuterHypotheticalMainLength += HypotheticalLength( child, container.direction, baseSizes[child], parentSizeOrNull);
   }

   /** ... 2b: Need to calc maximum Base width (for cross-axis sizing, and for doing inferred layout of parent-container size)
    * (recap: doesnt care about padding or margin)
    */
   largestBaseCrossAxisWidth = 0f;
   foreach( var child in childItems )
   {
    if( !baseSizes.ContainsKey(child) )
     continue; // if self not resolved, child may be unresolvable

    switch( container.direction )
    {
     case FlexDirection.COLUMN:
     case FlexDirection.COLUMN_REVERSED:
      largestBaseCrossAxisWidth = Mathf.Max(largestBaseCrossAxisWidth, baseSizes[child].x);
      break;
     case FlexDirection.ROW:
     case FlexDirection.ROW_REVERSED:
      largestBaseCrossAxisWidth = Mathf.Max(largestBaseCrossAxisWidth, baseSizes[child].y);
      break;
    }
   }

   /**
    * Child margins are needed to be recalculated everytime the parent container size is recalculated, which
    * happens once per this method call; so we do the child-margin recalc inside this method to ensure it's
    * always up-to-date
    *
    * In the CONTENT_BOX mode, child padding ALSO needs to be recalculated every time (but in BORDER_BOX mode,
    * padding is ignored here)
    */
   totalChildMargins = new Vector2();
   totalChildPaddingsGrowingSize = new Vector2();
   foreach( var child in childItems )
   {
    switch( container.direction )
    {
     case FlexDirection.COLUMN:
     case FlexDirection.COLUMN_REVERSED:
      if( child.cssMargins.isActive )
      {
       totalChildMargins.x = Mathf.Max(totalChildMargins.x, child.cssMargins.HorizontalValueOrZero(parentSizeOrNull ?? Vector2.zero));
       totalChildMargins.y += child.cssMargins.VerticalValueOrZero(parentSizeOrNull ?? Vector2.zero);

       if( child.boxSizing == BoxSizing.CONTENT_BOX
           && child.cssPadding.isActive )
       {
        totalChildPaddingsGrowingSize.x = Mathf.Max(totalChildPaddingsGrowingSize.x, child.cssPadding.HorizontalValueOrZero(parentSizeOrNull ?? Vector2.zero));
        totalChildPaddingsGrowingSize.y += child.cssPadding.VerticalValueOrZero(parentSizeOrNull ?? Vector2.zero);
       }
      }

      break;
     case FlexDirection.ROW:
     case FlexDirection.ROW_REVERSED:
      if( child.cssMargins.isActive )
      {
       totalChildMargins.x += child.cssMargins.HorizontalValueOrZero(parentSizeOrNull ?? Vector2.zero);
       totalChildMargins.y = Mathf.Max(totalChildMargins.y, child.cssMargins.VerticalValueOrZero(parentSizeOrNull ?? Vector2.zero));

       if( child.boxSizing == BoxSizing.CONTENT_BOX
           && child.cssPadding.isActive )
       {
        totalChildPaddingsGrowingSize.x += child.cssPadding.HorizontalValueOrZero(parentSizeOrNull ?? Vector2.zero);
        totalChildPaddingsGrowingSize.y = Mathf.Max(totalChildPaddingsGrowingSize.y, child.cssPadding.VerticalValueOrZero(parentSizeOrNull ?? Vector2.zero));
       }
      }

      break;
    }
   }

   if( container.showDebugMessages ) Debug.Log("Total base: " + totalOuterHypotheticalMainLength + ", largest Cross: " + largestBaseCrossAxisWidth + ", total child-margins: " + totalChildMargins);

   return totalResolved; // Needed by some callers to know if this method is having any effect, or whether they should break out of while-loop
  }
  
  public static float HypotheticalLength(FlexItem child, FlexDirection direction, Vector2 size, Vector2? parentSize)
  {
   switch( direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.COLUMN_REVERSED:
#if UNITY_2018_1_OR_NEWER
     return HypotheticalHeight( child, size.y, parentSize?.y ?? 0 );
#else
					return HypotheticalHeight(size.y, parentSize.HasValue ? parentSize.Value.y : 0);
#endif

    case FlexDirection.ROW:
    case FlexDirection.ROW_REVERSED:
#if UNITY_2018_1_OR_NEWER
     return HypotheticalWidth( child, size.x, parentSize?.x ?? 0 );
#else
					return HypotheticalWidth(size.x, parentSize.HasValue ? parentSize.Value.x : 0);
#endif
    default:
     Debug.LogError("Impossible. C# compiler has failed you. Find a better programming language");
     throw new Exception("C# sucks. 24151242132");
   }
  }

  private static HashSet<FlexItem> ResolveBaseSizes(FlexContainer container, RectTransform.Axis mainAxis, HashSet<FlexItem> possiblyResolvableItems, HashSet<FlexItem> resolvedItems, Dictionary<FlexItem, Vector2> baseSizes, Vector2? parentSizeOrNull = null)
  {
   var result = new HashSet<FlexItem>();
   bool isMainAxisHorizontal = mainAxis == RectTransform.Axis.Horizontal;
//  RectTransform.Axis crossAxis = isMainAxisHorizontal ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;
//  var parentGroupLayoutItem = GetComponent<FlexboxElement>();
   bool parentIsResolved = parentSizeOrNull != null;

   foreach( var child in possiblyResolvableItems )
   {
//   var childTransform = child.transform as RectTransform;
//   FlexboxLayoutGroup childFlexBox = child as FlexboxLayoutGroup;

    /** Skip items that use PERCENTage paddings if self-size isn't resolved yet */
    if( child.cssPadding.isActive
        && child.cssPadding.requiresParentSizeToResolve
        && !parentIsResolved )
     continue;

    /** Attempt to resolve the child item */
    switch( child.flexBasis.mode )
    {
     case FlexBasis.CONTENT:
     {
      FlexContainer childAsContainer = child.GetComponent<FlexContainer>();

      if( canProvideAContentSize(child, parentSizeOrNull) )
      {
       Vector2 resolvedSize = ContentSize( child, parentSizeOrNull);
       HypotheticalSize( child, ref resolvedSize, parentSizeOrNull ?? Vector2.zero);
       result.Add(child);
       baseSizes[child] = resolvedSize;
      }
      else
      {
       Debug.LogError("Not supported yet: flex-item with basis = CONTENT but not able to infer content-size for this item: " + child.name);
      }

     }
      break;

     case FlexBasis.LENGTH:
     case FlexBasis.AUTO: // AUTO is auto-handled within the FlexItem.BaseLength() function
     case FlexBasis.PERCENT:
     {
      if( child.flexBasis.mode != FlexBasis.PERCENT
          || parentIsResolved )
      {
       Vector2 resolvedSize;

       float childBaseLength = BaseLength( child, mainAxis, parentSizeOrNull);
       float childCrossLength;
       if( container.alignItems == AlignItems.STRETCH && parentSizeOrNull != null ) // major performance optimization: don't calculate child's CROSS length if parent is overriding anyway (saves us doing CONTENT calcs, which are often expensive in deep hierarchies)
        childCrossLength = isMainAxisHorizontal ? (parentSizeOrNull ?? Vector2.zero).y : (parentSizeOrNull ?? Vector2.zero).x;
       else
        childCrossLength = CrossLength( child, mainAxis, parentSizeOrNull);

       resolvedSize.x = isMainAxisHorizontal ? childBaseLength : childCrossLength;
       resolvedSize.y = isMainAxisHorizontal ? childCrossLength : childBaseLength;

       if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "]: resolved " + child.flexBasis.mode + " basis to: " + resolvedSize + " (parent size = " + parentSizeOrNull + ")");
       //TODO: cannot reproduce this bug since fixing the Space_Between bug:       Debug.LogError("BUG: if both objects are AUTO (ie no baseSize exists) AND they have differnt GROW factors, the ratios-of-grow-applied are FUBAR");

       HypotheticalSize( child, ref resolvedSize, parentSizeOrNull ?? Vector2.zero);
       result.Add(child);
       baseSizes[child] = resolvedSize;
      }
      else
      {
       if( container.showDebugMessages ) Debug.LogWarning("Parent Flexbox container (" + container + ") of child (" + child + ") has non-explicit size, so I cannot YET (might succeed on future layout call?) auto-layout this child Flex item (" + child + ") because it is dynamically sized based on parent size");
      }
     }
      break;
#if LITE_VERSION
#else
     case FlexBasis.ASPECT_FIT:
     {
      float aspectRatio = child.flexBasis.value; // treat the basisValue as an Aspect-Ratio fraction
      if( parentIsResolved )
      {
       Vector2 parentGroupSize = parentSizeOrNull ?? Vector2.zero;

       /** Use the parent's cross-axis length to obtain an ideal size,
        * then shrink it to fit if larger than parent's own size (maintaining the ratio)
        */
       float aspectBaseLength = 0;
       switch( container.direction )
       {
        case FlexDirection.COLUMN:
        case FlexDirection.COLUMN_REVERSED:
        {
         aspectBaseLength = 1f / aspectRatio * (parentGroupSize.x - child.cssMargins.HorizontalValueOrZero(parentGroupSize)); // the cross-axis length, not main axis
         aspectBaseLength = Mathf.Min(aspectBaseLength, parentGroupSize.y - child.cssMargins.VerticalValueOrZero(parentGroupSize));
        }
         break;
        case FlexDirection.ROW:
        case FlexDirection.ROW_REVERSED:
        {
         aspectBaseLength = aspectRatio * (parentGroupSize.y - child.cssMargins.VerticalValueOrZero(parentGroupSize)); // the cross-axis length, not main axis
         aspectBaseLength = Mathf.Min(aspectBaseLength, parentGroupSize.x - child.cssMargins.HorizontalValueOrZero(parentGroupSize));
        }
         break;
       }

       Vector2 resolvedSize = new Vector2(aspectBaseLength, aspectBaseLength);

       if( container.showDebugMessages ) Debug.Log("[" + container.name + "." + child.name + "]: resolved " + child.flexBasis.mode + " basis to: " + resolvedSize + " (parent size = " + parentSizeOrNull + ")");

       HypotheticalSize( child, ref resolvedSize, parentSizeOrNull ?? Vector2.zero);
       result.Add(child);
       baseSizes[child] = resolvedSize;
      }
     }
      break;
#endif

     default:
      Debug.LogError("Not supported yet: flexbox-basis = " + child.flexBasis.mode);
      break;
    }
   }

   return result;
  }

  private static Dictionary<FlexItem, Vector2> ResolveGrowSizes(FlexContainer container, List<FlexItem> childItems, float excessToFill, Dictionary<FlexItem, Vector2> baseSizes, Vector2? parentSize)
  {
   Dictionary<FlexItem, Vector2> grownSizes = new Dictionary<FlexItem, Vector2>(baseSizes);

   /** TODO: First pass: Allocate space to the children's margins if they are "auto" */

//TODO:  Debug.LogError("Grow is ignoring the min-height: adding a min-height bigger than the assigned height, in a place where some Grow is possible, is causing the height-based Spare space to be allocated, instead of the height-modified-by-minheight-based Spare space");

   /** Final pass: Allocate all remaining space as padding around items, proportional to flex-grow */
   float totalGrowWeight = 0;
   foreach( var child in childItems )
   {
    totalGrowWeight += Mathf.Max(0f, child.flexGrow);
   }

   if( totalGrowWeight == 0f ) // all items refused to grow
    return new Dictionary<FlexItem, Vector2>(baseSizes);

   /** Re-allocate any excesses that go above a child's max (width|height) */
   float allocatingExcess = excessToFill;
   float unusedExcess = 0;
   List<FlexItem> growingChildren = new List<FlexItem>(childItems);
   /** ... looping until either all excess has found a home, or all children have hit their MAX width|height limits */
   while( growingChildren.Count > 0 && allocatingExcess > 0 )
   {
    for( int i = 0; i < growingChildren.Count; i++ )
    {
     var child = growingChildren[i];
     float fractionToConsume = Mathf.Max(0f, child.flexGrow) / totalGrowWeight;
     float offeredExcess = fractionToConsume * allocatingExcess;
     float acceptedExcess;
     switch( container.direction )
     {
      case FlexDirection.COLUMN:
      case FlexDirection.COLUMN_REVERSED:
       if( child.cssMaxHeight.mode == CSS3LengthType.NONE )
        acceptedExcess = offeredExcess;
       else
       {
#if UNITY_2018_1_OR_NEWER
       float maxHeight = child.cssMaxHeight.ValueOrZero( parentSize?.y );
#else
        float maxHeight = child.cssMaxHeight.ValueOrZero(parentSize.HasValue ? parentSize.Value.y : 0);
#endif
        float height = grownSizes[child].y;
        acceptedExcess = Mathf.Min((maxHeight - height), offeredExcess);
       }

       grownSizes[child] += new Vector2(0, acceptedExcess);
       break;
      case FlexDirection.ROW:
      case FlexDirection.ROW_REVERSED:
       if( child.cssMaxHeight.mode == CSS3LengthType.NONE )
        acceptedExcess = offeredExcess;
       else
       {
#if UNITY_2018_1_OR_NEWER
       float maxWidth = child.cssMaxWidth.ValueOrZero( parentSize?.x );
#else
        float maxWidth = child.cssMaxWidth.ValueOrZero(parentSize.HasValue ? parentSize.Value.x : 0);
#endif
        float width = grownSizes[child].x;
        acceptedExcess = Mathf.Min((maxWidth - width), offeredExcess);
       }

       grownSizes[child] += new Vector2(acceptedExcess, 0);
       break;
      default:
       Debug.LogError("Impossible. C# compiler sucks.");
       acceptedExcess = 0;
       break;
     }

     //ULTRA debug: if( container.showDebugMessages ) Debug.Log("[" + this.name + "." + child.name + "]: grow, excess offered (frac:" + fractionToConsume + ") = " + offeredExcess + ", accepted = " + acceptedExcess + ", max-(w,h) = " + child.maxWidth + ", " + child.maxHeight); 

     if( acceptedExcess < offeredExcess )
     {
      unusedExcess += (offeredExcess - acceptedExcess);
      growingChildren.RemoveAt(i);
      i--; // because we just removed one
     }
    }

    allocatingExcess = unusedExcess;
    unusedExcess = 0;
   }

   return grownSizes;
  }

  private static float _ShrinkWeightAdjustedForChild(FlexDirection direction, FlexItem child, Vector2 baseSize)
  {
   /** Note that Shrink calculations are different from Grow:
     *
     * Grow only looks at the weights in the items,
     * Shrink does looks at weights AND ALSO premultiplies by the relative baseSizes!
     */
   float childShrinkWeight = Mathf.Max(0f, child.flexShrink);
   float childShrinkWeightAdjusted = 0f;
   switch( direction )
   {
    case FlexDirection.COLUMN:
    case FlexDirection.COLUMN_REVERSED:
     childShrinkWeightAdjusted = childShrinkWeight * baseSize.y;
     break;
    case FlexDirection.ROW:
    case FlexDirection.ROW_REVERSED:
     childShrinkWeightAdjusted = childShrinkWeight * baseSize.x;
     break;
   }

   return childShrinkWeightAdjusted;
  }

  private static Dictionary<FlexItem, Vector2> ResolveShrinkSizes(FlexContainer container, List<FlexItem> childItems, float extraSpaceNeeded, Dictionary<FlexItem, Vector2> baseSizes, Vector2 parentSize)
  {
   Dictionary<FlexItem, Vector2> shrunkSizes = new Dictionary<FlexItem, Vector2>(baseSizes);

   float totalShrinkWeight = 0;
   foreach( var child in childItems )
   {
    /** Note that Shrink calculations are different from Grow:
     *
     * Grow only looks at the weights in the items,
     * Shrink does looks at weights AND ALSO premultiplies by the relative baseSizes!
     */
    totalShrinkWeight += _ShrinkWeightAdjustedForChild(container.direction, child, baseSizes[child]);
   }

   bool _preventOverflow = false; // overflow-handling was added to the Flexbox spec later on; this is a quick hack if you need it.
   bool simulateEqualShrinkWeights = false;
   if( totalShrinkWeight == 0f ) // all items refused to shrink
   {
    if( _preventOverflow )
    {
     simulateEqualShrinkWeights = true;
    }
    else
     return new Dictionary<FlexItem, Vector2>(baseSizes);
   }

   /** Re-allocate any excesses that go above a child's max (width|height) */
   float allocatingSpace = extraSpaceNeeded;
   float unprovidedSpace = 0;
   List<FlexItem> shrinkingChildren = new List<FlexItem>(childItems);
   /** ... looping until either all needed space has been sourced, or all children have hit their MIN width|height limits */
   while( shrinkingChildren.Count > 0 && allocatingSpace > 0 )
   {
    for( int i = 0; i < shrinkingChildren.Count; i++ )
    {
     var child = shrinkingChildren[i];
     float fractionToConsume = simulateEqualShrinkWeights ? 1f / childItems.Count : _ShrinkWeightAdjustedForChild(container.direction, child, baseSizes[child]) / totalShrinkWeight;
     float requestedSpace = fractionToConsume * allocatingSpace;
     float providedSpace;
     //ULTRA debug: if( container.showDebugMessages ) Debug.Log( "[" + container.name + "] ResolveShrink: child " + child + " is surrendering " + fractionToConsume + ", supplying " + (extraSpaceNeeded * fractionToConsume) + " out of " + extraSpaceNeeded );
     switch( container.direction )
     {
      case FlexDirection.COLUMN:
      case FlexDirection.COLUMN_REVERSED:
       if( child.cssMinHeight.mode == CSS3LengthType.NONE )
        providedSpace = requestedSpace;
       else
       {
        float minHeight = child.cssMinHeight.ValueOrZero(parentSize.y);
        float height = shrunkSizes[child].y;
        providedSpace = Mathf.Min((height - minHeight), requestedSpace);
       }

       shrunkSizes[child] -= new Vector2(0, providedSpace);
       break;
      case FlexDirection.ROW:
      case FlexDirection.ROW_REVERSED:
       if( child.cssMinWidth.mode == CSS3LengthType.NONE )
        providedSpace = requestedSpace;
       else
       {
        float minWidth = child.cssMinWidth.ValueOrZero(parentSize.x);
        float width = shrunkSizes[child].x;
        providedSpace = Mathf.Min((width - minWidth), requestedSpace);
       }

       shrunkSizes[child] -= new Vector2(providedSpace, 0f);
       break;

      default:
       Debug.LogError("Impossible. C# compiler sucks.");
       providedSpace = 0;
       break;
     }

     if( providedSpace < requestedSpace )
     {
      unprovidedSpace += (requestedSpace - providedSpace);
      shrinkingChildren.RemoveAt(i);
      i--; // because we just removed one
     }

    }

    allocatingSpace = unprovidedSpace;
    unprovidedSpace = 0;
   }

   return shrunkSizes;
  }

  private static Vector2 ParentSizeOf(FlexItem item, bool showDebugMessages = false)
  {
   Vector2 parentSize;
   if( item.transform.parent != null )
    parentSize = (item.transform.parent as RectTransform).rect.size;
   else
   {
    if( showDebugMessages )
    {
     Debug.LogWarning("Asked to calculate padding, which depends on the containing-object's size - but this item has no parent, so using self-size instead (not really supported by CSS/flexbox)");
    }

    parentSize = (item.transform as RectTransform).rect.size;
   }

   return parentSize;
  }

  private static Vector2 ParentSizeOf(FlexContainer container, bool showDebugMessages = false)
  {
   Vector2 parentSize;
   if( container.transform.parent != null )
    parentSize = (container.transform.parent as RectTransform).rect.size;
   else
   {
    if( showDebugMessages )
    {
     Debug.LogWarning("Asked to calculate padding, which depends on the containing-object's size - but this item has no parent, so using self-size instead (not really supported by CSS/flexbox)");
    }

    parentSize = (container.transform as RectTransform).rect.size;
   }

   return parentSize;
  }

  private static Rect CalculateInternalPaddingAndChildOffset(FlexItem item, bool showDebugMessages)
  {
   if( item != null
       && item.cssPadding.isActive )
   {
    Vector2 parentSize = ParentSizeOf(item);
    Vector2 position = Vector2.zero;
    Vector2 paddingAdded = Vector2.zero;
    switch( item.boxSizing )
    {
     case BoxSizing.BORDER_BOX:
      position.x = item.cssPadding.LeftOrZero(parentSize);
      position.y = item.cssPadding.TopOrZero(parentSize);

      paddingAdded.x = item.cssPadding.HorizontalValueOrZero(parentSize);
      paddingAdded.y = item.cssPadding.VerticalValueOrZero(parentSize);
      break;

     case BoxSizing.CONTENT_BOX:
      break;
    }

    return new Rect(position, paddingAdded);
   }
   else
    return Rect.zero;
  }
  /**
		 * Mainly created to allow FlexBasis=CONTENT situations to be resolved in a modular, extensible way,
		 * but also proves useful to allow fallback-behaviour for the many cases where a size (either main-axis
		 * or cross-axis) is missing and Flexbox demands we infer it from the content-size(s).
		 */
  private static bool canProvideAContentSize( FlexItem item, Vector2? parentContainerSizeOrNull)
  {
   switch( item.flexBasis.mode )
   {
    case FlexBasis.CONTENT:
     if( null != item.GetComponent<Text>() )
      return true;
     else if( null != item.GetComponent<FlexContainer>() )
     {
      return BaseSizeOfBox(item.GetComponent<FlexContainer>()) != null; // TODO: provide an extra method in IFlexboxLayoutAlgorithm that answers this question without having to actually DO the full layout equation!
     }
     else
      return false;

    case FlexBasis.LENGTH:
    case FlexBasis.AUTO: // AUTO is auto-handled within the FlexItem.BaseLength() function
     return true;

    case FlexBasis.PERCENT:
#if LITE_VERSION
#else
    case FlexBasis.ASPECT_FIT:
#endif
     return parentContainerSizeOrNull != null;

    default:
     return false;
   }
  }
  
  /** Unity made this a class, and it often gets created inside deep loops, so we re-use it here to keep garbage down */
  private static TextGenerator _reusableTextGenerator;
  /*
		 * TODO: this method returns ZERO unless basis = CONTENT (which works, because we only invoke it in CONTENT mode), but we should move all .size code into here for clarity and maintainability
		 */
		private static Vector2 ContentSize( FlexItem child, Vector2? parentContainerSize = null)
		{
			switch( child.flexBasis.mode )
			{
				case FlexBasis.CONTENT: // override CONTENT sizing for FlexContainers only
				{
					FlexContainer fc = child.GetComponent<FlexContainer>();
					if( fc != null )
					{
						return BaseSizeOfBox(fc) ?? Vector2.zero;
					}
					else if( child.GetComponent<Text>() != null )
					{
						/**
					 * Calculate a content size for UnityEngine.UI.Text components using Unity's (broken + buggy + unfixed in 2019)
					 * internal text-layout engine...
					 */
						FlexContainer parentContainer = (child.transform.parent != null) ? (child.transform.parent.gameObject.GetComponent<FlexContainer>()) : null;


						/** Attempt to set a base length by pre-emptively formatting the Text */
						Text uiText = child.GetComponent<Text>();

						TextGenerationSettings textSettings = new TextGenerationSettings();
						textSettings.textAnchor = uiText.alignment;
						textSettings.color = uiText.color;
						textSettings.pivot = Vector2.zero;
						textSettings.richText = uiText.supportRichText;
						textSettings.font = uiText.font;
						textSettings.fontSize = uiText.fontSize;
						textSettings.fontStyle = uiText.fontStyle;
						textSettings.generateOutOfBounds = true;
						textSettings.lineSpacing = 1f;
						textSettings.resizeTextForBestFit = uiText.resizeTextForBestFit;
						textSettings.resizeTextMinSize = uiText.resizeTextMinSize;
						textSettings.resizeTextMaxSize = uiText.resizeTextMaxSize;
						textSettings.scaleFactor = 1f;

						bool attemptInfiniteWidthLayout = parentContainer == null || (parentContainer.direction == FlexDirection.ROW
						                                                              || parentContainer.direction == FlexDirection.ROW_REVERSED);
						if( attemptInfiniteWidthLayout )
						{
							textSettings.horizontalOverflow = HorizontalWrapMode.Overflow; // don't wrap when parent is allowing infinite width
							textSettings.verticalOverflow = VerticalWrapMode.Overflow;

							textSettings.generationExtents = new Vector2(1f, (parentContainerSize ?? Vector2.one).y);
						}
						else
						{
							textSettings.horizontalOverflow = HorizontalWrapMode.Wrap; // parent has fixed width, so wrap horizontally
							textSettings.verticalOverflow = VerticalWrapMode.Overflow;

							textSettings.generationExtents = new Vector2((parentContainerSize ?? Vector2.one).x, 1f);
						}

						if( _reusableTextGenerator == null )
							_reusableTextGenerator = new TextGenerator();
						textSettings.updateBounds = true; // necessary! Undocumented by the programmers who work at Unity Corp
						_reusableTextGenerator.Populate(uiText.text, textSettings);
						Vector2 resolvedSize = _reusableTextGenerator.rectExtents.size;
						if( parentContainer != null && parentContainer.showDebugMessages ) Debug.Log("Resolved size for TEXT component in parent (size=" + parentContainerSize + ") with " + uiText.text.Length + " chars = " + _reusableTextGenerator.rectExtents + " pref w = " + _reusableTextGenerator.GetPreferredWidth(uiText.text, textSettings) + ", h = " + _reusableTextGenerator.GetPreferredHeight(uiText.text, textSettings));

						return resolvedSize;
					}

					break;
				}
			}

			return Vector2.zero;
		}
 
  private static float CrossLength( FlexItem child, RectTransform.Axis mainAxis, Vector2? containerSizeOrNull)
  {
   float value;
   bool mainAxisIsHorizontal = mainAxis == RectTransform.Axis.Horizontal;

   bool isContentDeclared = canProvideAContentSize(child, containerSizeOrNull);
   bool isDefaultCrossDeclared = mainAxisIsHorizontal ? child.cssDefaultHeight.hasValue : child.cssDefaultWidth.hasValue;

   if( isDefaultCrossDeclared ) // User can specify a width/height if not-main, OR if no-flex-basis
    value = mainAxisIsHorizontal ? child.cssDefaultHeight.ValueOrZero((containerSizeOrNull ?? Vector2.zero).y) : child.cssDefaultWidth.ValueOrZero((containerSizeOrNull ?? Vector2.zero).x);
   else
    value = mainAxisIsHorizontal ? ContentSize( child, containerSizeOrNull).y : ContentSize( child, containerSizeOrNull).x;

   return value;
  }
 
  private static float BaseLength( FlexItem child, RectTransform.Axis axis, Vector2? containerSizeOrNull)
  {
   float value;
   bool axisIsHorizontal = axis == RectTransform.Axis.Horizontal;

   float basisDeclared = 12345; // I hate Microsoft. Hate, hate, hate. This is INCORRECT and is REQUIRED to workaround bugs in C# specification.
   bool isBasisDeclared = false;
   switch( child.flexBasis.mode )
   {
    case FlexBasis.LENGTH:
     basisDeclared = child.flexBasis.value;
     isBasisDeclared = true;
     break;

    case FlexBasis.PERCENT:
     if( containerSizeOrNull == null )
     {
      Debug.LogError("Asked for BaseLength on element that is in PERCENT mode but whose parent is not-yet-resolved");
     }
     else
     {
      Vector2 v = containerSizeOrNull ?? Vector2.zero;
      basisDeclared = child.flexBasis.value * (axisIsHorizontal ? v.x : v.y) / 100f;
      isBasisDeclared = true;
     }

     break;

    case FlexBasis.AUTO:
     basisDeclared = 0f;
     isBasisDeclared = false; // AUTO definitely means there's no specified flex-basis!
     break;
   }


   bool isContentDeclared = false;
   bool isDefaultMainDeclared = axisIsHorizontal ? child.cssDefaultWidth.hasValue : child.cssDefaultHeight.hasValue;

   float contentDeclared = 0; // TODO: this should use "content-width" instead of 0
   float defaultDeclared = axisIsHorizontal ? child.cssDefaultWidth.ValueOrZero((containerSizeOrNull ?? Vector2.zero).x) : child.cssDefaultHeight.ValueOrZero((containerSizeOrNull ?? Vector2.zero).y);


   if( isBasisDeclared ) // Main-axis always uses flex-basis if available
    value = basisDeclared;
   else if( isDefaultMainDeclared ) // User can specify a width/height if not-main, OR if no-flex-basis
    value = defaultDeclared;
   else
    value = contentDeclared;

   return value;
  }
 
  private static float HypotheticalWidth( FlexItem item, float width, float parentWidth)
  {
   if( item.cssMinWidth.mode != CSS3LengthType.NONE )
    width = Mathf.Max(width, item.cssMinWidth.ValueOrZero(parentWidth));
   if( item.cssMaxWidth.mode != CSS3LengthType.NONE )
    width = Mathf.Min(width, item.cssMaxWidth.ValueOrZero(parentWidth));
   return width;
  }

  private static float HypotheticalHeight( FlexItem item, float height, float parentHeight)
  {
   if( item.cssMinHeight.mode != CSS3LengthType.NONE )
    height = Mathf.Max(height, item.cssMinHeight.ValueOrZero(parentHeight));
   if( item.cssMaxHeight.mode != CSS3LengthType.NONE )
    height = Mathf.Min(height, item.cssMaxHeight.ValueOrZero(parentHeight));
   return height;
  }

  private static void HypotheticalSize( FlexItem item, ref Vector2 size, Vector2 containerSize)
  {
   size.x = HypotheticalWidth( item, size.x, containerSize.x);
   size.y = HypotheticalHeight( item, size.y, containerSize.y);
  }
  
 }
}