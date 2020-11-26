using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flexbox4Unity;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Object = UnityEngine.Object;
using Version = IntelligentPluginVersioning.Version;

namespace Flexbox4Unity
{
#pragma warning disable 612, 618 // The whole point of the Upgrade system is that it copes with Obsolete code and gets rid of it for you :)
#pragma warning disable 642 // C# compiler is just badly written, this warning is almost always wrong
	public class F4UUpgrader
	{

		public class UpgradeSettings
		{
			public bool showResultsPopup;
			public bool upgradeCurrentScene;
			public bool upgradeProjectPrefabs;
			public bool showResultsEvenIfNothingChanged;
		}
		
		private static F4UUpgradeModule[] upgraders
		{
			get
			{
				return new F4UUpgradeModule[]
				{
					//new F4UUpgrader2xTo3xChangeRegisterKeys(),
					//new F4UUpgrader2xTo3xDeleteObsoleteClasses(),
					new F4UUpgrader3xTo320ChangeSettingsFiles(),
				};
			}
		}

		private static void PreCheckUpgraders( out UpgradeResult firstFailure )
		{
			foreach( var m in upgraders )
			{
				m.PrepareToProcessUpgrades( out UpgradeResult canUpgrade );

				if( !canUpgrade.success )
				{
					Debug.Log( "Module failed: "+m+", with message = "+canUpgrade.problemMessage );
					firstFailure = canUpgrade;
					return;
				}
			}
			
			//Debug.Log( "no failures, returning success: "+UpgradeResult.Success.success );
			firstFailure = UpgradeResult.Success;
		}
		
		/// <summary>
		/// WARNING: many bugs in Unity 2018/2019/2020's NestedPrefabs API from Unity mean this is too dangerous to use in most cases
		/// (even though it's correct, Unity's new code breaks itself and breaks the UnityEditor because they changed internal
		/// representations but didn't fix their core APIs).
		///
		/// Instead, use: GetAllPrefabPaths() and WorkaroundUnityUnsafePrefabEditing
		/// </summary>
		/// <see cref="WorkaroundUnityUnsafePrefabEditing"/>
		/// <see cref="GetAllPrefabPaths"/>
		/// <returns></returns>
		public static List<GameObject> GetAllPrefabs()
		{
			string[] guids = AssetDatabase.FindAssets("t:Prefab a:assets" /** Undocumented feature, because Unity staff don't document the core APIs */ );
			
			return guids.Select( s => AssetDatabase.LoadAssetAtPath<GameObject>( AssetDatabase.GUIDToAssetPath( s ) ) ).Where( o => o != null ).ToList();
		}
		
		/// <summary>
		/// Use this with "using( new WorkaroundUnityUnsafePrefabEditing( [path from this method] ) ) { .. write your prefab-editing-code here }"
		/// </summary>
		/// <see cref="WorkaroundUnityUnsafePrefabEditing"/>
		/// <returns></returns>
		public static List<string> GetAllPrefabPaths()
		{
			string[] guids = AssetDatabase.FindAssets("t:Prefab a:assets" /** Undocumented feature, because Unity staff don't document the core APIs */ );
			
			return guids.Select( s => AssetDatabase.GUIDToAssetPath( s ) ).Where( s => s != null ).ToList();
		}

		public static string[] GetAllScenePaths()
		{
			/**
			 * This is the official (!) June 2020 way from Unity to get a list of Scene objects.
			 */
			var guids = AssetDatabase.FindAssets( "t:Scene a:assets" );
			var scenePaths = Array.ConvertAll<string, string>( guids, AssetDatabase.GUIDToAssetPath );
			scenePaths = Array.FindAll( scenePaths, File.Exists ); // Unity erroneously considers folders named something.unity as scenes, remove them
			
			return scenePaths;
		}

		[MenuItem( "Tools/Flexbox/Upgrader/Upgrade: current scene" )]
		public static void Menu_UpgradeCurrentScene()
		{
			AutoUpgrade( new UpgradeSettings() {upgradeCurrentScene = true, showResultsEvenIfNothingChanged = true} );
			//AutoUpgradeCurrentSceneOnly();
		}
		[MenuItem( "Tools/Flexbox/Upgrader/Upgrade: all prefabs" )]
		public static void Menu_UpgradeProjectPrefabs()
		{
			Upgrade( lastUpgradedToVersion, Flexbox4UnityProjectSettings.builtVersion, new UpgradeSettings() {upgradeProjectPrefabs = true, showResultsEvenIfNothingChanged = true} );
		}
		[MenuItem( "Tools/Flexbox/Upgrader/Upgrade: Scene + Prefabs" )]
		public static void Menu_UpgradeProjectPrefabsAndCurrentScene()
		{
			AutoUpgrade( new UpgradeSettings() {upgradeCurrentScene = true, upgradeProjectPrefabs = true, showResultsEvenIfNothingChanged = true});
			//AutoUpgradeSceneAndPrefabs();
		}

		public static Version lastUpgradedToVersion
		{
			get
			{
				var vString = EditorPrefs.GetString( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "lastUpgradeVersionRun" );
				Version v = new Version( vString );
				return v;
			}
			private set 
			{
				EditorPrefs.SetString( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "lastUpgradeVersionRun", value.ToStringFull() );
			}
		}
		
		/// <summary>
		/// Use this for all upgrading - this automatically processes upgrades safely - or one of the variants which pre-fill the Settings var
		/// </summary>
		/// <param name="settings"></param>
		public static void AutoUpgrade( UpgradeSettings settings )
		{
			Upgrade( lastUpgradedToVersion, Flexbox4UnityProjectSettings.builtVersion, settings );
		}
		
		public static void AutoUpgradeSceneAndPrefabs()
		{
			AutoUpgrade( new UpgradeSettings() {upgradeCurrentScene = true, upgradeProjectPrefabs = true });
		}
		public static void AutoUpgradeCurrentSceneOnly(UpgradeSettings settings = null)
		{
			AutoUpgrade( new UpgradeSettings() {upgradeCurrentScene = true} );
		}

		private static void Upgrade( Version projectOldVersion, Version newVersion, UpgradeSettings settings )
		{
			
			int numProjectsUpgraded = 0;
			int numScenesUpgraded = 0;
			int numFlexComponentsUpgraded = 0;

			PreCheckUpgraders( out UpgradeResult preCheck );

			if( !preCheck.success )
			{
				if( preCheck.resolveMessage == null )
					EditorUtility.DisplayDialog( "Auto-upgrade failed", preCheck.problemMessage + "\n\n" + "Cannot auto-upgrade. Please fix and try again", "OK" );
				else
				{
					int dialogResult = EditorUtility.DisplayDialogComplex( "Auto-upgrade failed", preCheck.problemMessage + "\n\n" + "Press Resolve to: " + preCheck.resolveMessage, "Resolve", "Cancel", null );
					switch( dialogResult )
					{
						case 0: // OK
							preCheck.resolveAction();
							break;

						case 1: // Cancel
							break;
					}
				}
			}
			else
			{
				//Debug.Log( "Upgrading"+(settings.upgradeCurrentScene?" scene":" ")+(settings.upgradeProjectPrefabs?" prefabs":"") );
				var allPrefabPaths = GetAllPrefabPaths();
				var allScenePaths = GetAllScenePaths();
					
				//Debug.Log( "Running "+upgraders.Length+" with "+allPrefabPaths.Count+" prefabs in project..." );
				foreach( var m in upgraders )
				{
					if( projectOldVersion >= m.minVersionToUpgrade
					    && projectOldVersion <= m.maxVersionToUpgrade )
					{
						if( settings.upgradeCurrentScene )
						{
							if( m.UpgradeSceneCurrentOnly( projectOldVersion, newVersion, allPrefabPaths, settings ) )
								; //numProjectsUpgraded++;
						}

						if( settings.upgradeProjectPrefabs )
						{
							if( m.UpgradeProject( projectOldVersion, newVersion, allPrefabPaths, settings ) )
								numProjectsUpgraded++;
						}
					}
				}
				
				/**
				 * Update the stored "upgradedToVersion" data (has to be done AFTER all modules have processed their upgrades)
				 */
				if( settings.upgradeCurrentScene )
				{
					var stage = StageUtility.GetCurrentStageHandle();
					var containers = stage.FindComponentsOfType<FlexContainer>();
					var items = stage.FindComponentsOfType<FlexItem>();
					
					foreach( var c in containers )
						c.upgradedToVersion = newVersion;
					foreach( var i in items )
						i.upgradedToVersion = newVersion;

					if( containers.Length > 0 || items.Length > 0 )
					{
						EditorSceneManager.MarkSceneDirty( SceneManager.GetActiveScene() );

						/** NB: the previous call will SILENTLY FAIL if the scene HAPPENED TO BE a Prefab scene.
						 *
						 * This is one of the MANY core APIs that Unity staff deliberately broke in 2018 because they didn't want to
						 * back and fix the existing APIs to preserve their behaviour ... and which (as of Winter 2020) they STILL
						 * HAVE NOT updated the documentation to state this.
						 */
						if( PrefabStageUtility.GetCurrentPrefabStage() != null )
						{
							//Debug.Log( "Marking dirty prefab-stage = "+PrefabStageUtility.GetCurrentPrefabStage().scene.name );
							EditorSceneManager.MarkSceneDirty( PrefabStageUtility.GetCurrentPrefabStage().scene );
						}
					}
				}
				if( settings.upgradeProjectPrefabs )
				{
					foreach( var prefabPath in allPrefabPaths )
					{
						try
						{
							using( var editablePrefabScpe = new WorkaroundUnityUnsafePrefabEditing(prefabPath) )
							{
								if( ! editablePrefabScpe.isValid )
								{
									Debug.LogWarning("Flexbox: Problem during upgrade, couldn't upgrade prefab = " + prefabPath + ". Exception? = " + editablePrefabScpe.thrownException);
									continue;
								}

								var containers = editablePrefabScpe.prefabRoot.GetComponentsInChildren<FlexContainer>();
								var items = editablePrefabScpe.prefabRoot.GetComponentsInChildren<FlexItem>();

								foreach( var c in containers )
									c.upgradedToVersion = newVersion;
								foreach( var i in items )
									i.upgradedToVersion = newVersion;
							}
						}
						catch( Exception e )
						{
							Debug.LogWarning("Flexbox: Exception during upgrade, couldn't upgrade prefab = " + prefabPath + ", due to Exception = " + e);
						}
					}
				}
				
				
				/**
				 * If no errors / exceptions, update the saved upgraded-to version
				 */
				lastUpgradedToVersion = newVersion;

				if( settings.showResultsEvenIfNothingChanged
				    || numProjectsUpgraded > 0
				    || numScenesUpgraded > 0
				    || numFlexComponentsUpgraded > 0 )
				{
					string msg = "Flexbox4Unity Auto-upgrader: for version " + Flexbox4UnityProjectSettings.builtVersion + ", Processed: " + numProjectsUpgraded + " project upgrades, " + numScenesUpgraded + " scene upgrades, " + numFlexComponentsUpgraded + " flex item upgrades (ran: " + upgraders.Length + " upgrader-modules)";
					msg += ". Module list = " + string.Join(",", upgraders.Select(module => module.GetType().Name));
					
					if( settings.showResultsPopup )
						EditorUtility.DisplayDialog("Flexbox-Upgrader tool", msg, "OK");
					else
						Debug.Log( msg );
				}
			}
		}

		[MenuItem("Tools/Flexbox/Upgrader/ADVANCED: Reset upgrade status")]
		public static void TESTING_wipeUpgradedVersionToZero()
		{
			lastUpgradedToVersion = Version.zero;
		}

		[MenuItem("Tools/Flexbox/Upgrader/Check Upgrades Possible")]
		public static void TestAvailableUpgrades()
		{
			PreCheckUpgraders( out UpgradeResult firstFailure );

			if( firstFailure.success )
				EditorUtility.DisplayDialog( "Flexbox Auto-Upgrade", "Upgrade-check: success! Auto-upgrade has no blockers", "OK" );
			else
			{
				if( firstFailure.resolveMessage == null )
					EditorUtility.DisplayDialog( "Auto-upgrade failed", firstFailure.problemMessage + "\n\n" + "Cannot auto-upgrade. Please fix and try again", "OK" );
				else
				{
					int dialogResult = EditorUtility.DisplayDialogComplex( "Auto-upgrade failed", firstFailure.problemMessage + "\n\n" + "Press Resolve to: " + firstFailure.resolveMessage, "Resolve", "Cancel", null );
					switch( dialogResult )
					{
						case 0: // OK
							firstFailure.resolveAction();
							break;

						case 1: // Cancel
							break;
					}
				}

			}
		}
		
#if FALSE
		/**
		 * For manually testing a specific upgrade module while developing pre-launch beta
		 * releases
		 */
		public static void UpgradeTest(UnityEditorDomain editorMode, F4UUpgradeModule module)
		{
			HashSet<Scene> modifiedScenes = new HashSet<Scene>();

			module.PrepareToProcessUpgrades();
			throw new NotImplementedException("Not used since v2.0.x, needs rewriting for v3/v4");
			#pragma warning disable 0162
			module.FinishProcessingUpgrades();

			foreach( var s in modifiedScenes )
			{
				EditorSceneManager.MarkSceneDirty(s);
			}
		}
#endif
	}
#pragma warning restore 612, 618
}