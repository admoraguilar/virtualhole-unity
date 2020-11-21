#define UNITY_EDITOR_INTERNAL_CRASH_DURING_ASSEMBLY_RELOADS_DUE_TO_BUG_IN_UNITY_INSPECTOR // Confirmed major bug in Unity 2018.3 to at least 2019.2
#define UNITY_HAS_FIXED_THE_NESTED_PREFABS_FINDOBJECTSOFTYPE_BUG // Not fixed in 2018. Maybe they'll fix it in 2019?
#if UNITY_EDITOR // Workaround bugs in the UnityEditor build command (sometimes ignores the fact this file is inside an Editor folder!)
using System;
using System.Collections;
using System.Collections.Generic;
using IntelligentPluginVersioning;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;
using Version = IntelligentPluginVersioning.Version;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Flexbox4Unity
{
#if FALSE
	/**
 * This is complicated, because of the way Unity wrote the Editor-startup API in small incompatible pieces over many years
 *
 * Experimentally, in Unity 2018-2019:
 *
 *    We MUST process the settings-file-load during the static initializer
 *    We MUST NOT display the splash screen during static initializer (Unity shows it, then destroys it, then corrupts the window - bug in core Unity 2018-2019)
 *    We MUST NOT do certain actions while viewing prefabs or nested prefabs
 *    We MUST NOT do any loading if the Editor is entering play mode
 */
	[InitializeOnLoad]
	public class FlexboxSettingsLoader : 
		IPreprocessBuildWithReport /** Required so we can insert the settings file into the build - Unity otherwise deletes your settings during build */
		, IPostprocessBuildWithReport /** Required so we can insert the settings file into the build - Unity otherwise deletes your settings during build */
		//Not needed since version 2.0: ScriptableObject
	{
		public static bool debugSettingsLoading = false;
		public static bool debugBuildPrePostProcess = false; 
		
		//DEBUGGING: public static bool debugSettingsLoading = true;
		private static bool showSplashAfterEditorHasLoaded;

		static FlexboxSettingsLoader()
		{
			if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: static initializer called");

			showSplashAfterEditorHasLoaded = false;
			EditorApplication.update += RunOnceAfterEditorLoaded;
#if UNITY_EDITOR_INTERNAL_CRASH_DURING_ASSEMBLY_RELOADS_DUE_TO_BUG_IN_UNITY_INSPECTOR
			AssemblyReloadEvents.beforeAssemblyReload += AssemblyAboutToReload;
			AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadComplete;
#endif

			if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: RunOncePerReload triggered");
			EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
#if UNITY_2018_1_OR_NEWER // Nested prefabs require this, but were only added in 2018
		PrefabStage.prefabStageOpened += PrefabStage_prefabStageOpened;
#endif

			try
			{
				// Check if we're ALREADY LOOKING AT a prefab, or a game-scene
#if UNITY_2018_1_OR_NEWER
				showSplashAfterEditorHasLoaded = PreLoadSettingsFileAndRunUpgrades(true, null != PrefabStageUtility.GetCurrentPrefabStage());
#else
			showSplashAfterEditorHasLoaded = PreLoadSettingsFileAndRunUpgrades(true, false);
#endif
			}
			catch( Exception e )
			{
				Debug.LogError("Flexbox4Unity: Exception caught while trying to pre-load settings; this is serious, and because of the way Unity callbacks (don't) work, we have to now cancel the pre-load - YOUR SETTINGS ARE NOW CORRUPT. e = "+e );
				EditorApplication.update -= RunOnceAfterEditorLoaded;
			}

			if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: static initializer ended, will show splash = " + showSplashAfterEditorHasLoaded);
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

		public static void RunOnceAfterEditorLoaded()
		{
			EditorApplication.update -= RunOnceAfterEditorLoaded;

			if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: editor-loaded callback: will show splash = " + showSplashAfterEditorHasLoaded);
			
			if( EditorPrefs.GetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "RecordUsageAnonymously") )
			{
				EditorStats.sharedInstance.SendEvent("editor", "retina-pixelsPerPoint", "" + EditorGUIUtility.pixelsPerPoint, 1);
				EditorStats.sharedInstance.SendEvent("editor", "screen-resolution", Screen.currentResolution.width + " x " + Screen.currentResolution.height, 1);
			}

			if( showSplashAfterEditorHasLoaded )
			{
				EditorStats.sharedInstance.SendEvent("editor",Flexbox4UnityProjectSettings.sharedInstance.hasDisplayedFirstStartup ? "upgrade-splash" : "firstrun-splash","about", 1);
				F4UWindowAbout.Init();
				Flexbox4UnityProjectSettings.sharedInstance.hasDisplayedFirstStartup = true; // set after display, so that the window can detect if it was true or not
			}
			else if( !(EditorPrefs.GetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "HasClickedWriteReview")
			         || EditorPrefs.GetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "HasSeenReviewAsk"))
			&& EditorPrefs.GetInt(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "NumLocalSessions") > 15 )
			{
				EditorPrefs.SetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "HasSeenReviewAsk", true );
				if( EditorUtility.DisplayDialog("Write a Review", "If you're finding Flexbox4Unity useful, please write a review! Reviews help other people to discover this asset and decide if it will be useful to them", "Write Review", "No thanks") )
				{
					Application.OpenURL("https://assetstore.unity.com/packages/tools/gui/flexbox-4-unity-139571#reviews");
				}
			}
		}

#if UNITY_2018_1_OR_NEWER // Nested prefabs require this, but were only added in 2018
	private static void PrefabStage_prefabStageOpened( PrefabStage stage )
	{
		if( debugSettingsLoading ) Debug.Log( "Prefab stage opened: "+stage );		
#if UNITY_HAS_FIXED_THE_NESTED_PREFABS_FINDOBJECTSOFTYPE_BUG
		PreLoadSettingsFileAndRunUpgrades( false, true );
		#else
		Debug.LogWarning( "Due to issues in UnityEngine, we CANNOT auto-upgrade your prefab contents. Right-click any Flexbox component and select 'Force Upgrade (all descendants)' to manually trigger any needed upgrades. (bug: FindObjectsOfType is broken when invoked from a PrefabStage)" );
#endif
	}
#endif
		private static void EditorSceneManager_sceneOpened(UnityEngine.SceneManagement.Scene arg0, OpenSceneMode mode)
		{
			if( debugSettingsLoading ) Debug.Log("sceneOpened:" + arg0.name + " -> " + mode.ToString());
			PreLoadSettingsFileAndRunUpgrades(false, false);
		}

		public static bool PreLoadSettingsFileAndRunUpgrades(bool isFirstRefreshDuringUnityEditorReloadStatics, bool isRefreshingPrefabViewNotMainEditor)
		{
			if( EditorApplication.isPlaying ) // Unity doesn't let us choose to ONLY listen to "scene reloads due to source code being updated", but some calls in this method will fail when UnityEngine is in runtime mode
				return false;
			
			bool requiresDisplayWindowAtEnd = false;
			if( isFirstRefreshDuringUnityEditorReloadStatics
			    || !isRefreshingPrefabViewNotMainEditor )
			{
				if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: auto-loading project-wide settings");

				string loadedFilePath;
				string loadedFileFolder;
				bool fileAlreadyExisted;
				Flexbox4UnityProjectSettings newSettings = VersionManager.LoadOrCreateNewInstance<Flexbox4UnityProjectSettings>("Flexbox4Unity", out loadedFilePath, out loadedFileFolder, out fileAlreadyExisted);
				newSettings.settingsFilePath = loadedFilePath;

				if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: found settings file? " + fileAlreadyExisted + ", freshly-loaded (or created) settings = " + newSettings);

				bool saveRequired = !fileAlreadyExisted;
				
				/** Assign the hardcoded built version to the currently running tag */
				Flexbox4UnityProjectSettings.currentlyRunningVersion = Flexbox4UnityProjectSettings.builtVersion; // have to assign from EDITOR class to RUNTIME class because unity's compilation architecture sucks
				
				/** Check if this was first EVER run on this machine, initialize local config if not */
				string originalVersion = EditorPrefs.GetString(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "FirstEverVersion");
				if( originalVersion == null || originalVersion.Length < 1 )
				{
					originalVersion = Flexbox4UnityProjectSettings.builtVersion.ToString();
					EditorPrefs.SetString(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "FirstEverVersion", originalVersion);
				}
				
				/** Decide whether to re-run one-time install and upgrade code */
				Version previousRuntimeVersion = newSettings.lastRuntimeVersion;
				if( previousRuntimeVersion != Flexbox4UnityProjectSettings.builtVersion )
				{
					saveRequired = true;
					Debug.Log("Save required: upgraded version from "+previousRuntimeVersion+" to "+Flexbox4UnityProjectSettings.builtVersion );
					newSettings.lastRuntimeVersion = Flexbox4UnityProjectSettings.builtVersion;
				}
				
				if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: checking if any auto-upgrade modules need to run...");
				F4UUpgrader.Upgrade(isRefreshingPrefabViewNotMainEditor ? UnityEditorDomain.PREFAB_EDITOR : UnityEditorDomain.SCENE_EDITOR,
					previousRuntimeVersion,
					Flexbox4UnityProjectSettings.builtVersion);

				newSettings.wasLoadedFromFile = fileAlreadyExisted;

				/** Install a default layout-algorithm if none exists */
				if( newSettings.v2layoutAlgorithm == null && newSettings.v3layoutAlgorithm == null )
				{
					saveRequired = true;
					
					Debug.Log("Flexbox4Unity: no LayoutAlgorithm configured for Flexbox - creating a default one in project and assigning now");
					var newAlgorithmInstance = ScriptableObject.CreateInstance( Flexbox4UnityProjectSettings.latestOfficialLayoutAlgorithm ) as IFlexboxLayoutAlgorithmV3;
					newSettings.v3layoutAlgorithm = newAlgorithmInstance;

					//Debug.Log("Loaded file from folder: "+loadedFileFolder);
					string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/" + loadedFileFolder + "/" + newAlgorithmInstance.defaultAssetName + ".asset");
					
					//Debug.Log("Will create asset at: "+assetPathAndName);
					AssetDatabase.CreateAsset(newAlgorithmInstance, assetPathAndName);

					//not needed: a save is guaranteed to happen a few lines below: AssetDatabase.SaveAssets(); 
				}


				if( debugSettingsLoading ) Debug.Log("newSettings = " + newSettings);
				Flexbox4UnityProjectSettings.sharedInstance = newSettings; // WARNING!! This reference will be DESTROYED when you call SaveAssets() after SetDirty()!!!
				if( debugSettingsLoading ) Debug.Log("sharedInstance = " + Flexbox4UnityProjectSettings.sharedInstance);

				if( saveRequired )
				{
					EditorUtility.SetDirty(newSettings); // Unity's Serialization doesn't deal with ScriptableObject automatically, needs this or else it won't save the SO :(
					try
					{
						AssetDatabase.SaveAssets();
					}
					catch( Exception e )
					{
						Debug.LogError("UnityEditor internal crash: please inform Unity about this, it's a major bug inside their internal AssetDatabase system! e = " + e);
						Debug.LogWarning("...UnityEditor just crashed internally, but we MUST continue or else we'll have a corrupt Flexbox system; continuing...");
					}


					if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: after saving assets, freshly-loaded (or created) settings = " + newSettings);
				}

				requiresDisplayWindowAtEnd = !Flexbox4UnityProjectSettings.sharedInstance.hasDisplayedFirstStartup
				                             || previousRuntimeVersion.major < Flexbox4UnityProjectSettings.builtVersion.major
				                             || (previousRuntimeVersion.major == Flexbox4UnityProjectSettings.builtVersion.major
				                                 && previousRuntimeVersion.minor < Flexbox4UnityProjectSettings.builtVersion.minor);
				if( debugSettingsLoading ) Debug.Log("hasdisplayedFirstStartup? " + Flexbox4UnityProjectSettings.sharedInstance.hasDisplayedFirstStartup + " ... new version (on obj) = " + Flexbox4UnityProjectSettings.sharedInstance.lastRuntimeVersion + "previous ver < built? " + previousRuntimeVersion.major + " < " + Flexbox4UnityProjectSettings.builtVersion.major + " = " + (previousRuntimeVersion.major < Flexbox4UnityProjectSettings.builtVersion.major));
			}
			else if( debugSettingsLoading ) Debug.Log("Flexbox4Unity: NOT auto-loading project-wide settings");

			return !isRefreshingPrefabViewNotMainEditor && requiresDisplayWindowAtEnd;
		}

		int IOrderedCallback.callbackOrder => _callbackOrder;
		/**
				 * The settings file name has to be hard-coded so that it can be loaded by code in a non-Editor DLL
				 * (which in DLL builds *CANNOT ACCESS* this class/instance data, hence: must hard-coded it)
				 */
		private string settingsFileName = "Flexbox4Unity.asset";
		private bool _createdResourcesFolderBeforeBuild = false;
		string preferredResourcesFolderSubPath = "Resources";
		
		private bool _autoDeleteTemporaryBuildFiles = false; /** TODO: Unity currently doesn't allow us to delete temp build-resources without you creating your own, custom, PERSONAL, build pipeline :( */
		public void OnPostprocessBuild(BuildReport report)
		{
			if( _autoDeleteTemporaryBuildFiles )
			{
				if( _createdResourcesFolderBeforeBuild )
				{
					if( debugBuildPrePostProcess ) Debug.Log("Flexbox4Unity: deleting self-created Resources folder at: " + preferredResourcesFolderSubPath);
					FileUtil.DeleteFileOrDirectory("Assets/" + preferredResourcesFolderSubPath);
					FileUtil.DeleteFileOrDirectory("Assets/" + preferredResourcesFolderSubPath + ".meta");
				}
				else // only delete the file we copied
				{
					if( debugBuildPrePostProcess ) Debug.Log("Flexbox4Unity: deleting temporary settings asset (" + settingsFileName + ") injected into build at: \"" + preferredResourcesFolderSubPath + "\"");
					AssetDatabase.DeleteAsset("Assets/" + preferredResourcesFolderSubPath + "/" + settingsFileName);
				}

				AssetDatabase.Refresh();
			}
		}
}
#else
	public class FlexboxSettingsLoader
	{
		/// <summary>
		/// In 2020, the AssetDatabase API is still so broken that the only way to find the asset for a classfile is this
		/// ridiculous dance of taking Unity's bad API and loading all the (incorrect!) matches then filtering them to
		/// find the match that matches the item you asked for!
		/// </summary>
		/// <returns></returns>
		private static string _PathToAssetForClass<T>() where T : Object
		{
			var fuzzyMatchesUnityBadSearch = AssetDatabase.FindAssets( typeof(T).Name );
			foreach( var fuzzyMatch in fuzzyMatchesUnityBadSearch )
			{
				var path = AssetDatabase.GUIDToAssetPath( fuzzyMatch );
				var matched = AssetDatabase.LoadAssetAtPath<MonoScript>( path );
				if( matched.GetClass().IsAssignableFrom( typeof(T) ) )
					return path;
			}

			return null;
		}

		private static string _FolderPathOfSettingsClass()
		{
			string assetPath = _PathToAssetForClass<FlexContainer>();
			//Debug.Log( "path to FlexContainer = "+assetPath );
			string[] pathArray = assetPath.Split('/');
			return String.Join("/", pathArray, 0, pathArray.Length - 1);
		}

		public static string pathToPDFCurrentDocs
		{
			get
			{
				var pdfName = "Flexbox4Unity-UserGuide-v" + Flexbox4UnityProjectSettings.builtVersion.StringMajorMinorOnly() + ".pdf";
				//var pdfURL = 
				var assetsFolder = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')); // required if being used with Unity's own APIs later
				var pluginSubfolder = assetsFolder + "/" + _FolderPathOfSettingsClass();
				//Debug.Log( " ... opening PDF = " + pluginSubfolder + "/" + pdfName );
				return pluginSubfolder + "/" + pdfName;
			}
		}
	}
#endif
}
#endif