#define WORKAROUND_UNITY_API_MISSING_ONRECTTRANSFORMCHANGED_CALLBACK
//#define DEBUG_DISABLE_CUSTOM_INSPECTOR
#define USE_UNITY_SERIALIZED_OBJECTS_FOR_INSPECTOR
//April 2019: Eventually we can merge these, when Unity fixes their bug in Unity 2018+2019: #define UNITY_HAS_FIXED_MAJOR_SENDMESSAGE_BUG
#if UNITY_EDITOR // Workaround bugs in the UnityEditor build command (sometimes ignores the fact this file is inside an Editor folder!) 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine.UI;

namespace Flexbox4Unity
{
	[CustomEditor(typeof(FlexItem))]
	[CanEditMultipleObjects]
	public class EditorForFlexItem : Editor
	{
		private const string pBasis = "flexBasis";
		private const string pDefaultWidth = "cssDefaultWidth";
		private const string pDefaultHeight = "cssDefaultHeight";
		private const string pGrow = "flexGrow";
		private const string pShrink = "flexShrink";
		private const string pMargins = "cssMargins";
		private const string pPadding = "cssPadding";
		private const string pOrder = "flexOrder";
		private const string pEnabled_AlignSelf = "isSelfAlignmentActive";
		private const string pAlignSelf = "alignSelf";



		private static int _currentTab; // static makes Unity remember the selected tab when you select new objects
		
		
		
#if WORKAROUND_UNITY_API_MISSING_ONRECTTRANSFORMCHANGED_CALLBACK
		/** Because Unity refuses (we've been requesting this for almost TEN YEARS!) to add a callback for transform-changed,
		 * and because their implementation of RectTransformDimensionsChange() has never been implemented to fire
		 * for position-changes (even though it seems it should do, given what it was designed for), we have to
		 * "fake" a detection of RectTransform.position changing
		 *
		 * TODO: I found an undocumented Unity workaround for this a year ago, but now it's stopped working and I cannot
		 * remember how I did it (I had to decompile Unity's source code to think of it in the first place)
		 */
		private Vector2 _rectTransformLastPosition;
		private FlexItem _lastViewedItem;
#endif
		public override void OnInspectorGUI()
		{
			FlexItem value = target as FlexItem;
			FlexContainer itemAsContainer = value.gameObject.GetComponent<FlexContainer>();
			bool newSelection = _lastViewedItem != value;
			_lastViewedItem = value;

#if DEBUG_DISABLE_CUSTOM_INSPECTOR
		base.OnInspectorGUI();
#else

#if USE_UNITY_SERIALIZED_OBJECTS_FOR_INSPECTOR
			serializedObject.Update();
#endif

			/** bug in Unity all versions from 4.6 onwards: "RectTransform" is a magic object, 
			 * by the UnityUI team, and they hacked UnityEditor to auto-replace Transform with RectTransform *only if* you
			 * create your object within a Canvas.
			 *
			 * If you create your object anywhere else, it cannot be used, cannot be fixed, because the UnityUI team's hack
			 * can't be re-run after object-creation. It's tragic, but we're stuck with it.
			 */
			if( value.transform as RectTransform == null )
			{
				EditorGUILayout.HelpBox("Major bug in UnityEditor: UnityUI objects *must* be created as children of a Canvas object. UnityEditor requires this, or else it disables the RectTransform permanently. Please delete this object and try again.", MessageType.Error);
				return;
			}

			/** Add the tabbed toolbar OUTSIDE the change-check, so that tab changing doesnt trigger a re-save */
			GUILayout.Space(10f);
#if LITE_VERSION
			EditorGUILayout.HelpBox("Not available in LITE version: padding, margins, aspect-fit, self-align, box-mode", MessageType.Info);
#else
#endif
			_currentTab = GUILayout.Toolbar(_currentTab, new string[] {"Simple", "Advanced", "Help"});

			GUILayout.BeginVertical(new GUIStyle(GUI.skin.box) {margin = new RectOffset(10, 5, 0, 0)});
			switch( _currentTab )
			{
				case 0:
				{
					EditorGUI.BeginChangeCheck();
					bool changeAffectsSelfNotParent = false; // a nested change that requires special handling

					//GUILayout.Label("Flex-item:", EditorStyles.boldLabel);

					FlexContainer parentFlexContainer = (value.transform.parent == null) ? null : value.transform.parent.GetComponent<FlexContainer>();
					if( parentFlexContainer == null )
					{
						EditorGUILayout.HelpBox("Please insert this FlexItem into a flex-container - e.g. parent it with a FlexContainer", MessageType.Error);
					}

					GUILayout.Label("Size:", EditorStyles.boldLabel);
					EditorGUI.indentLevel++;

					var propBasis = serializedObject.FindProperty(pBasis);
					EditorGUILayout.PropertyField(propBasis, new GUIContent("Flex Basis"));
					if( parentFlexContainer != null )
					{
						
						float currentLength = (parentFlexContainer.direction == FlexDirection.ROW || parentFlexContainer.direction == FlexDirection.ROW_REVERSED)
							? value.rectTransform.rect.width
							: value.rectTransform.rect.height;
						float parentLength = (parentFlexContainer.direction == FlexDirection.ROW || parentFlexContainer.direction == FlexDirection.ROW_REVERSED)
							? parentFlexContainer.rectTransform.rect.width
							: parentFlexContainer.rectTransform.rect.height;
						string currentPercent = parentLength > 0
							? string.Format( "{0:0.00} %",(currentLength * 100f) / parentLength )
							: "[N/A]";
							EditorGUILayout.LabelField("\u00A0","<color=grey>Current: "+currentLength+" px ("+currentPercent+")</color>", new GUIStyle() { richText = true });
					}
					else
						EditorGUILayout.LabelField("\u00A0","<color=grey>Current: (not in a container)</color>", new GUIStyle() { richText = true });
					

#if LITE_VERSION
					if( value.flexBasis.mode == FlexBasis.ASPECT_FIT )
					{
						EditorGUI.indentLevel++;
						EditorGUILayout.HelpBox("'flex-basis : aspect-fit' only available in full version; please upgrade", MessageType.Error);
						EditorGUI.indentLevel--;
					}
#endif

					CustomSeparator();

					/** Width and Height section */
					{
						EditorGUILayout.PrefixLabel("Constraints:", EditorStyles.boldLabel);
						EditorGUI.indentLevel++;



						var space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(40));
						Vector2 indentOffset = 16f * EditorGUI.indentLevel * Vector2.right;
						space.position += indentOffset;
						space.size -= indentOffset;

						Vector2 lineHeight = new Vector2(space.size.x, 16f);
						var l1 = new Rect(space.position + new Vector2(0, 0f), lineHeight);
						var l2 = new Rect(space.position + new Vector2(0, 20f), lineHeight);

						EditorGUI.BeginChangeCheck();
						DrawSingleLineConstraint("Width", value.cssMinWidth, value.cssMaxWidth, l1);
						DrawSingleLineConstraint("Height", value.cssMinHeight, value.cssMaxHeight, l2);
						if( EditorGUI.EndChangeCheck() )
							FlexboxActionHooks.shared.OnConstraintsSet.Invoke(value);

						EditorGUI.indentLevel--;
					}

					using( new EditorGUI.DisabledScope(true) )
					{
						EditorGUILayout.LabelField("Box mode", "border-box");
					}

					EditorGUI.indentLevel--;

					{
						GUILayout.Label("Fill / expand to fit:", EditorStyles.boldLabel);
						EditorGUI.indentLevel++;
						
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(serializedObject.FindProperty(pGrow), new GUIContent("Grow"));
						if( EditorGUI.EndChangeCheck() )
							FlexboxActionHooks.shared.OnGrowSet.Invoke(value);
						
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(serializedObject.FindProperty(pShrink), new GUIContent("Shrink"));
						if( EditorGUI.EndChangeCheck() )
							FlexboxActionHooks.shared.OnShrinkSet.Invoke(value);
				
#if LITE_VERSION
#else
						/** Check for UnityUI child elements - UnityUI sucks, and always creates Images, Buttons, and Text at stupid
						 * tiny sizes, instead of doing the obvious correct default: maximizing them to fit the space they are added to
						 */
						List<RectTransform> resizeableChildren = new List<RectTransform>();
						foreach( Transform ct in value.transform ) // cannot use Unity's broken GetComponentsInChildren, because it returns componets in all GRANDChildren too...
						{
							//List<ILayoutElement> childUIs = new List<ILayoutElement>( ct.GetComponents<ILayoutElement>() );
							List<ICanvasElement> childUIs = new List<ICanvasElement>( ct.GetComponents<ICanvasElement>() );
							for( int i=0; i<childUIs.Count; i++ )
								/*if( childUIs[i] is FlexItem || childUIs[i] is FlexContainer )
									;
								else*/ if( (ct.transform as RectTransform).CouldExpandToFillParent() )
								{
									resizeableChildren.Add( ct.transform as RectTransform);
								}
						}

						if( resizeableChildren.Count > 0 )
						{
							if( GUILayout.Button("Resize children to fit self") )
							{
								EditorGUI.BeginChangeCheck();
								
								foreach( var rt in resizeableChildren )
								{
									rt.ExpandToFillParent();
								}
								
								if( EditorGUI.EndChangeCheck() )
									FlexboxActionHooks.shared.OnExpandChildrenToFitSelfSet.Invoke(value);
							}
						}
						#endif
						
#if LITE_VERSION
						using( new EditorGUI.DisabledScope(true) )
						{
							EditorGUILayout.LabelField("Margins", "None");
							EditorGUILayout.LabelField("Padding", "None");
						}
#endif
						EditorGUI.indentLevel--;
					}

#if LITE_VERSION
#else
					{
						GUILayout.Label("Margins:", EditorStyles.boldLabel);
						using( new GUILayout.HorizontalScope() )
						{
							Rect layoutArea = EditorGUILayout.GetControlRect(false); // bug in UnityEditor: this method ignores EditorGUI.indentLevel
							layoutArea = EditorGUI.PrefixLabel(layoutArea, new GUIContent(" ")); // bug in UnityEditor: only way to find out the area is to insert a "space" label; UnityEditor incorrectly treats "" as GUIContent.none
							EditorGUI.BeginChangeCheck();
							EditorGUI.PropertyField(layoutArea, serializedObject.FindProperty(pMargins + ".isActive"), new GUIContent("Use margins?"));
							if( EditorGUI.EndChangeCheck() )
								FlexboxActionHooks.shared.OnMarginsSet.Invoke(value);
						}

						if( value.cssMargins.isActive )
						{
							using( new GUILayout.HorizontalScope() )
							{
								GUILayout.Space(16f); // simulate indent

								using( new GUILayout.VerticalScope(GUI.skin.box) )
								{
									float targetWidthOfEachProperty = 130f;

									SerializedProperty mainProperty = serializedObject.FindProperty(pMargins);
									using( new GUILayout.HorizontalScope() )
									{
										GUILayout.Space((EditorGUIUtility.currentViewWidth - targetWidthOfEachProperty) / 2f);
										EditorGUILayout.PropertyField(mainProperty.FindPropertyRelative("top"), GUIContent.none);
										GUILayout.Space((EditorGUIUtility.currentViewWidth - targetWidthOfEachProperty) / 2f);
									}

									GUILayout.Space(10f);

									using( new GUILayout.HorizontalScope() )
									{
										EditorGUILayout.PropertyField(mainProperty.FindPropertyRelative("left"), GUIContent.none);
										GUILayout.Space((EditorGUIUtility.currentViewWidth - 2f * targetWidthOfEachProperty));
										EditorGUILayout.PropertyField(mainProperty.FindPropertyRelative("right"), GUIContent.none);
									}

									GUILayout.Space(5f);

									using( new GUILayout.HorizontalScope() )
									{
										GUILayout.Space((EditorGUIUtility.currentViewWidth - targetWidthOfEachProperty) / 2f);
										EditorGUILayout.PropertyField(mainProperty.FindPropertyRelative("bottom"), GUIContent.none);
										GUILayout.Space((EditorGUIUtility.currentViewWidth - targetWidthOfEachProperty) / 2f);
									}
								}

							}
						}
					}
#endif

#if LITE_VERSION
#else
					{
						if( value.boxSizing == BoxSizing.BORDER_BOX )
							EditorGUI.BeginChangeCheck();

						GUILayout.Label("Padding:", EditorStyles.boldLabel);
						using( new GUILayout.HorizontalScope() )
						{
							Rect layoutArea = EditorGUILayout.GetControlRect(false); // bug in UnityEditor: this method ignores EditorGUI.indentLevel
							layoutArea = EditorGUI.PrefixLabel(layoutArea, new GUIContent(" ")); // bug in UnityEditor: only way to find out the area is to insert a "space" label; UnityEditor incorrectly treats "" as GUIContent.none
							EditorGUI.BeginChangeCheck();
							EditorGUI.PropertyField(layoutArea, serializedObject.FindProperty(pPadding + ".isActive"), new GUIContent("Use padding?"));
							if( EditorGUI.EndChangeCheck() )
								FlexboxActionHooks.shared.OnPaddingSet.Invoke(value);
						}

						if( value.cssPadding.isActive )
						{
							using( new GUILayout.HorizontalScope() )
							{
								GUILayout.Space(16f); // simulate indent

								using( new GUILayout.VerticalScope(GUI.skin.box) )
								{
									if( itemAsContainer == null )
										EditorGUILayout.HelpBox("Padding has no effect unless you also add a FlexContainer component to this object (child FlexItems will be positioned using the padding)", MessageType.Warning);

									float targetWidthOfEachProperty = 130f;

									SerializedProperty mainProperty = serializedObject.FindProperty(pPadding);
									using( new GUILayout.HorizontalScope() )
									{
										GUILayout.Space((EditorGUIUtility.currentViewWidth - targetWidthOfEachProperty) / 2f);
										EditorGUILayout.PropertyField(mainProperty.FindPropertyRelative("top"), GUIContent.none);
										GUILayout.Space((EditorGUIUtility.currentViewWidth - targetWidthOfEachProperty) / 2f);
									}

									GUILayout.Space(10f);

									using( new GUILayout.HorizontalScope() )
									{
										EditorGUILayout.PropertyField(mainProperty.FindPropertyRelative("left"), GUIContent.none);
										GUILayout.Space((EditorGUIUtility.currentViewWidth - 2f * targetWidthOfEachProperty));
										EditorGUILayout.PropertyField(mainProperty.FindPropertyRelative("right"), GUIContent.none);
									}

									GUILayout.Space(5f);

									using( new GUILayout.HorizontalScope() )
									{
										GUILayout.Space((EditorGUIUtility.currentViewWidth - targetWidthOfEachProperty) / 2f);
										EditorGUILayout.PropertyField(mainProperty.FindPropertyRelative("bottom"), GUIContent.none);
										GUILayout.Space((EditorGUIUtility.currentViewWidth - targetWidthOfEachProperty) / 2f);
									}

								}

							}
						}

						if( value.boxSizing == BoxSizing.BORDER_BOX )
						{
							if( EditorGUI.EndChangeCheck() )
								changeAffectsSelfNotParent = true;
						}
					}
#endif

					{
						GUILayout.Label("Overrides:", EditorStyles.boldLabel);
						EditorGUI.indentLevel++;

#if LITE_VERSION
						using( new EditorGUI.DisabledScope(true) )
#endif
						{
							using( new GUILayout.HorizontalScope() )
							{
#if LITE_VERSION
							EditorGUILayout.LabelField("Default Width", "Auto");
#else
								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField(serializedObject.FindProperty(pDefaultWidth), new GUIContent("Default Width"));
								if( EditorGUI.EndChangeCheck() )
									FlexboxActionHooks.shared.OnDefaultWidthSet.Invoke(value);
#endif
							}

							using( new GUILayout.HorizontalScope() )
							{
#if LITE_VERSION
							EditorGUILayout.LabelField("Default Height", "Auto");
#else
								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField(serializedObject.FindProperty(pDefaultHeight), new GUIContent("Default Height"));
								if( EditorGUI.EndChangeCheck() )
									FlexboxActionHooks.shared.OnDefaultHeightSet.Invoke(value);
#endif
							}

							/** Order */
#if USE_UNITY_SERIALIZED_OBJECTS_FOR_INSPECTOR
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField(serializedObject.FindProperty(pOrder));
							if( EditorGUI.EndChangeCheck() )
								FlexboxActionHooks.shared.OnOrderSet.Invoke(value);
#else
		value.order = EditorGUILayout.IntField("Order", value.order);
#endif

							/** Align-self */
							using( new GUILayout.HorizontalScope() )
							{
								EditorGUILayout.PrefixLabel("Align-self");
#if LITE_VERSION
							EditorGUILayout.Toggle("", false);
#else
#if USE_UNITY_SERIALIZED_OBJECTS_FOR_INSPECTOR
								EditorGUILayout.PropertyField(serializedObject.FindProperty(pEnabled_AlignSelf), GUIContent.none, GUILayout.MaxWidth(20f));
#else
		value.isSelfAlignmentActive = EditorGUILayout.Toggle(value.isSelfAlignmentActive, GUILayout.MaxWidth(20f) );
#endif
								if( value.isSelfAlignmentActive )
								{
#if USE_UNITY_SERIALIZED_OBJECTS_FOR_INSPECTOR
									EditorGUILayout.PropertyField(serializedObject.FindProperty(pAlignSelf), GUIContent.none);
#else
			value.alignSelf = (FlexboxAlign) EditorGUILayout.EnumPopup(value.alignSelf);
#endif
								}

								if( value.isSelfAlignmentActive )
								{
									EditorGUI.indentLevel++;
									EditorGUILayout.HelpBox("'align-self : (anything except \"auto\")' is not supported yet. Using \"auto\" instead.", MessageType.Warning);
									EditorGUI.indentLevel--;
								}
#endif
							}

						}

						EditorGUI.indentLevel--;
					}

#if USE_UNITY_SERIALIZED_OBJECTS_FOR_INSPECTOR
					// Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
					
					serializedObject.ApplyModifiedProperties();
#endif
					EditorGUI.indentLevel--;

 
					if( EditorGUI.EndChangeCheck() )
					{
						if( value.settings.debugRefreshTriggers ) Debug.Log("[" + value.hierarchicalName + "]: Inspector - something changed!");
						
						/**
						 * Pivot on parent,
						 *  unless:
						 *     the change is purely internal (only one such case exists! BORDERBOX.padding),
						 *  or:
						 *     there is no parent
						 */
						if( (itemAsContainer != null)
							&&
							(changeAffectsSelfNotParent || parentFlexContainer == null) )
						{
#if UNITY_HAS_FIXED_MAJOR_SENDMESSAGE_BUG
value.RefreshLayout();
#else
							GlobalFlexboxLayoutTicker.AddRefreshOnNextTick(itemAsContainer, RefreshLayoutMode.FORCE_RECURSE_UP_THEN_SELF_DOWN);
#endif	
						}
						else if( parentFlexContainer != null )
						{
#if UNITY_HAS_FIXED_MAJOR_SENDMESSAGE_BUG
parentFlexContainer.RefreshLayout();
#else
							GlobalFlexboxLayoutTicker.AddRefreshOnNextTick(parentFlexContainer, RefreshLayoutMode.FORCE_RECURSE_UP_THEN_SELF_DOWN);
#endif				
						}
					}

#if LITE_VERSION
					{
						Color oldc = GUI.color;
						GUI.color = new Color(1f, 0.75f, 0.5f);
						using( new GUILayout.VerticalScope() )
						{
							using( new GUILayout.HorizontalScope() )
							{
								GUILayout.Space(16f * (1 + EditorGUI.indentLevel));
								using( new GUILayout.VerticalScope(GUI.skin.box) )
								{
									var wrappedCentered = new GUIStyle(EditorStyles.wordWrappedLabel);
									wrappedCentered.alignment = TextAnchor.MiddleCenter;
									GUILayout.Label("This is the LITE version. Some features are disabled. Click to purchase full version from AssetStore:", wrappedCentered);
									using( new GUILayout.HorizontalScope() )
									{
										GUILayout.FlexibleSpace();
										//HttpLink("Buy now (Asset Store)", "https://www.assetstore.unity3d.com/#!/content/139571");
										GUI.color = Color.white;
										if( GUILayout.Button("Buy now (AssetStore)") )
										{
											Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/139571");
										}

										GUILayout.FlexibleSpace();
									}
								}
							}
						}

						GUI.color = oldc;
					}
#endif
					break;
				}

				case 1:
				{
					EditorGUI.BeginChangeCheck();
					base.DrawDefaultInspector();
					if( EditorGUI.EndChangeCheck() )
					{
						GlobalFlexboxLayoutTicker.AddRefreshOnNextTick(itemAsContainer, RefreshLayoutMode.FORCE_RECURSE_UP_THEN_SELF_DOWN);
					}

					GUILayout.Label("Advanced:", EditorStyles.boldLabel);
					EditorGUI.indentLevel++;
					EditorGUILayout.LabelField("Text-child relative grow-factor", ""+value.lastReportedUnityTextWidthInPixels);
					EditorGUI.indentLevel--;
					
					// Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
					serializedObject.ApplyModifiedProperties();

					break;
				}

				case 2:
				{
					InsertHelpTabContentsShared();

					break;
				}
			}

			GUILayout.EndVertical();

			EditorGUILayout.Space(); //Unity's EndVertical with a Box style doesn't leave enough space at bottom

#if WORKAROUND_UNITY_API_MISSING_ONRECTTRANSFORMCHANGED_CALLBACK
			/**
			 * NOTE:
			 *
			 * 1. BUG: RectTransform.rect.position IS NEVER CORRECT: it is NOT updated when you update the inspector OR the gameobject!
			 *  ... it has been implemented as "this is hardcoded to always be [some undefined random pair of numbers]"
			 *
			 * ...however, transform.position does change when you update RectTransform.rect's position (which is confusing),
			 * even though its a 3D position not a 2D one. But that's enough for us to detect and generate the change-trigger.
			 */
			Vector2 currentRectTransformPosition = (value.transform as RectTransform).position;
			if( currentRectTransformPosition != _rectTransformLastPosition
			    && value.transform.parent != null
			    && value.transform.parent.GetComponent<FlexContainer>() != null )
			{
				if( !value.RelayoutWheneverSelected && newSelection )
					#pragma warning disable 0642 // hate this error: bug in Unity's C# compiler that it fails to read the code properly (normal OOP languages have compilers that get this right, C# could do it too)
					;//Debug.Log("Newly-selected FlexItem; suppressing RectTransformPositionChange");
				else
				{
					/** UnityEditor has allowed the user to move a flex item which they should not have tried to do, so we force a relayout of its parent */
					value.transform.parent.GetComponent<FlexContainer>().OnChildRectTransformPositionChange(value.transform as RectTransform);
				}
			}
			else
			{
				//Ultralog: Debug.Log("current rect position is same as last rect: "+(value.transform as RectTransform).position);
			}

			_rectTransformLastPosition = currentRectTransformPosition;
#endif
#endif
		}

		public static void InsertHelpTabContentsShared()
		{
#if LITE_VERSION
GUILayout.Label("Flexbox4Unity (LITE)", EditorStyles.boldLabel);
#else
			GUILayout.Label("Flexbox4 Unity", EditorStyles.boldLabel);
			#endif

			using( new GUILayout.HorizontalScope() )
			{
				GUILayout.FlexibleSpace();
				Color c = GUI.color;
				GUI.color = new Color( 0.75f, 1f, 1f );
				if( GUILayout.Button("Online Tutorial", GUILayout.MinWidth(100), GUILayout.MaxWidth(200f)) )
				{
					Application.OpenURL("http://flexbox4unity.com/2020/06/05/guide-using-flexbox-in-unity-2020/");
				}
				GUI.color = c;
				GUILayout.FlexibleSpace();
			}
			
			using( new GUILayout.HorizontalScope() )
			{
				GUILayout.FlexibleSpace();
				Color c = GUI.color;
				GUI.color = new Color( 0.75f, 1f, 0.75f );
				if( GUILayout.Button("Discord (flexbox4Unity)", GUILayout.MinWidth(100), GUILayout.MaxWidth(200f)) )
				{
					Application.OpenURL("https://discord.gg/umXJq4c");
				}
				GUI.color = c;
				GUILayout.FlexibleSpace();
			}

			using( new GUILayout.HorizontalScope() )
			{
				GUILayout.FlexibleSpace();
				if( GUILayout.Button("Full PDF UserGuide (v" + Flexbox4UnityProjectSettings.builtVersion.StringMajorMinorOnly() + ")", GUILayout.MinWidth(100), GUILayout.MaxWidth(200f)) )
				{
					Debug.Log("Attempting to open: " + FlexboxSettingsLoader.pathToPDFCurrentDocs);
					Application.OpenURL(FlexboxSettingsLoader.pathToPDFCurrentDocs);
				}
				GUILayout.FlexibleSpace();
			}

			using( new GUILayout.HorizontalScope() )
			{
				GUILayout.FlexibleSpace();
				if( GUILayout.Button("Support forum (Unity3D.com)", GUILayout.MaxWidth(200)) )
				{
					Application.OpenURL("https://forum.unity.com/threads/released-flexbox-fast-easy-layout-from-html-css-in-unity-2017-2018-2019.699749/");
				}
				GUILayout.FlexibleSpace();
			}
			
			using( new GUILayout.HorizontalScope() )
			{
				GUILayout.FlexibleSpace();
#if LITE_VERSION
			if( GUILayout.Button("About CSS Flexbox (LITE)", GUILayout.MinWidth(100), GUILayout.MaxWidth(200f)) )
#else
				if( GUILayout.Button("About Flexbox4Unity", GUILayout.MinWidth(100), GUILayout.MaxWidth(200f)) )
#endif
				{
					if( EditorPrefs.GetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "RecordUsageAnonymously") )
						EditorStats.sharedInstance.SendEvent("inspector", "help-tab", "about", 1);
					F4UWindowAbout.Init();
				}
				GUILayout.FlexibleSpace();
			}


			GUILayout.Label("Flexbox quick-start", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			GUILayout.Space(16f * (1 + EditorGUI.indentLevel));
			GUILayout.BeginVertical();
			StringBuilder sb = new StringBuilder();
			sb.Append("With flexbox, you group things together in FlexContainers, and use multiple nested containers to achieve any layout you want.\n\n");
			sb.Append("Each container draws its children either in a row, or in a column.\n\n");
			sb.Append("By default, the container is set to 'grow=1', which means that any remaining space will be used to 'grow' the child items, and set to 'align=stretch', which means that all items in a row will be max height, or all items in a column will be max width.\n\n");
			sb.Append("For finer control, each child has a FlexItem attached, that lets you fine-tune details such as padding or margins around that child (not available in LITE version).");
			GUILayout.Label(sb.ToString(), EditorStyles.wordWrappedLabel);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();


		}

		private static GUIStyle _styleUrlLabel = null; /* Major bug in Unity Editor (all versions): if you instantiate this, you trigger internal errors in the Unity Engine that Unity needs to fix */

		private static GUIStyle styleUrlLabel
		{
			get
			{
				if( _styleUrlLabel == null )
				{
					/* Major bug in Unity Editor (all versions): if you instantiate this in class (which is the correct C# way), you trigger internal errors in the Unity Engine that Unity needs to fix */
					_styleUrlLabel = new GUIStyle(GUI.skin.label)
					{
						normal = new GUIStyleState() {textColor = Color.blue},
						hover = new GUIStyleState() {textColor = Color.cyan},
						active = new GUIStyleState() {textColor = Color.white},
					};
				}

				return _styleUrlLabel;
			}
		}

		public static void HttpLink(string title, string URL)
		{
			var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(new GUILayoutOption[] { })); // BUG: Unity never bothered to implement EditorGUILayout.Button(), even though it's necessary
			if( GUI.Button(rect, title, styleUrlLabel) )
			{
				Application.OpenURL(URL);
			}
		}

		/** Returns true if changed, false otherwise */
		private void DrawSingleLineConstraint(string textPostfix, CSS3Length lowValue, CSS3Length highValue, Rect space)
		{
			float indentOffset = space.position.x;
			float minScreenWidthForFullLabel = 500 + indentOffset;
			float widthMiddleArea = (Screen.width<minScreenWidthForFullLabel)? 45f : 75f;
			
			Vector2 sizeLeft = new Vector2((space.width - widthMiddleArea) / 2f, space.height);
			var rectLHS = new Rect(space.position, sizeLeft);
			var rectMiddle = new Rect(space.position + new Vector2(sizeLeft.x, 0), new Vector2(widthMiddleArea, space.size.y));
			var rectRHS = new Rect(space.position + new Vector2(sizeLeft.x + widthMiddleArea, 0), sizeLeft);

			string textLabel = (Screen.width < minScreenWidthForFullLabel) ? textPostfix.Substring(0, 1) : ""+textPostfix;
			
			EditorGUILayout.BeginHorizontal( GUILayout.Height(rectMiddle.height) );
			EditorGUI.PropertyField(rectLHS, serializedObject.FindProperty("cssMin" + textPostfix), GUIContent.none);
			GUI.Label(rectMiddle, "< " + textLabel + " <");
			EditorGUI.PropertyField(rectRHS, serializedObject.FindProperty("cssMax" + textPostfix), GUIContent.none);
			EditorGUILayout.EndHorizontal();
		}

		private void CustomSeparator()
		{
			GUILayout.Space(3f);

			Rect r; // = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
			r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1f));

			// EditorGUI.indentLevel:
			float indentAdded = 16f * EditorGUI.indentLevel;
			r.x += indentAdded;
			r.width -= indentAdded;

			Color color = new Color(0.7f, 0.7f, 0.7f);
			EditorGUI.DrawRect(r, color);
		}
	}
}
#endif