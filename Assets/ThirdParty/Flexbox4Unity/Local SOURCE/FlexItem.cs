//#define USING_ASSEMBLIES_IN_PROJECT // If you're using Assemblies in your project, you can uncomment this and get cleaner code for UnityEditor features
#define EXTENDFLEXBOX_FLEXWRAP_GROWSHRINK // proprietary extension that allows you to grow/shrink the cross-axis of wrapped lines
#pragma warning disable 0219

using System;
using UnityEngine;
using System.Linq;
using IntelligentPluginTools;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Version = IntelligentPluginVersioning.Version;

namespace Flexbox4Unity
{
	/**
	 * Flexbox4Unity v2.0 onwards: one of the main classes (FlexItem and FlexContainer)
	 *
	 * Main Goal: keep this class as small as possible, and move any/all layout-specific features into modular
	 * layoutAlgorithm classes. The only methods that should be here are ones required by Unity (there are many
	 * of those!), and the fields/properties needed to fulfil CSS-Flexbox
	 */
	[RequireComponent(typeof(RectTransform))]
	public class FlexItem : UIBehaviour, ILayoutElement
	{
		public FlexboxBasis flexBasis = FlexboxBasis.Auto;

		public CSS3Length cssDefaultWidth = CSS3Length.None;
		public CSS3Length cssDefaultHeight = CSS3Length.None;

		public CSS3Length cssMinWidth = CSS3Length.None; // has to be named this to avoid collision with Unity's ILayout interface
		public CSS3Length cssMaxWidth = CSS3Length.None;
		public CSS3Length cssMinHeight = CSS3Length.None; // has to be named this to avoid collision with Unity's ILayout interface
		public CSS3Length cssMaxHeight = CSS3Length.None;

		/** Convenience method for Flex: cssMinWidth OR cssMinHeight - Flex can switch axis arbitrarily */
		public CSS3Length cssMin(RectTransform.Axis axis)
		{
			return axis == RectTransform.Axis.Horizontal ? cssMinWidth : cssMinHeight;
		}
		/** Convenience method for Flex: cssMaxWidth OR cssMaxHeight - Flex can switch axis arbitrarily */
		public CSS3Length cssMax(RectTransform.Axis axis)
		{
			return axis == RectTransform.Axis.Horizontal ? cssMaxWidth : cssMaxHeight;
		}
		
		/** Convenience method for Flex: cssDefaultWidth OR cssDefaultHeight - Flex can switch axis arbitrarily */
		public CSS3Length cssDefault(RectTransform.Axis axis)
		{
			return axis == RectTransform.Axis.Horizontal ? cssDefaultWidth : cssDefaultHeight;
		}

		/** Convenience method for Flex: clamp a length to min/max, automatically using width or height */ 
		public float cssClampLength( float length, RectTransform.Axis axis, Vector2? containerSize)
		{
			return CSS3Length.Clamp(cssMin(axis), cssMax(axis), length, axis.Length(containerSize));
		}
		/** Convenience method for Flex: clamp a length to min/max, automatically using width or height */ 
		public float cssClampLength( float length, RectTransform.Axis axis, VectorNullable2? containerSize)
		{
			return CSS3Length.Clamp(cssMin(axis), cssMax(axis), length, axis.Length(containerSize));
		}
		
#if LITE_VERSION
		public BoxSizing boxSizing
		{
			get { return BoxSizing.BORDER_BOX; }
		}
#else
	public BoxSizing boxSizing = BoxSizing.BORDER_BOX; // all CSS should use this, but CONTENT_BOX was added to CSS to workaround old Web Browsers (Internet Explorer, Netscape, etc)
#endif
		public float flexShrink = 1.0f;
		public float flexGrow = 1.0f;
		public int flexOrder = 0;

#if LITE_VERSION
		public CSS3Margins cssMargins
		{
			get { return new CSS3Margins(false); }
		}

		public CSS3Margins cssPadding
		{
			get { return new CSS3Margins(false); }
		}
#else
	public CSS3Margins cssMargins = new CSS3Margins(false);
	public CSS3Margins cssPadding = new CSS3Margins(false);
#endif

#if LITE_VERSION
		public bool isSelfAlignmentActive
		{
			get { return false; }
		}

		public AlignSelf alignSelf
		{
			get { return AlignSelf.CENTER; }
		}
#else
	public bool isSelfAlignmentActive = false;
	public AlignSelf alignSelf = AlignSelf.CENTER;
#endif

		#region Settings management
		[SerializeField] private Flexbox4UnityProjectSettings _settings;
		public Flexbox4UnityProjectSettings settings
		{
			get
			{
				if( _settings == null )
				{
					if( Application.isEditor )
						if( Application.isPlaying )
							throw new Exception( "(in-Editor): Please run the \"Tools > Flexbox > Auto-discover all RuntimeSettings\" tool! Flexbox settings missing at runtime, should have been pre-configured before entering playmode, for object = " + this );
						else
						{
							_settings = Flexbox4UnityProjectSettings.findProjectSettings;

						#if UNITY_EDITOR
							EditorSceneManager.MarkSceneDirty( PrefabStageUtility.GetCurrentPrefabStage() != null ? PrefabStageUtility.GetCurrentPrefabStage().scene : SceneManager.GetActiveScene() );
						#endif
						}
					else
						_settings = Flexbox4UnityProjectSettings.findProjectSettings;
				}

				return _settings;
			}
   
			set { _settings = value; }
		}
		public bool hasSettings { get { return _settings != null; } }
		public void _InternalWipeSettings()
		{
			_settings = null;
		}
		#endregion
		
		#region Proprietary extensions to Flexbox (may be ignored by some layout algorithms)
		public float flexWrapGrow = 1f;
		public float flexWrapShrink = 1f;
		#endregion
		
		#region Workaround major bugs in UnityUI / uGUI unfixed for 5+ years: Unity lies about Text.preferredSize / TextGenerator.cs is badly written and full of bugs
		/*****
		 * TL;DR: Unity wrote TextGenerator.cs extremely badly, filled it with bugs, and never bothered to test (or fix!) it,
		 * which causes ALL UNITYUI to ALWAYS HAVE THE WRONG SIZE for text, in ALL LAYOUT (this is the root cause of why a lot
		 * of UnityUI layouts look wrong with text: one class, one method, never implemented properly, and apparently never unit-tested)
		 *
		 * ... but we can do some clever workarounds. This is one of them.
		 */
		
		/**
		 * c.f. the major bugs in Unity's TextGenerator.cs ... our workaround is to tell the user what size Unity lied about,
		 * and although Unity gets it wrong, Unity is consistently wrong, and so the user can copy/paste this figure as the
		 * flex-grow, causing the object to automatically remain relatively correct to sibling text items - "fixing" the bug
		 * that Unity mis-reported the original size and so caused the initial size to be random (Unity returns neither the
		 * min size, nor the max size, nor the user-specified default size - but some bizarre number in between all three)
		 */
		public float lastReportedUnityTextWidthInPixels;
		#endregion

		public Version createdWithVersion = new Version(0, 0, 0); // defaults to non-existent version for old code that pre-dates the versioning plugin 
		public Version upgradedToVersion = new Version( 0, 0, 0 ); // defaults to non-existent version for old code that pre-dates the versioning plugin

		#region Advanced bonus settings
		public bool RelayoutWheneverSelected = false;
		#endregion
		
		#region UnityUI callbacks that we ignore - our algorithm is much faster than the UnityUI one

		public void CalculateLayoutInputHorizontal()
		{
		}

		public void CalculateLayoutInputVertical()
		{
		}

		public float minWidth
		{
			get { return 0f; }
		}

		public float preferredWidth
		{
			get { return 0f; }
		}

		public float flexibleWidth
		{
			get { return 0f; }
		}

		public float minHeight
		{
			get { return 0f; }
		}

		public float preferredHeight
		{
			get { return 0f; }
		}

		public float flexibleHeight
		{
			get { return 0f; }
		}

		public int layoutPriority
		{
			get { return 0; }
		}

		#endregion
		

		/************************************************************
  *
  * This is slightly insane.
  *
  * One of Unity's programmers made a mistake a few years ago, and it seems to have broken Unity's
  * build-system if you use Reset() or OnValidate() on any UIBehaviour subclass.
  *
  * (The UnityEditor DLL and the UnityEngine DLL have *incompatible* definitions of those methods - which should not
  * be possible!)
  *
  * The workaround we use for now:
  *   1. For source builds, we default to doing it correctly (IF UNITY_EDITOR will always be true)
  *   2. For source builds, when Unity builds the App/Game/Player, we do a HACK that tricks Unity into building, to workaround
  *        Unity's own HACK where they messed-up their own DLL.
  *   3. DLL builds of the project (e.g. the free LITE version) are incapable of processing
  * Reset() or OnValidate() properly. THIS IS A BUG CAUSED BY UNITY and as far as I can tell it is impossible to fix/workaround
  * until Unity fixes their internal DLL!
  ************************************************************/
#if UNITY_EDITOR
	protected override void Reset()
#else
		protected void Reset()
#endif
		{
			//Debug.Log("New component added, setting createdWithVersion & lastEditedWithVersion = "+EditorOnlyCurrentlyRunningVersion);
			
			upgradedToVersion = Flexbox4UnityProjectSettings.builtVersion;
			if( createdWithVersion < new Version(1, 0, 0) )
			{
				createdWithVersion = upgradedToVersion;

				// Trigger a single layout call, to make sure it gets laid-out the first time
				RelayoutAfterCreatingAtRuntime();
			}
			
			FlexboxActionHooks.shared.OnItemCreated.Invoke(this);
		}
	
	/**
   * Unity refuses to give us a callback when objects are added at runtime, so although we can auto-detect this in
   * Editor ... for runtime changes, you'll need to call this manually
   */
	public void RelayoutAfterCreatingAtRuntime()
	{
		FlexContainer fc = transform.parent == null ? null : transform.parent.GetComponent<FlexContainer>();
		if( fc != null )
			if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithm v2Algorithm )
				v2Algorithm.ReLayout(fc, RefreshLayoutMode.FORCE_RECURSE_UP_THEN_SELF_DOWN);
			else if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithmV3 v3Algorithm )
				v3Algorithm.ReLayout(fc);
	}

#if UNITY_EDITOR
	#region Automatic upgrade system
	private System.Reflection.Assembly _FindAssemblyContainingFlexboxEditorClasses()
	{
		var allAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach( var a in allAssemblies )
		{
			if( a.FullName.Equals("Flexbox4Unity") )
			{
				return a;
			}
			else if( a.FullName.StartsWith("Assembly-CSharp-Editor,") ) // ',' included to ignore  Assembly-CSharp-Editor-FirstPass
			{
				return a;
			}
		}

		return null;
	}

	[ContextMenu("Force Upgrade")]
	public void ForceUpgrade()
	{
#if USING_ASSEMBLIES_IN_PROJECT
		Flexbox4UnityProjectSettings.ProcessRequiredUpgrades(this);
#else // UnityEditor is badly designed and won't let you use [ContextMenu] from Editor classes, but it's an editor-only feature. We have to use reflection to invoke the static Editor methods
		/*Debugging Reflection in Unity, that runs differently in every Unity version, arrgggh!!!:
		 
		 foreach( var v in System.AppDomain.CurrentDomain.GetAssemblies() )
			Debug.Log( "assembly.fullname = "+v.FullName );
		*/

		Type utilityType = _FindAssemblyContainingFlexboxEditorClasses().GetTypes().FirstOrDefault(t => t.FullName.Contains("Flexbox4UnityProjectSettings"));
		utilityType.GetMethod("ProcessRequiredUpgrades", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(obj: null, parameters: new object[] {this});
#endif
	}

	[ContextMenu("Force Upgrade (all descendants)")]
	public void ForceUpgradeAllDescendants()
	{
#if USING_ASSEMBLIES_IN_PROJECT
foreach( var elem in GetComponentsInChildren<FlexboxItem>())
			Flexbox4UnityProjectSettings.ProcessRequiredUpgrades( elem );
#else // UnityEditor is badly designed and won't let you use [ContextMenu] from Editor classes, but it's an editor-only feature. We have to use reflection to invoke the static Editor methods
		Type utilityType = _FindAssemblyContainingFlexboxEditorClasses().GetTypes().FirstOrDefault(t => t.FullName.Contains("Flexbox4UnityProjectSettings"));

		foreach( var elem in GetComponentsInChildren<FlexItem>() )
			utilityType.GetMethod("ProcessRequiredUpgrades", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(obj: null, parameters: new object[] {elem});
#endif
	}
	
	#endregion
#endif

		#region Improve Unity's weak design of base MonoBehaviour/Component/Behaviour

		public RectTransform rectTransform
		{
			get
			{

				if( transform is RectTransform rt )
					return rt;
				else
				{
					throw new Exception( "This FlexItem is attached to a corrupt GameObject that does not have a RectTransform. Due to bugs in Unity (all versions), you must delete this GameObject and re-create it as a UnityUI GameObject: " + gameObject.name );
				}
			}
		}

		public string hierarchicalName
		{
			get
			{
				string hName = name;

				var parent = transform.parent;
				while( parent != null )
				{
					hName = parent.name + "." + hName;
					parent = parent.parent;
				}

				return hName;
			}
		}
		#endregion

		protected HierarchyObjectType parentObjectType
		{
			get
			{
				if( transform.parent == null )
					return HierarchyObjectType.NULL;

				GameObject goParent = transform.parent.gameObject;
				if( goParent.GetComponent<FlexItem>() != null
				    || goParent.GetComponent<FlexContainer>() != null )
					return HierarchyObjectType.FLEXBOX_COMPONENT;
				else if( goParent.transform is RectTransform )
					return HierarchyObjectType.RECTTRANSFORM_ONLY;
				else
					return HierarchyObjectType.TRANSFORM_ONLY;
			}
		}

		private void OnDrawGizmosSelected()
		{
			if( settings.drawFlexHierarchyUsing != GizmosRenderMode.NONE
#if UNITY_EDITOR // this hurts: Unity engineers made this an editor-only call, even though it is commonly *required* in NON-EDITOR classes (because OnDrawGizmos will ONLY work in non-editor classes). Bug in Unity all versions for last 10+ years :(
		    && Selection.Contains(gameObject)
#endif
			)
				DrawAsWireframe(0f, 0);
		}

	/**
   * InsetAmount is in pixels, but in a World-Space canvas, Unity will treat
   * it as meters.
   *
   * So we need to pre-multiply it by the canvas's scale every time we use it.
   */
		public virtual void DrawAsWireframe(float insetAmount = 0f, int selectionDepth = 0)
		{
			RectTransform rt = (transform as RectTransform);
			RectTransform rtParent = (transform.parent as RectTransform);
			Canvas canvas = this.Canvas();
			if( canvas != null )
			{
				Vector2 canvasScaleFactor = new Vector2( canvas.transform.localScale.x, canvas.transform.localScale.y );
				//DrawWireframeRectangle( rt.rect, (0.5f * rtParent.rect.size - 0.5f * rt.rect.size) + rt.anchoredPosition , Color.cyan );

				Vector3[] corners = new Vector3[4];
				rt.GetWorldCorners( corners );
				corners = ExtendedGizmos.InsetRectanglePixels( corners, canvasScaleFactor * insetAmount );

				ExtendedGizmos.DrawWireframeRectangleFractionalSides( corners, Color.grey, 0.5f );
				Gizmos.DrawLine( corners[0], corners[2] );
				Gizmos.DrawLine( corners[1], corners[3] );
			}
		}


		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();

			if( parentObjectType == HierarchyObjectType.RECTTRANSFORM_ONLY )
			{
				this.ExpandToFillParent();
			}
		}
	}
}