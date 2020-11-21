using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Flexbox4Unity
{
/**
 * This allows you to swap in and out different layout algorithms (including ones optimzed for performance,
 * for specific situations - or even for your own customised version of Flexbox!). The main use is for me
 * to provide an "official" algorithm that I'm continuously updating, but allow you to rollback to previous
 * versions in any situation where you've deliberately or accidentally built a GUI that depends on some
 * old / buggy behaviour of the earlier algorithm.
 * 
 * Unity (all versions up to early 2020) does not yet support C# interfaces in the editor, so this
 * is an interface faked using abstract class. However, now that it's already an abstract class, I am
 * termporarily using that feature to also embed the default "relayout algorithm" (this is the meta-algorithm
 * that decides WHEN to invoke the "layout algorithm", and how). TODO: move the relayout algorithm to a separate, swappable, ScriptableObject
 *
 */
	public abstract class IFlexboxLayoutAlgorithmV3 : ScriptableObject
	{
		/**
		 * Subclasses override this to implement their specific algorithm
		 */
		public abstract void AlgorithmLayout(FlexContainer fc, Vector2 availableSize );
		/**
		 * Each algorithm should have a unique name (this is used to create SO instances in Editor)
		 */
		public abstract string defaultAssetName { get; }

		public abstract ReadOnlyCollection<string> featureDescription { get; }

		/** UnityEditor's built-in API for stating if Layout is in progress is broken in all versions up to 2019, so we have to track it manually */
		private int _layoutsStartedUnfinished = 0;
		public bool isLayoutInProgress
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
		
		public class FlexRelayoutEvent : UnityEvent<FlexContainer>
		{
		}

		public static FlexRelayoutEvent onRelayout = new FlexRelayoutEvent();

		/**
		 * This will usually recurse up the tree until it finds a fixed-size container, and then the fixed-size container
		 * will do a single re-layout of its direct children, which MAY as a side-effect trigger relayouts of the sub-trees
		 *
		 * It will also FORCE an internal relayout of its own children if its size didn't change (if its size did change,
		 * then the internal relayout has already been triggered, and doesn't need to be done again)
		 *
		 * TODO: we could simplify this by making the On..TransformChanged code empty when layout-in-progress ... then THIS method could remove the check and just ALWAYS force an internal relayout, whether or not the size had changed
		 */
		public void ReLayout( FlexContainer container )
		{
			if( container.settings.debugRelayoutCalls ) Debug.Log( "["+container.hierarchicalName+"] ReLayout invoked..." );
			var parentContainer = container.transform.parent?.gameObject.GetComponent<FlexContainer>(); 
			if( container.transform.parent != null && parentContainer != null )
			{
				if( container.settings.debugRelayoutCalls || container.showDebugMessages || parentContainer.showDebugMessages ) Debug.Log("["+container.hierarchicalName+"] ReLayout() - delegating to parent" );
				
				Vector2 preReLayoutSize = container.rectTransform.rect.size;
				// recurse to the parent
				ReLayout( parentContainer );
				Vector2 postReLayoutSize = container.rectTransform.rect.size;

				if( Vector2.Distance(preReLayoutSize, postReLayoutSize) < 1f )
				{
					if( container.showDebugMessages || parentContainer.showDebugMessages ) Debug.Log("["+container.hierarchicalName+"] ReLayout() - parent laid me out, but didn't change my size, so now MANUALLY re-laying out my children" );
					ReLayoutChildrenOnly(container);
				}
				else if( container.showDebugMessages || parentContainer.showDebugMessages ) Debug.Log("["+container.hierarchicalName+"] ReLayout() - parent laid me out, and changed my size (from: "+preReLayoutSize+" --> "+postReLayoutSize+") - so not doing any further internal layout" );
			}
			else
			{
				if( container.showDebugMessages ) Debug.Log("["+container.hierarchicalName+"] ReLayout() - know my own size, so laying out my children" );
				ReLayoutChildrenOnly(container);
			}
		}
		
		/**
		 * This should only really be called automatically, by FlexContainer, when RectTransform.sizeChanges
		 * 
		 * In all other situations, you should call ReLayout() instead, since it's possible/probable that the
		 * parent-of-the-container may/will need to relayout too.
		 */
		public void ReLayoutChildrenOnly(FlexContainer fc)
		{
			if( fc.settings.debugRelayoutCalls ) Debug.Log( "["+fc.hierarchicalName+"] ReLayoutChildrenOnly invoked..." );
			Vector2 preProvidedSize = fc.rectTransform.rect.size;
			
			if( fc.showDebugMessages )
				Debug.Log("["+fc.hierarchicalName+"] Starting algorithm.layout using "+defaultAssetName+" (num already running: "+_layoutsStartedUnfinished);
			_layoutsStartedUnfinished++;
			try
			{
				AlgorithmLayout( fc, preProvidedSize );
			}
			finally
			{
				_layoutsStartedUnfinished--;
			}
   
			if( fc.showDebugMessages )
				Debug.Log("["+fc.hierarchicalName+"] -- Finished: algorithm.layout using "+defaultAssetName+" (num layouts still running: "+_layoutsStartedUnfinished);
		}

		#if IS_THIS_ACTUALLY_NEEDED
		/**
		 * Re-implementation of Unity's low-quality (and buggy) method: "LayoutRebuilder.ForceRebuildLayoutImmediate( rt );"
		 *
		 * 2020 THEORY: this was, more importantly, needed so that we DISABLE Unity's extremely slow and low-quality layout algorithm;
		 * without manually destroying Unity's algorithm, your performance goes from 300 FPS to 0.01 FPS (Unity's algorithm is truly terrible)
		 *
		 * This is necessary so that we can trigger Unity child-elements to update their own size when they're embedded inside
		 * Flexbox elements.
		 */
		private static void UnityGUI_ForceRebuildLayoutImmediate_ChildrenOf(RectTransform rt)
		{
			var uGUIBehavioursList = new List<ILayoutController>();
			
			LayoutRebuilder.ForceRebuildLayoutImmediate( rt );
			
			
			(rt.gameObject.GetComponents<ILayoutController>());
			componentList = new List<ILayoutController>() {rt.gameObject.GetComponent<ILayoutController>()};
			componentList.RemoveAll((Predicate<ILayoutController>) (e => e is Behaviour && !((Behaviour) e).isActiveAndEnabled));

			string goTitle = rt.gameObject.GetComponent<FlexItem>() != null
					? rt.gameObject.GetComponent<FlexItem>().hierarchicalName
					: (rt.gameObject.GetComponent<FlexContainer>() != null
						? rt.gameObject.GetComponent<FlexContainer>().hierarchicalName
						: rt.gameObject.name
						);
			if( Flexbox4UnityProjectSettings.sharedInstance.debugRefreshTriggers ) Debug.Log("RebuildLayoutImmediate (fast): found " + componentList.Count + " layout-items on GO = " + goTitle);
				
			//Image = ilayoutelement
			RawImage
			/** Re-implement the slow and badly designed implemention inside UnityUI's LayoutRebuilder.Rebuild.PerformLayoutControl(..) methods
			 *
			 * NB: UnityUI's Rebuild spams the CPU by running the layout algorithm 4 times in different ways, when only one is needed,
			 * so we skip the other 3 for any Flexbox4Unity classes, and only run the full four for UnityUI's classes
			 */
			if( componentList.Count > 0 )
			{
				/**
				 * Unity runs its algorithms in two passes, one pass for self-layout-controller subclasses, and a second pass for normal ones.
				 * 
				 * This is a side-effect of their choice to adopt a bad layout algorithm that has intrinsically low performance
				 * (instead of using an industry-standard algorithm, or writing a decent one)
				 */
				for( int index = 0; index < componentList.Count; ++index )
				{
					if( componentList[index] is ILayoutSelfController )
						(componentList[index] as ILayoutController).SetLayoutHorizontal();
				}

				for( int index = 0; index < componentList.Count; ++index )
				{
					if( !(componentList[index] is ILayoutSelfController) )
						(componentList[index] as ILayoutController).SetLayoutHorizontal();
				}
			}
		}
#endif
	}
}