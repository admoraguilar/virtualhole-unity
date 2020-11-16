#define USE_HIGHER_PERFORMANCE_LAYOUT_ALGORITHM_INSTEAD_OF_UNITYS_SLOW_ONE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Flexbox4Unity
{
	public struct LayoutResult
	{
		public Vector2? resolvedSize;
	}
	
	public enum RefreshLayoutMode
	{
		RECURSE_UP_WHOLE_TREE,
		SELF_AND_DESCENDENTS_ONLY,

		// TODO: 2020 / Flexbox4Unity v2.1.x: remove the need for this special-case mode by integrating more deeply into Unity's internal (private) layout mechanisms
		FORCE_RECURSE_UP_THEN_SELF_DOWN // Special case: Unity's implementation of core GUI means that recurse up will NOT invoke a layout phase on children that don't change size; but if a flex container changes settings, we need BOTH things to happen 
	}

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
	public abstract class IFlexboxLayoutAlgorithm : ScriptableObject
	{
		public abstract bool isLayoutInProgress { get; }
		public abstract void Layout(FlexContainer fc);

		public abstract string defaultAssetName { get; }

		public class FlexRelayoutEvent : UnityEvent<FlexContainer>
		{
		}

		public static FlexRelayoutEvent onRelayout = new FlexRelayoutEvent();

		/**
			* This is the core replacement for UnityUI LayoutBuilder.ForceRebuildLayoutImmediate - our version is at least
			* 4 times faster, and up to 10000x faster for complex layouts.
			*/
		public void ReLayout(FlexContainer container, RefreshLayoutMode layoutMode)
		{
#if DEBUG_INTEGRATION_WITH_UNITY_UI_LAYOUT
		Debug.Log("["+name+"] "+GetType()+".RefreshLayout(). FlexContainer.layoutSelf calls in progress = "+totalActiveLayoutSelfAndChildren);
#endif
			
			/** NB: As part of the workaround for Unity's internal 'SendMessage' bug, we have to invoke this method
			 * asynchronously in-editor.
			 *
			 * A side effect of async invocation is that it triggers ANOTHER Bug in UnityEngine/UnityEditor, where
			 * the badly-implemented EditorApplication.update system will call this method an extra time after the
			 * Editor has already destroyed the object. To guard against that happening, we check if the Editor has
			 * already marked this object as "destroyed".
			 */
			if( container == null || container.gameObject == null )
			{
				if( container.settings.debugRefreshTriggers ) Debug.Log("ReLayout[null]: returning early, Unity has wiped some of the Editor data");
				return;
			}
			
			if( container.settings.debugRefreshTriggers ) Debug.Log("ReLayout[" + container.hierarchicalName + "], mode = "+layoutMode);

			/** Bug in Unity all versions from 4.6 onwards: "RectTransform" is a magic object, strangely implemented
			 * by the UnityUI team, and they hacked UnityEditor to auto-replace Transform with RectTransform *only if* you
			 * create your object within a Canvas.
			 *
			 * If you create your object anywhere else, it cannot be used, cannot be fixed, because the UnityUI team's hack
			 * can't be re-run after object-creation. It's tragic, but we're stuck with it.
			 */
			if( container.transform as RectTransform == null )
				return;

			// TODO: 2020 / Flexbox4Unity v2.x: remove the need for this special-case mode by integrating more deeply into Unity's internal (private) layout mechanisms
			if( layoutMode == RefreshLayoutMode.FORCE_RECURSE_UP_THEN_SELF_DOWN )
			{
				ReLayout(container, RefreshLayoutMode.RECURSE_UP_WHOLE_TREE);
				ReLayout(container, RefreshLayoutMode.SELF_AND_DESCENDENTS_ONLY);
			}
			else
			{

				/** NB: transform.parent will never be null in a project - but Unity bizarrely invokes this method on PREFABS when
				 * you hit Play in the Editor (never at any other time!). Unity says that prefabs don't exist at runtime, but this
				 * is apparently not quite true (2019)
				 */
				FlexContainer parentLayoutItem = container.transform.parent == null ? null : container.transform.parent.gameObject.GetComponent<FlexContainer>();

				if( onRelayout != null )
				{
//				Debug.Log("invoking relayouts...");
					onRelayout.Invoke(container);
				}

				switch( layoutMode )
				{
					case RefreshLayoutMode.RECURSE_UP_WHOLE_TREE:
						if( parentLayoutItem == null ) // not directly embedded inside any form of FlexboxLayoutGroup, so only refresh self and downwards:
							Replaceable_LayoutRebuilder_RebuildLayoutImmediate(container.transform as RectTransform);
						else
						{
							ReLayout(parentLayoutItem, RefreshLayoutMode.RECURSE_UP_WHOLE_TREE);
						}

						break;

					case RefreshLayoutMode.SELF_AND_DESCENDENTS_ONLY:
						Replaceable_LayoutRebuilder_RebuildLayoutImmediate(container.transform as RectTransform);
						break;
					
					default:
						throw new Exception("Impossible: 25kldfklj35");
				}
			}
		}

		private static void Replaceable_LayoutRebuilder_RebuildLayoutImmediate(RectTransform rt)
		{
#if USE_HIGHER_PERFORMANCE_LAYOUT_ALGORITHM_INSTEAD_OF_UNITYS_SLOW_ONE
			var componentList = new List<ILayoutController>(rt.gameObject.GetComponents<ILayoutController>());
			componentList = new List<ILayoutController>() {rt.gameObject.GetComponent<ILayoutController>()};
			componentList.RemoveAll((Predicate<ILayoutController>) (e => e is Behaviour && !((Behaviour) e).isActiveAndEnabled));

			string goTitle = rt.gameObject.GetComponent<FlexItem>() != null
					? rt.gameObject.GetComponent<FlexItem>().hierarchicalName
					: (rt.gameObject.GetComponent<FlexContainer>() != null
						? rt.gameObject.GetComponent<FlexContainer>().hierarchicalName
						: rt.gameObject.name
						);
			if( Flexbox4UnityProjectSettings.findProjectSettings?.debugRefreshTriggers ?? false ) Debug.Log("RebuildLayoutImmediate (fast): found " + componentList.Count + " layout-items on GO = " + goTitle);
				
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
			LayoutRebuilder.ForceRebuildLayoutImmediate( rt );
#else
		LayoutRebuilder.ForceRebuildLayoutImmediate( rt );
#endif
		}
	}
}