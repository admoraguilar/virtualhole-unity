using System;
using System.Collections;
using System.Collections.Generic;
using IntelligentPluginTools;
using IntelligentPluginVersioning;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
	public class FlexboxProjectSettingsRenderer
	{
		private static bool debugSettingsFileSaving = false;

		public static void RenderSettingsPanel()
		{
#if UNITY_2018_1_OR_NEWER
			var settings = EditorProjectSettings.requireProjectSettings; //VersionManager.LoadOrCreateNewInstance<Flexbox4UnityProjectSettings>("Flexbox4Unity", out string ignored, out string ignored2, out bool ignored3);
#else
			string ignored, ignored2;
			bool ignored3;
			var settings = VersionManager.LoadOrCreateNewInstance<Flexbox4UnityProjectSettings>("Flexbox4Unity", out ignored, out ignored2, out ignored3);
#endif
			float panelWidth = EditorGUIUtility.currentViewWidth; // This is fundamentally broken inside Unity's "project settings" window and they have no workaround
			//panelWidth = EditorGUILayout.GetControlRect().width; // This is also fundamentally broken inside Unity's "project settings" window, with serious internal bugs even in Unity 2018!

			using( new GUILayout.HorizontalScope() )
			{
				GUILayout.Space( 15 );
				using( new GUILayout.VerticalScope() )
				{
					panelWidth = EditorGUILayout.GetControlRect( true, 0f ).width;
					EditorGUIUtility.labelWidth = panelWidth > 600f ? 300f : 200f; //Screen.width/2f;

					/**
					 * Read-only values -- anything here will NOT be saved
					 */

					EditorGUILayout.LabelField( "Plugin version", "" + Flexbox4UnityProjectSettings.builtVersion.ToStringFull() );
					EditorGUILayout.LabelField( "Built by Unity version", "" + Flexbox4UnityProjectSettings.builtForUnityVersion );

					//EditorGUILayout.LabelField( "Has displayed splash screen?", "" + settings.hasDisplayedFirstStartup );

					GUILayout.Space( 8 );
					GUILayout.Label( "Info", EditorStyles.boldLabel );
					using( new DivHorizontal() )
					{
						if( GUILayout.Button( "Release notes" ) )
						{
							F4UReleaseNotesWindow.Init();
						}

						if( GUILayout.Button( "Splash screen" ) )
						{
							F4UWindowAbout.Init();
						}
						
						if( GUILayout.Button( "PDF manual" ) )
						{
							Application.OpenURL(FlexboxSettingsLoader.pathToPDFCurrentDocs);
						}
					}

					/**
					 * User-changeable values -- everything here, whenever it changes, will trigger an auto-save of Project assets
					 */

					EditorGUI.BeginChangeCheck();

					GUILayout.Label( "Registered?", EditorStyles.boldLabel );
					using( new GUILayout.HorizontalScope( "Box" ) )
					{
						if( F4URegistration.IsRegistered() )
						{
							EditorGUILayout.HelpBox( "Already registered!", MessageType.Info );
							if( GUILayout.Button( "View/Edit Registration" ) )
							{
								F4UWindowRegistration.Init();
							}
						}
						else
						{
							EditorGUILayout.HelpBox( "If you register the plugin, you'll get faster support, and email updates about new features and upcoming releases.", MessageType.Warning );
							if( GUILayout.Button( "Register (free)" ) )
							{
								F4UWindowRegistration.Init();
							}
						}
					}

					/** Debug: testing auto-upgrader functionality
					if( GUILayout.Button("Set v=1.5.0") )
						settings.lastRuntimeVersion = new Version(1,5,1);
					*/

					GUILayout.Label( "Layout system", EditorStyles.boldLabel );
					GUILayout.BeginVertical( "Box" );

					if( settings.v2layoutAlgorithm != null )
						GUI.color = Color.red;
					EditorGUI.BeginChangeCheck();
					settings.v2layoutAlgorithm = EditorGUILayout.ObjectField( "Layout algorithm (legacy)", settings.v2layoutAlgorithm, typeof(IFlexboxLayoutAlgorithm), false ) as IFlexboxLayoutAlgorithm;
					if( EditorGUI.EndChangeCheck() )
						settings.v3layoutAlgorithm = null; // if you set a v2 layout, remove any v3 layout that was already there
					GUI.color = Color.white;

					EditorGUI.BeginChangeCheck();
					settings.v3layoutAlgorithm = EditorGUILayout.ObjectField( "Layout algorithm (current)", settings.v3layoutAlgorithm, typeof(IFlexboxLayoutAlgorithmV3), false ) as IFlexboxLayoutAlgorithmV3;
					if( EditorGUI.EndChangeCheck() )
						settings.v2layoutAlgorithm = null; // if you set a v3 layout, remove any v2 layout that was already there

					if( settings.v3layoutAlgorithm != null )
					{
						GUIStyle wrappingStyle = new GUIStyle( GUI.skin.label )
						{
							wordWrap = true
						};
						using( new GUILayout.VerticalScope(settings.v3layoutAlgorithm.GetType().Name, "Box") )
						{
							GUILayout.Space(25f ); // Unity Box layout is broken doesn't add padding-top correctly
							foreach( var feature in settings.v3layoutAlgorithm.featureDescription )
									EditorGUILayout.LabelField( "", feature, wrappingStyle);
						}
					}

					GUILayout.EndVertical();

					GUILayout.Label( "Preview / SceneView", EditorStyles.boldLabel );
					GUILayout.BeginVertical( "Box" );
					settings.drawFlexHierarchyUsing = (GizmosRenderMode) EditorGUILayout.EnumPopup( "Preview in Scene as", settings.drawFlexHierarchyUsing );
					settings.flexHierarchyGizmosInsetAmount = EditorGUILayout.FloatField( "Inset for debug view", settings.flexHierarchyGizmosInsetAmount );
					GUILayout.EndHorizontal();
					
					GUILayout.Label( "Automatic features", EditorStyles.boldLabel );
					using( new GUILayout.VerticalScope( "Box" ) )
					{
						settings.autoUpgradePrefabsOnOpening = EditorGUILayout.Toggle( "Auto-upgrade prefabs when opening them in Editor", settings.autoUpgradePrefabsOnOpening );
						settings.autoUpgradeScenesOnOpening = EditorGUILayout.Toggle( "Auto-upgrade Scenes when opening them in Editor", settings.autoUpgradeScenesOnOpening );
					}

					GUILayout.Label( "Debugging", EditorStyles.boldLabel );
					GUILayout.BeginVertical( "Box" );
					settings.debugRefreshTriggers = EditorGUILayout.Toggle( "Log actions that trigger a refresh?", settings.debugRefreshTriggers );
					settings.debugRelayoutCalls = EditorGUILayout.Toggle( "Log calls to layout/relayout?", settings.debugRelayoutCalls );
					settings.debugShowForceLayoutButton = EditorGUILayout.Toggle( "Show 'Force Re-Layout' button?", settings.debugShowForceLayoutButton );
					settings.debugAutoUpgrades = EditorGUILayout.Toggle( "Show auto-upgrader output?", settings.debugAutoUpgrades );
					GUILayout.EndVertical();

					GUILayout.Label( "Anonymous stats", EditorStyles.boldLabel );
					using( new GUILayout.VerticalScope( "Box" ) )
					{
						EditorGUILayout.HelpBox( "This DOES NOT TRACK your game - all stats-tracking is stripped from the build automatically (by Unity). These stats are only used for fixing bugs and features in Flexbox itself", MessageType.Info );
						EditorGUILayout.HelpBox( "Info is fully anonymised, and all history is randomised on every session, so that no tracking can be done from these stats. Stats are used to plan future bugfixes and features based on which features people are using the most", MessageType.Info );
						if( GUILayout.Button( "View local statistics" ) )
							F4UWindowUsageStats.Init();
						/** Note: stats settings are not stored per-project, but for ALL projects, so this will NOT trigger a re-save of settings */
						F4UAnonymousEditorStats.SetRecordingUsageAnonymously( EditorGUILayout.Toggle( "Send anonymous usage-stats", F4UAnonymousEditorStats.isRecordingAnonymousUsage ) );
					}

					if( EditorGUI.EndChangeCheck() )
					{
						if( debugSettingsFileSaving ) Debug.Log( "Flexbox4Unity: Saving project-settings..." );
						EditorUtility.SetDirty( settings ); // otherwise Unity never saves the changes to the ScriptableObject - UnityEngine's API here is confusing and poorly designed  
						AssetDatabase.SaveAssets();
					}

					EditorGUIUtility.labelWidth = 0f;
				}

				GUILayout.Space( 5 );
			}
		}
	}
}