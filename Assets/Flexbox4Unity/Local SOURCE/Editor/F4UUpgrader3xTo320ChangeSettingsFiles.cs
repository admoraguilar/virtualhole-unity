using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Version = IntelligentPluginVersioning.Version;

namespace Flexbox4Unity
{
	public class F4UUpgrader3xTo320ChangeSettingsFiles : F4UUpgradeModule
	{
		private struct InternalUpgradeData
		{
			public int numMissingContainerSettings;
			public int numMissingItemSettings;
			public int containersUpgradeable;
			public int itemsUpgradeable;
			public int containersModified;
			public int itemsModified;
			public int prefabsChecked;

			public int numExceptionsInPrefabUpgrading;
		}

		Flexbox4UnityProjectSettings sharedSettingsFile;
		private bool testFakeFailure = false;//true;
		
		public override Version minVersionToUpgrade { get { return new Version( 0, 0, 0 ); } }
		public override Version maxVersionToUpgrade { get { return new Version( 3, 2, 0 ); } }

		[MenuItem("GameObject/Flexbox/TESTING/Wipe settings from selected object", false, 1)]
		public static void _Menu_TESTING_WipeFlexboxSettingsReference(MenuCommand menuCommand)
		{
			GameObject goParent = menuCommand.ContextSensitiveGameObject( new[] {typeof(FlexContainer), typeof(FlexItem)} );
			if( goParent == null )
				throw new Exception( "This method has to be run on a FlexContainer or FlexItem" );

			if( goParent.TryGetComponent<FlexContainer>( out FlexContainer parentContainer ) )
				parentContainer._InternalWipeSettings();
			else
				Debug.Log( "...parent "+goParent.name+" had no FlexContainer to wipe" );
			if( goParent.TryGetComponent<FlexItem>( out FlexItem parentItem ) )
				parentItem._InternalWipeSettings();
			else
				Debug.Log( "...parent "+goParent.name+" had no FlexItem to wipe" );
		}
		
		public override void PrepareToProcessUpgrades( out UpgradeResult result )
		{
			var allFound = EditorProjectSettings.findAllPossibleProjectSettings;
			
			if( allFound.Count > 1 || testFakeFailure)
			{
				result = new UpgradeResult()
				{
					success = false,
					module = this,
					problemMessage = "Multiple Flexbox settings objects found in project - Unity does not support this.",
					resolveMessage = "open an interactive settings browser and delete unwanted files, then select 'Tools > Flexbox > Auto-Upgrade' to try again",
					resolveAction = () =>
					{
						F4UUpgrader312DetectDoubleSettingsFiles.LaunchWindow();
						//throw new Exception( "Multiple Flexbox settings objects found in project - Unity does not support this, please delete one or more. Unity's names: " + string.Join( ";", allFound.Select( settings => settings.name ) ) );
					}
				};
			}
			else if( allFound.Count < 1  )
			{
				Debug.LogError( "No Flexbox settings found in project" );
				result = new UpgradeResult()
				{
					success = false,
					module = this,
					problemMessage = "No Flexbox settings found in project",
					resolveMessage = null,
					resolveAction = null
				};
			}
			else 
				result = UpgradeResult.Success;
		}

		public override bool UpgradeProject( Version projectOldVersion, Version newVersion, List<string> allPrefabPaths, F4UUpgrader.UpgradeSettings settings )
		{
			/**
			 *  ... an autodetected settings file exists, so we can now trigger each FlexContainer/Item to auto-assign
       */
			
			/**
			 * Process all objects found in current open scene...
			 */
			
			// TODO: this is deliberately skipped here, and instead processed in F4UStartup's OnSceneLoaded callback
			
			InternalUpgradeData results = new InternalUpgradeData();
			/**
			 * Process all prefabs found in project...
			 */
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
						//Debug.Log( "Prefab: " + editablePrefabScpe.prefabRoot.name + " -- found " + containers.Length + " FlexContainers + "+items.Length+" FlexItems to potentially upgrade" );

						/**
						 * NB: Flexbox4Unity's auto-configuring of settings is now so complete - and Unity is (undocumented)
						 * now so aggressive at auto-loading ALL ITEMS in a prefab and running their full OnValidate() logic
						 * even if you haven't changed anything (!) ... that this whole section may be unncessary.
						 *
						 * ...because merely opening the Prefab for editing has the side-effect of auto-fixing everything on it.
						 */

						_InternalUpgradeContainersAndItems(containers.ToList(), items.ToList(), ref results);

						results.prefabsChecked++;
					}
				}
				catch( Exception e )
				{
					results.numExceptionsInPrefabUpgrading++;
					Debug.LogWarning("Flexbox: Exception during upgrade, couldn't upgrade prefab = " + prefabPath + ", due to Exception = " + e);
				}
			}
			
			if( settings.showResultsPopup ) 
				EditorUtility.DisplayDialog( "Flexbox Re-check tool", "In "+results.prefabsChecked+" prefabs ... modified: " +results.containersModified+" / "+ results.containersUpgradeable + " upgradeable containers, " + results.itemsModified+" / "+ results.itemsUpgradeable + " upgradeable items in "+results.prefabsChecked+" prefabs.\n\nMissing: " + results.numMissingContainerSettings + " container-settings, " + results.numMissingItemSettings + " item-settings.\n\nTotal fixed/loaded: " + (results.numMissingContainerSettings + results.numMissingItemSettings)+"\n\nTotal ERRORS: "+results.numExceptionsInPrefabUpgrading, "OK" );
			else
				Debug.Log( "In "+results.prefabsChecked+" prefabs ... Upgrader '"+GetType().Name+"' - modified: " +results.containersModified+" / "+ results.containersUpgradeable + " upgradeable containers, " + results.itemsModified+" / "+ results.itemsUpgradeable + " upgradeable items in "+results.prefabsChecked+" prefabs.\n\nMissing: " + results.numMissingContainerSettings + " container-settings, " + results.numMissingItemSettings + " item-settings.\n\nTotal fixed/loaded: " + (results.numMissingContainerSettings + results.numMissingItemSettings)+"\n\nTotal ERRORS: "+results.numExceptionsInPrefabUpgrading );
			
			return false;
		}

		public override bool UpgradeSceneCurrentOnly( Version projectOldVersion, Version newVersion, List<string> allPrefabPaths, F4UUpgrader.UpgradeSettings settings )
		{
			/**
			 * Assuming the PrepareToProcessUpgrades(..) was already called ...
			 *  ... an autodetected settings file exists, so we can now trigger each FlexContainer/Item to auto-assign
       */

			InternalUpgradeData results = new InternalUpgradeData();
			/**
			 * Process all objects found in current open scene...
			 */
			var stage = StageUtility.GetCurrentStageHandle();
			var containers = stage.FindComponentsOfType<FlexContainer>();
			var items = stage.FindComponentsOfType<FlexItem>();
			//Debug.Log( "Prefab: " + editablePrefabScpe.prefabRoot.name + " -- found " + containers.Length + " FlexContainers + "+items.Length+" FlexItems to potentially upgrade" );

			/**
			 * NB: Flexbox4Unity's auto-configuring of settings is now so complete - and Unity is (undocumented)
			 * now so aggressive at auto-loading ALL ITEMS in a prefab and running their full OnValidate() logic
			 * even if you haven't changed anything (!) ... that this whole section may be unncessary.
			 *
			 * ...because merely opening the Prefab for editing has the side-effect of auto-fixing everything on it.
			 */

			_InternalUpgradeContainersAndItems( containers.ToList(), items.ToList(), ref results );

			/**
			 * Undocumented, Unity has had almost 2 years to document this but still hasn't: how do you ensure that a scene
			 * is saved when IT MIGHT BE a prefab-stage, rather than an editor-scene (Unity APIs often treat them
			 * interchangeably!)
			 */
			if( results.numMissingContainerSettings > 0 || results.numMissingItemSettings > 0 )
			{
				//Debug.Log( "Marking dirty scene = "+SceneManager.GetActiveScene().name );
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

			if( settings.showResultsPopup ) 
				EditorUtility.DisplayDialog( "Flexbox Re-check tool", "Modified: " +results.containersModified+" / "+ results.containersUpgradeable + " upgradeable containers, " + results.itemsModified+" / "+ results.itemsUpgradeable + " upgradeable items in "+results.prefabsChecked+" prefabs.\n\nMissing: " + results.numMissingContainerSettings + " container-settings, " + results.numMissingItemSettings + " item-settings.\n\nTotal fixed/loaded: " + (results.numMissingContainerSettings + results.numMissingItemSettings), "OK" );
			else
				Debug.Log( "Upgrader '"+GetType().Name+"' - modified: " +results.containersModified+" / "+ results.containersUpgradeable + " upgradeable containers, " + results.itemsModified+" / "+ results.itemsUpgradeable + " upgradeable items in "+results.prefabsChecked+" prefabs.\n\nMissing: " + results.numMissingContainerSettings + " container-settings, " + results.numMissingItemSettings + " item-settings.\n\nTotal fixed/loaded: " + (results.numMissingContainerSettings + results.numMissingItemSettings) );
			
			return false;
		}

		private void _InternalUpgradeContainersAndItems( List<FlexContainer> containers, List<FlexItem> items, ref InternalUpgradeData results )
		{
			foreach( var c in containers )
			{
				if( shouldUpgradeVersion( c.upgradedToVersion ) )
				{
					results.containersUpgradeable++;
					if( !c.hasSettings )
					{
						results.numMissingContainerSettings++;
						var forceLoad = c.settings;

						results.containersModified++;
					}
				}
			}

			foreach( var c in items )
			{
				if( shouldUpgradeVersion( c.upgradedToVersion ) )
				{
					results.itemsUpgradeable++;
					
					if( !c.hasSettings )
					{
						results.numMissingItemSettings++;
						var forceLoad = c.settings;

						results.itemsModified++;
					}
				}
			}
			
			
		}
	}
}