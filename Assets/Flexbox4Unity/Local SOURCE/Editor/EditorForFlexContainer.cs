//#define DEBUG_ENABLE_FORCE_RELAYOUT_FOR_TESTING_CHANGES_TO_LAYOUT_ROUTINE
#define WORKAROUND_UNITY_API_MISSING_ONRECTTRANSFORMCHANGED_CALLBACK  
//April 2019: Unity still hasn't fixed this: #define UNITY_HAS_FIXED_MAJOR_SENDMESSAGE_BUG
#if UNITY_EDITOR // Workaround bugs in the UnityEditor build command (sometimes ignores the fact this file is inside an Editor folder!)
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

namespace Flexbox4Unity
{
	[CustomEditor(typeof(FlexContainer))]
	[CanEditMultipleObjects]
	public class EditorForFlexContainer : Editor
	{
		public bool temporaryForceAllowEditFlexItemSettings_ForPrefabs = false;
		public bool newSelection = false;
		
		private static int _currentTab; // static makes Unity remember the selected tab when you select new objects

		/** Undocumented by Unity: every time you select something in Inspector, the Editor class is re-instantiated,
		 * so this is how you "detect" object was selected (there is no other API call that provides this callback!)
		 */
		public EditorForFlexContainer()
		{
#if WORKAROUND_UNITY_API_MISSING_ONRECTTRANSFORMCHANGED_CALLBACK
			_rectTransformLastPositions.Clear();
#endif
			newSelection = true;
		}
#if WORKAROUND_UNITY_API_MISSING_ONRECTTRANSFORMCHANGED_CALLBACK
		/** Because Unity refuses (we've been requesting this for almost TEN YEARS!) to add a callback for transform-changed,
		 * and because their implementation of RectTransformDimensionsChange() has never been implemented to fire
		 * for position-changes (even though it clearly should do, given what it was designed for), we have to
		 * "fake" a detection of RectTransform.position changing
		 *
		 * TODO: I found an undocumented Unity workaround for this a year ago, but now it's stopped working and I cannot
		 * remember how I did it (I had to decompile Unity's source code to think of it in the first place)
		 */
		private Dictionary<FlexContainer,Vector2> _rectTransformLastPositions = new Dictionary<FlexContainer, Vector2>();
#endif
		public override void OnInspectorGUI()
		{
			/** -------- Render the flex-container GUI: ---------- */

			serializedObject.Update();

			/** Add the tabbed toolbar OUTSIDE the change-check, so that tab changing doesnt trigger a re-save */
			GUILayout.Space(10f);
			_currentTab = GUILayout.Toolbar(_currentTab, new string[] {"Simple", "Advanced", "Help"});

			GUILayout.BeginVertical(new GUIStyle(GUI.skin.box) {margin = new RectOffset(10, 5, 0, 0)});
			switch( _currentTab )
			{
				case 0:
				{
					EditorGUI.BeginChangeCheck();

					GUILayout.Label("Layout children:", EditorStyles.boldLabel);
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(serializedObject.FindProperty("direction"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("justifyContent"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("alignItems"));
					EditorGUI.indentLevel--;


					// Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
					serializedObject.ApplyModifiedProperties();

					if( EditorGUI.EndChangeCheck() )
					{
						foreach( FlexContainer value in targets )
						{
#if UNITY_HAS_FIXED_MAJOR_SENDMESSAGE_BUG
This will no longer work since we've changed so much since. Even if Unity fixes their bug, this code would need rewriting:
 ALSO: some users prefer the use of the GlobalFlexboxLayoutTicker even if it's not needed
value.RefreshLayout();
#else
							GlobalFlexboxLayoutTicker.AddRefreshOnNextTick(value, RefreshLayoutMode.FORCE_RECURSE_UP_THEN_SELF_DOWN);
#endif
						}
					}

					bool canAnyMultiSelectedItemExpandToParent = false;
					foreach( FlexContainer value in targets )
					{
						bool couldExpand = (value.parentObjectType == HierarchyObjectType.RECTTRANSFORM_ONLY && value.CouldExpandToFillParent());

						if( couldExpand
						    && (value.transform.parent != null
						        && value.transform.parent.gameObject.GetComponent<ScrollRect>() == null) // skip children of scrollrects - the scrollbars mean you NEVER want to expand to fit
						    ) 
							canAnyMultiSelectedItemExpandToParent = true;
					}

					if( canAnyMultiSelectedItemExpandToParent )
					{
								GUILayout.Label("Unity GUI:", EditorStyles.boldLabel);
								EditorGUI.indentLevel++;
								this.IndentGUILayout_Begin();

								var oldColour = GUI.color;
								GUI.color = Color.green;
								if( GUILayout.Button("Expand to fit parent RectTransform") )
								{
									foreach( FlexContainer value in targets )
									{
										if(value.parentObjectType==HierarchyObjectType.RECTTRANSFORM_ONLY && value.CouldExpandToFillParent())
											value.ExpandToFillParent();
									}
								}
								GUI.color = oldColour;

								this.IndentGUILayout_End();
								EditorGUI.indentLevel--;
					}

					FlexContainer singleTarget = target as FlexContainer;
					if( singleTarget.settings.currentLayoutAlgorithm == null )
					{
						GUILayout.Label("Global layout-algorithm", EditorStyles.boldLabel);
						EditorGUI.indentLevel++;
						EditorGUILayout.HelpBox("No layout algorithm selected", MessageType.Error);
						var c = GUI.color;
						GUI.color = Color.yellow;
						if( GUILayout.Button("Auto-select algorithm") )
						{
							Debug.Log("Flexbox4Unity: no LayoutAlgorithm configured for Flexbox - creating a default one in project and assigning now");
							var newAlgorithmInstance = IFlexboxLayoutAlgorithmV3.CreateInstance(Flexbox4UnityProjectSettings.latestOfficialLayoutAlgorithm) as IFlexboxLayoutAlgorithmV3;
							if( newAlgorithmInstance != null )
							{
									AssetDatabase.CreateAsset(newAlgorithmInstance, EditorWindowExtensions.FolderPathOfEditorClasses(singleTarget.settings) + "/" + newAlgorithmInstance.defaultAssetName + ".asset");
									singleTarget.settings.v3layoutAlgorithm = newAlgorithmInstance;
									Debug.Log("Saving project-settings...");
									EditorUtility.SetDirty(singleTarget.settings); // otherwise Unity never saves the changes to the ScriptableObject - UnityEngine's API here is very poorly designed  
									AssetDatabase.SaveAssets();
							}
							else
								Debug.LogError("Error trying to auto-assign the latest v3 Algorithm");
						}

						GUI.color = c;
						EditorGUI.indentLevel--;
					}
					else if( singleTarget.settings.v3layoutAlgorithm == null 
						|| Flexbox4UnityProjectSettings.latestOfficialLayoutAlgorithm != singleTarget.settings.v3layoutAlgorithm.GetType() )
					{
						GUILayout.Label("Global layout-algorithm", EditorStyles.boldLabel);
						EditorGUI.indentLevel++;
						EditorGUILayout.HelpBox("You're not running the current recommended layout algorithm.", MessageType.Error);
						var c = GUI.color;
						GUI.color = Color.yellow;
						if( GUILayout.Button("Auto-upgrade algorithm") )
						{
							Debug.Log("Flexbox4Unity: upgrading LayoutAlgorithm - creating a default one in project and assigning now");
							var newAlgorithmInstance = IFlexboxLayoutAlgorithmV3.CreateInstance(Flexbox4UnityProjectSettings.latestOfficialLayoutAlgorithm) as IFlexboxLayoutAlgorithmV3;
							if( newAlgorithmInstance != null )
							{
								AssetDatabase.CreateAsset(newAlgorithmInstance, EditorWindowExtensions.FolderPathOfEditorClasses(singleTarget.settings) + "/" + newAlgorithmInstance.defaultAssetName + ".asset");
								singleTarget.settings.v3layoutAlgorithm = newAlgorithmInstance;

								/** Remove any legacy layout-algorithm if it's still there */
								if( singleTarget.settings.v2layoutAlgorithm != null )
									singleTarget.settings.v2layoutAlgorithm = null;
								
								Debug.Log("Saving project-settings...");
								EditorUtility.SetDirty(singleTarget.settings); // otherwise Unity never saves the changes to the ScriptableObject - UnityEngine's API here is very poorly designed  
								AssetDatabase.SaveAssets();
							}
							else
								Debug.LogError("Error trying to auto-assign the latest v3 Algorithm");
						}

						GUI.color = c;
						EditorGUI.indentLevel--;
					}

					break;
			}

				case 1:
				{
					base.DrawDefaultInspector();

					GUILayout.Label("Advanced:", EditorStyles.boldLabel);
					EditorGUI.indentLevel++;
					temporaryForceAllowEditFlexItemSettings_ForPrefabs = EditorGUILayout.Toggle("Force-enable editing (if prefab)?", temporaryForceAllowEditFlexItemSettings_ForPrefabs);
					EditorGUI.indentLevel--;
					
					// Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
					serializedObject.ApplyModifiedProperties();

#if DEBUG_ENABLE_FORCE_RELAYOUT_FOR_TESTING_CHANGES_TO_LAYOUT_ROUTINE
					EditorGUI.indentLevel++;

					if( Flexbox4UnityProjectSettings.sharedInstance.debugShowForceLayoutButton )
					{
						if( GUILayout.Button("Force Unity to Re-layout") )
						{
							foreach( FlexContainer value in targets )
							{
								if( value.showDebugMessages ) Debug.Log("User requested re-layout of: [" + value.name + "]");
								// As of 2019, All versions of Unity from 2017.1 onwards are broken and IGNORE this call in Editor: LayoutRebuilder.MarkLayoutForRebuild( value.transform as RectTransform );

								//To only layout THIS child and its descendents (should trigger relayout of parent automatically, so Unity will lay this + children out twice :( )
								LayoutRebuilder.ForceRebuildLayoutImmediate(value.transform as RectTransform);

								//To force jumping to root flexbox first, and doing full layout of all children (including this one): value.RefreshLayout();
							}
						}

						if( GUILayout.Button("ReLayout: SELF downwards") )
						{
							foreach( FlexContainer value in targets )
							{
								if( value.showDebugMessages ) Debug.Log("User requested re-layout SELF downwards: [" + value.name + "]");
								// As of 2019, All versions of Unity from 2017.1 onwards are broken and IGNORE this call in Editor: LayoutRebuilder.MarkLayoutForRebuild( value.transform as RectTransform );

								Flexbox4UnityProjectSettings.sharedInstance.layoutAlgorithm.ReLayout(value, RefreshLayoutMode.SELF_AND_DESCENDENTS_ONLY);
							}
						}

						if( GUILayout.Button("ReLayout: PARENT upwards") )
						{
							foreach( FlexContainer value in targets )
							{
								if( value.showDebugMessages ) Debug.Log("User requested re-layout PARENT upwards: [" + value.name + "]");
								// As of 2019, All versions of Unity from 2017.1 onwards are broken and IGNORE this call in Editor: LayoutRebuilder.MarkLayoutForRebuild( value.transform as RectTransform );

								Flexbox4UnityProjectSettings.sharedInstance.layoutAlgorithm.ReLayout(value, RefreshLayoutMode.RECURSE_UP_WHOLE_TREE);
							}
						}
					}
#endif

					EditorGUI.indentLevel--;

					break;
				}

				case 2:
				{
					EditorForFlexItem.InsertHelpTabContentsShared();
					break;
				}
			}

			GUILayout.EndVertical();

			EditorGUILayout.Space(); //Unity's EndVertical with a Box style doesn't leave enough space at bottom 

#if WORKAROUND_UNITY_API_MISSING_ONRECTTRANSFORMCHANGED_CALLBACK
			foreach( var o in targets )
			{
				if( o is FlexContainer value )
				{
					/**
					 *
					 * NOTE
					 * 
					 * 1. BUG: RectTransform.rect.position IS NEVER CORRECT: it is NOT updated when you update the inspector OR the gameobject!
					 *  ... it has been implemented as "this is hardcoded to always be [some undefined random pair of numbers]"
					 *
					 * ...however, transform.position does update when you update RectTransform.rect's position (which is very confusing)
					 */
					if( value.transform is RectTransform rt ) // Note: a badly-created GO could have a Transform instead of RectTransform because of Unity's 6-year-old bugs (major bug added in 2015, still unfixed in 2020)
					{
						Vector2 currentRectTransformPosition = rt.position;
						Vector2 lastPosition = _rectTransformLastPositions.ContainsKey( value ) ? _rectTransformLastPositions[value] : Vector2.negativeInfinity;

						if( currentRectTransformPosition != lastPosition
						    && (value.RelayoutWheneverSelected
						        || lastPosition.x > Vector2.negativeInfinity.x) // NB: bug in UnityEditor - Vector2.equals is broken for Vector2.negativeInfinity
						)
						{
							//ULTRA DEBUG: Debug.Log( "_rectTransformLastPosition = "+_rectTransformLastPosition);
							value.OnRectTransformPositionChange();
						}
						else
						{
							//ULTRA DEBUG: Debug.Log("current rect position is same as last rect: "+(value.transform as RectTransform).position);
						}

						_rectTransformLastPositions[value] = currentRectTransformPosition;
					}
				}
			}
#endif

			newSelection = false;
		}
	}
}
#endif