#define UNITY_EDITOR_INTERNAL_CRASH_DURING_ASSEMBLY_RELOADS_DUE_TO_BUG_IN_UNITY_INSPECTOR // needed in Unity 2018-2019 (2020: unconfirmed if still needed)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flexbox4Unity;
using IntelligentPluginVersioning;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using Version = System.Version;

namespace Flexbox4Unity
{
	/**
	 * Detects Editor startup and/or Scene startup, and runs Flexbox4Unity's startup code.
	 *
	 * This class is quite complex because Unity keeps adding new situations that re-start
	 * the Editor and they provide no single, official, way for Assets/Plugins to trigger
	 * an "asset was upgraded" or "Unity Editor was upgraded" callback.
	 *
	 * We have two situations we need to detect:
	 *  1. Flexbox4Unity was upgraded
	 *  2. An old scene is (re-)opened that MAY also need upgrading 
	 */
	[InitializeOnLoad]
	public class F4UStartup
	{
		//PRODUCTION:
		public static bool debugSettingsLoading = false;
		//DEBUGGING:public static bool debugSettingsLoading = true;

		/**
		 * Startup process is currently:
		 *
		 * 1: Start the Editor
		 *   ...
		 *   Static init (this method)
		 *   Static end (this method)
		 *    Assembly-reload-complete -- note: no "reload-started" gets triggered 
		 *   EditorLoaded
		 *   ...
		 * 
		 * 2: Change a source file and let UnityEditor auto re-compile:
		 *   ...
		 *    Assembly-about-to-reload
		 *     Static init (this method)
		 *     Static end (this method)
		 *    Assembly-reload-complete
		 *   EditorLoaded
		 *   ...
		 *
		 * 3: Open a different scene in editor:
		 *   ...
		 *   SceneOpened
		 *   ...
		 *
		 * 4: Open a prefab in editor:
		 *   ...
		 *   PrefabStageOpened
		 *   ...
		 *
		 * 5: WHILE VIEWING A PREFAB: Change a source file and let UnityEditor auto re-compile:
		 *   ...
		 *    Assembly-about-to-reload
		 *     Static init (this method)
		 *     Static end (this method)
		 *    Assembly-reload-complete
		 *   EditorLoaded
		 *   ...
		 */
		static F4UStartup()
		{
			if( debugSettingsLoading ) Debug.Log( "Flexbox4Unity: static initializer called (is prefabStage? "+isRefreshingPrefabViewNotMainEditor+")" );

			EditorApplication.update += RunOnceAfterEditorLoaded;
			
#if UNITY_EDITOR_INTERNAL_CRASH_DURING_ASSEMBLY_RELOADS_DUE_TO_BUG_IN_UNITY_INSPECTOR
			AssemblyReloadEvents.beforeAssemblyReload += AssemblyAboutToReload;
			AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadComplete;
#endif
			
			EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
			
#if UNITY_2018_1_OR_NEWER // Nested prefabs require this, but were only added in 2018
			PrefabStage.prefabStageOpened += PrefabStage_prefabStageOpened;
			PrefabStage.prefabStageClosing += PrefabStage_prefabStageClosing;
#endif

			if( debugSettingsLoading ) Debug.Log( "Flexbox4Unity: static initializer ended (is prefabStage? "+isRefreshingPrefabViewNotMainEditor+")" );
		}

		private static bool isRefreshingPrefabViewNotMainEditor { get { return null != PrefabStageUtility.GetCurrentPrefabStage(); } }


		public static void RunOnceAfterEditorLoaded()
		{
			EditorApplication.update -= RunOnceAfterEditorLoaded;


			/**
			 * In prefabstage when reload happens?
			 *  - FCOT finds components in stage only
			 *  - leaving stage: NOTHING HAPPENS
			 *
			 * In scene when reload happens?
			 *  - FCOT finds components in SCENE only
			 *  - joining prefabstage:
			 *     - FOOT finds a VERY cutdown list of the SCENE items (only the ENABLED items)
			 *     - SU.FCOT finds the stage only
			 *     - PSU.GCIC finds the stage only
			 */

			if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: editor-loaded callback (is prefabStage? " + isRefreshingPrefabViewNotMainEditor + ")");
		#if FALSE
			Debug.Log( "  O.FOOT<FlexContainer> =\n  "+string.Join( ",\n  ", Object.FindObjectsOfType<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			Debug.Log( "  R.FOOTA<FlexContainer> =\n  "+string.Join( ",\n  ", Resources.FindObjectsOfTypeAll<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			Debug.Log( "  SU.FCOT<FlexContainer> =\n  "+string.Join( ",\n  ", StageUtility.GetCurrentStageHandle().FindComponentsOfType<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
		#endif
		#if FALSE
			if( EditorPrefs.GetBool( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "RecordUsageAnonymously" ) )
			{
				EditorStats.sharedInstance.SendEvent( "editor", "retina-pixelsPerPoint", "" + EditorGUIUtility.pixelsPerPoint, 1 );
				EditorStats.sharedInstance.SendEvent( "editor", "screen-resolution", Screen.currentResolution.width + " x " + Screen.currentResolution.height, 1 );
			}
		#endif

			bool saveRequired = false;

			string stringLastGlobalLoadedVersion = EditorPrefs.GetString(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "LastUsedFlexbox4UnityVersion");
			IntelligentPluginVersioning.Version editorLastVersion = new IntelligentPluginVersioning.Version(stringLastGlobalLoadedVersion);

			var foundSettings = EditorProjectSettings.findProjectSettingsDontThrowExceptions;
			IntelligentPluginVersioning.Version lastLoadedVer = foundSettings?.lastLoadedVersion ?? IntelligentPluginVersioning.Version.zero;
			IntelligentPluginVersioning.Version liveVersion = Flexbox4UnityProjectSettings.builtVersion;

			/**
			 * Update the global "last used version in editor" and the local "last used versio in this project"
			 */
			EditorPrefs.SetString(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "LastUsedFlexbox4UnityVersion", liveVersion.ToStringFull());
			if( foundSettings != null && liveVersion != lastLoadedVer )
			{
				foundSettings.lastLoadedVersion = liveVersion;
				saveRequired = true;
			}

			if( debugSettingsLoading ) Debug.Log("LastLoaded version was: " + lastLoadedVer + ", current version in RAM is: " + liveVersion + " (and editor's global last version was: " + stringLastGlobalLoadedVersion);

			/**
			 * Show splash if:
			 *
			 *   New version is different from last version used with this project
			 *
			 *     AND:
			 *
			 *   New version is different from the last version that was used ANYWHERE in the editor
			 *
			 * (i.e. if you have two old projects and open one, splash will appear. Then when you open the other,
			 * the splash will NOT appear, but we can optionally detect that an upgrade happened)
			 */
			bool showSplashAfterEditorHasLoaded = liveVersion > lastLoadedVer && liveVersion != editorLastVersion;

			if( showSplashAfterEditorHasLoaded )
			{
				if( foundSettings != null )
					EditorStats.sharedInstance.SendEvent("editor", foundSettings.hasDisplayedFirstStartup ? "upgrade-splash" : "firstrun-splash", "about", 1);

				F4UWindowAbout.Init();

				if( foundSettings != null )
					foundSettings.hasDisplayedFirstStartup = true; // set after display, so that the window can detect if it was true or not
			}

			//#endif
			if( ! (EditorPrefs.GetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "HasClickedWriteReview")
			       || EditorPrefs.GetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "HasSeenReviewAsk"))
			    && EditorPrefs.GetInt(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "NumLocalSessions") > 15 )
			{
				EditorPrefs.SetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "HasSeenReviewAsk", true);
				if( EditorUtility.DisplayDialog("Write a Review", "If you're finding Flexbox4Unity useful, please write a review! Reviews help other people to discover this asset and decide if it will be useful to them", "Write Review", "No thanks") )
				{
					Application.OpenURL("https://assetstore.unity.com/packages/tools/gui/flexbox-4-unity-139571#reviews");
				}
			}

			/**
				* Save the main settings file if it's changed (e.g. because the lastLoadedVersion has been updated)
				*/
			if( saveRequired )
			{
				EditorUtility.SetDirty(foundSettings); // Unity's Serialization doesn't deal with ScriptableObject automatically, needs this or else it won't save the SO :(
				try
				{
					AssetDatabase.SaveAssets();
				}
				catch( Exception e )
				{
					Debug.LogError("UnityEditor internal crash: please inform Unity about this, it's a major bug inside their internal AssetDatabase system! e = " + e);
					Debug.LogWarning("...UnityEditor just crashed internally, but we MUST continue or else we'll have a corrupt Flexbox system; continuing...");
				}

				if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: after saving assets, freshly-loaded (or created) settings = " + foundSettings);
			}
		}

	#if UNITY_2018_1_OR_NEWER // Nested prefabs require this, but were only added in 2018
		private static void PrefabStage_prefabStageOpened( PrefabStage stage )
		{
			if( debugSettingsLoading )
				Debug.Log( "Prefab stage opened: " + stage+ " (is prefabStage? "+isRefreshingPrefabViewNotMainEditor+")" );
			
		#if TESTING
			/**
			 * Here, in prefab-stage-open:
			 * 
			 * O.FOOT: returns *all* objects in previously-open scene (NOT in the prefab!) but ONLY the ones that are Enabled in scene
			 * SU.FCOT: returns *only* objects in prefab
			 */
			Debug.Log( "O.FOOT<FlexContainer> =\n  "+string.Join( ",\n  ", Object.FindObjectsOfType<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			Debug.Log( "R.FOOTA<FlexContainer> =\n  "+string.Join( ",\n  ", Resources.FindObjectsOfTypeAll<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			Debug.Log( "SU.FCOT<FlexContainer> =\n  "+string.Join( ",\n  ", StageUtility.GetCurrentStageHandle().FindComponentsOfType<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			var pstage = PrefabStageUtility.GetCurrentPrefabStage();
			if( pstage != null )
				Debug.Log( "PSU.GCIC<FlexContainer> =\n  "+string.Join( ",\n  ", pstage.prefabContentsRoot.GetComponentsInChildren<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			else
				Debug.Log( "PSU.GCIC<FlexContainer> = [none]" );
		
		#endif
			
			if( EditorProjectSettings.findAnyProjectSettings != null && EditorProjectSettings.findAnyProjectSettings.autoUpgradePrefabsOnOpening ) 
				F4UUpgrader.AutoUpgrade( new F4UUpgrader.UpgradeSettings() { upgradeCurrentScene = true, showResultsPopup = false } );
		}
		
		private static void PrefabStage_prefabStageClosing( PrefabStage stage )
		{
			if( debugSettingsLoading )
				Debug.Log( "Prefab stage closing: " + stage+ " (is prefabStage? "+isRefreshingPrefabViewNotMainEditor+")" );
#if TESTING
			/**
			 * Here, in prefab-stage-CLOSING:
			 * 
			 * O.FOOT: returns *all* objects in previously-open scene (NOT in the prefab!) but ONLY the ones that are Enabled in scene
			 * SU.FCOT: returns *only* objects in prefab
			 */
			Debug.Log( "O.FOOT<FlexContainer> =\n  "+string.Join( ",\n  ", Object.FindObjectsOfType<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			Debug.Log( "R.FOOTA<FlexContainer> =\n  "+string.Join( ",\n  ", Resources.FindObjectsOfTypeAll<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			Debug.Log( "SU.FCOT<FlexContainer> =\n  "+string.Join( ",\n  ", StageUtility.GetCurrentStageHandle().FindComponentsOfType<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			var pstage = PrefabStageUtility.GetCurrentPrefabStage();
			if( pstage != null )
				Debug.Log( "PSU.GCIC<FlexContainer> =\n  "+string.Join( ",\n  ", pstage.prefabContentsRoot.GetComponentsInChildren<FlexContainer>().Select( container => container.name + " - v"+container.upgradedToVersion ) ) );
			else
				Debug.Log( "PSU.GCIC<FlexContainer> = [none]" );
#endif
		}
#endif
		
		private static void EditorSceneManager_sceneOpened( UnityEngine.SceneManagement.Scene arg0, OpenSceneMode mode )
		{
			if( debugSettingsLoading )
				Debug.Log( "sceneOpened:" + arg0.name + " -> " + mode.ToString()+ " (is prefabStage? "+isRefreshingPrefabViewNotMainEditor+")" );
			
			if( EditorProjectSettings.findAnyProjectSettings != null && EditorProjectSettings.findAnyProjectSettings.autoUpgradeScenesOnOpening ) 
				F4UUpgrader.AutoUpgrade( new F4UUpgrader.UpgradeSettings() { upgradeCurrentScene = true, showResultsPopup = false } );
		}

		public static void PreLoadSettingsFileAndRunUpgrades( bool isFirstRefreshDuringUnityEditorReloadStatics )
		{
			if( EditorApplication.isPlaying ) // Unity doesn't let us choose to ONLY listen to "scene reloads due to source code being updated", but some calls in this method will fail when UnityEngine is in runtime mode
				return;

			#if FALSE
			bool requiresDisplayWindowAtEnd = false;
			if( isFirstRefreshDuringUnityEditorReloadStatics
			    || !isRefreshingPrefabViewNotMainEditor )
			{
				if( debugSettingsLoading ) Debug.Log( "Flexbox4Unity: auto-loading project-wide settings" );

				/** Check if this was first EVER run on this machine, initialize local config if not */
				string originalVersion = EditorPrefs.GetString( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "FirstEverVersion" );
				if( originalVersion == null || originalVersion.Length < 1 )
				{
					originalVersion = Flexbox4UnityProjectSettings.builtVersion.ToString();
					EditorPrefs.SetString( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "FirstEverVersion", originalVersion );
				}

				/** Decide whether to re-run one-time install and upgrade code */
				Version previousRuntimeVersion = newSettings.lastRuntimeVersion;
				if( previousRuntimeVersion != Flexbox4UnityProjectSettings.builtVersion )
				{
					saveRequired = true;
					Debug.Log( "Save required: upgraded version from " + previousRuntimeVersion + " to " + Flexbox4UnityProjectSettings.builtVersion );
					newSettings.lastRuntimeVersion = Flexbox4UnityProjectSettings.builtVersion;
				}

				if( debugSettingsLoading ) Debug.Log( "Flexbox4Unity: checking if any auto-upgrade modules need to run..." );
				F4UUpgrader.Upgrade( isRefreshingPrefabViewNotMainEditor ? UnityEditorDomain.PREFAB_EDITOR : UnityEditorDomain.SCENE_EDITOR,
					previousRuntimeVersion,
					Flexbox4UnityProjectSettings.builtVersion );

				newSettings.wasLoadedFromFile = fileAlreadyExisted;

				/** Install a default layout-algorithm if none exists */
				if( newSettings.v2layoutAlgorithm == null && newSettings.v3layoutAlgorithm == null )
				{
					saveRequired = true;

					Debug.Log( "Flexbox4Unity: no LayoutAlgorithm configured for Flexbox - creating a default one in project and assigning now" );
					var newAlgorithmInstance = ScriptableObject.CreateInstance( Flexbox4UnityProjectSettings.latestOfficialLayoutAlgorithm ) as IFlexboxLayoutAlgorithmV3;
					newSettings.v3layoutAlgorithm = newAlgorithmInstance;

					//Debug.Log("Loaded file from folder: "+loadedFileFolder);
					string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( "Assets/" + loadedFileFolder + "/" + newAlgorithmInstance.defaultAssetName + ".asset" );

					//Debug.Log("Will create asset at: "+assetPathAndName);
					AssetDatabase.CreateAsset( newAlgorithmInstance, assetPathAndName );

					//not needed: a save is guaranteed to happen a few lines below: AssetDatabase.SaveAssets(); 
				}


				if( debugSettingsLoading ) Debug.Log( "newSettings = " + newSettings );
				Flexbox4UnityProjectSettings.sharedInstance = newSettings; // WARNING!! This reference will be DESTROYED when you call SaveAssets() after SetDirty()!!!
				if( debugSettingsLoading ) Debug.Log( "sharedInstance = " + Flexbox4UnityProjectSettings.sharedInstance );

				if( saveRequired )
				{
					EditorUtility.SetDirty( newSettings ); // Unity's Serialization doesn't deal with ScriptableObject automatically, needs this or else it won't save the SO :(
					try
					{
						AssetDatabase.SaveAssets();
					}
					catch( Exception e )
					{
						Debug.LogError( "UnityEditor internal crash: please inform Unity about this, it's a major bug inside their internal AssetDatabase system! e = " + e );
						Debug.LogWarning( "...UnityEditor just crashed internally, but we MUST continue or else we'll have a corrupt Flexbox system; continuing..." );
					}


					if( debugSettingsLoading ) Debug.Log( "Flexbox4Unity: after saving assets, freshly-loaded (or created) settings = " + newSettings );
				}

				requiresDisplayWindowAtEnd = !Flexbox4UnityProjectSettings.sharedInstance.hasDisplayedFirstStartup
				                             || previousRuntimeVersion.major < Flexbox4UnityProjectSettings.builtVersion.major
				                             || (previousRuntimeVersion.major == Flexbox4UnityProjectSettings.builtVersion.major
				                                 && previousRuntimeVersion.minor < Flexbox4UnityProjectSettings.builtVersion.minor);
				if( debugSettingsLoading ) Debug.Log( "hasdisplayedFirstStartup? " + Flexbox4UnityProjectSettings.sharedInstance.hasDisplayedFirstStartup + " ... new version (on obj) = " + Flexbox4UnityProjectSettings.sharedInstance.lastRuntimeVersion + "previous ver < built? " + previousRuntimeVersion.major + " < " + Flexbox4UnityProjectSettings.builtVersion.major + " = " + (previousRuntimeVersion.major < Flexbox4UnityProjectSettings.builtVersion.major) );
			}
			else if( debugSettingsLoading ) Debug.Log( "Flexbox4Unity: NOT auto-loading project-wide settings" );

			return !isRefreshingPrefabViewNotMainEditor && requiresDisplayWindowAtEnd;
#endif
		}

#if UNITY_EDITOR_INTERNAL_CRASH_DURING_ASSEMBLY_RELOADS_DUE_TO_BUG_IN_UNITY_INSPECTOR
#pragma warning disable 0414
		private static bool _isAssemblyReloading;
		private int _callbackOrder = 0;
#pragma warning restore 0414
		public static void AssemblyAboutToReload()
		{
			if( debugSettingsLoading ) Debug.Log("Assembly is about to reload...");
			_isAssemblyReloading = true;
		}

		public static void AssemblyReloadComplete()
		{
			_isAssemblyReloading = false;
			if( debugSettingsLoading ) Debug.Log("...assembly reload complete");
		}
#endif
	}
}