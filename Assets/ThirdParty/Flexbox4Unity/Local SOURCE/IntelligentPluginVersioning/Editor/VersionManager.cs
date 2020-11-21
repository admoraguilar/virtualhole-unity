#if UNITY_EDITOR // Workaround bugs in the UnityEditor build command (sometimes ignores the fact this file is inside an Editor folder!)
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IntelligentPluginVersioning
{
/**
 * IntelligentPluginVersioning - plugin-management system for Unity Asset Store publishers
 *
 * v2020.6.18
 */
	public class VersionManager
	{
		public static string PathToPluginFolderInsideProject(ScriptableObject objectInPlugin)
		{
			var assetsPrefixedPathToAssetFolder = PathToPluginFolderOutsideProject(objectInPlugin);

			int firstSlash = assetsPrefixedPathToAssetFolder.IndexOf('/');
			string localPathToAssetFolder = firstSlash < 0 ? assetsPrefixedPathToAssetFolder.Remove(0, "Assets".Length) : assetsPrefixedPathToAssetFolder.Remove(0, "Assets/".Length);

			return localPathToAssetFolder;
		}

		public static string PathToPluginFolderOutsideProject(ScriptableObject objectInPlugin)
		{
			var pathAndFilename = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(objectInPlugin)).ToString();
			/** path will ALWAYS have AT LEAST ONE /, because Unity prefixes it with "Assets/" */

			int lastSlash = pathAndFilename.LastIndexOf('/');
			var assetsPrefixedPathToAssetFolder = pathAndFilename.Remove(lastSlash, pathAndFilename.Length - lastSlash);

			return assetsPrefixedPathToAssetFolder;
		}
		
		/**
		 */
		public static T LoadRuntimeAccessibleInstance<T>(string preferredFilenameNoextension, out string loadedFilePath, out string loadedFileFolder) where T : ScriptableObject
		{
			var _newPrivateInstance = ScriptableObject.CreateInstance<T>();

			string localPathToAssetResourcesFolder = PathToPluginFolderInsideProject(_newPrivateInstance) + "/" + "Resources"; 
			string assetsPrefixedPathToAssetFile = "Assets/" + localPathToAssetResourcesFolder + "/" + preferredFilenameNoextension + ".asset";

			/** Attempt to load pre-existing instance from disk */
			loadedFileFolder = localPathToAssetResourcesFolder;
			loadedFilePath = loadedFileFolder + "/" + preferredFilenameNoextension + ".asset";
			T _sharedInstance = AssetDatabase.LoadAssetAtPath<T>(assetsPrefixedPathToAssetFile);

			GameObject.DestroyImmediate(_newPrivateInstance);

			return _sharedInstance;
		}
		
		/**
		 */
		public static T LoadInstance<T>(string preferredFilenameNoextension, out string loadedFilePath, out string loadedFileFolder) where T : ScriptableObject
		{
			var _newPrivateInstance = ScriptableObject.CreateInstance<T>();

			string localPathToAssetFolder = PathToPluginFolderInsideProject(_newPrivateInstance);
			string assetsPrefixedPathToAssetFile = "Assets/" + localPathToAssetFolder + "/" + preferredFilenameNoextension + ".asset";

			/** Attempt to load pre-existing instance from disk */
			loadedFileFolder = localPathToAssetFolder;
			loadedFilePath = localPathToAssetFolder + "/" + preferredFilenameNoextension + ".asset";
			T _sharedInstance = AssetDatabase.LoadAssetAtPath<T>(assetsPrefixedPathToAssetFile);

			GameObject.DestroyImmediate(_newPrivateInstance);

			return _sharedInstance;
		}

		/**
		 * 2020: Prefer use of LoadInstance<T>, and caller manually decides to create a new instance if LoadInstance fails
		 * (it turns out that the logic is cleaner that way, in practice, when you try implementing this and writing UI
		 * and logging for it across multiple projects)
		 * 
		 * @return true if file existed and was loaded, false if file did not exist and had to be created
		 */
		public static T LoadOrCreateNewInstance<T>(string preferredFilenameNoextension, out string loadedFilePath, out string loadedFileFolder, out bool fileLoadedNotCreated, T preExistingInstance = null) where T : ScriptableObject
		{
			var _newPrivateInstance = preExistingInstance == null ? ScriptableObject.CreateInstance<T>() : null;

			string localPathToAssetFolder = PathToPluginFolderInsideProject(_newPrivateInstance);
			string assetsPrefixedPathToAssetFile = "Assets/" + localPathToAssetFolder + "/" + preferredFilenameNoextension + ".asset";

			/** Attempt to load pre-existing instance from disk */
			loadedFileFolder = localPathToAssetFolder;
			loadedFilePath = localPathToAssetFolder + "/" + preferredFilenameNoextension + ".asset";
			T _sharedInstance = AssetDatabase.LoadAssetAtPath<T>(assetsPrefixedPathToAssetFile);

			if( _sharedInstance == null )
			{
				Debug.Log("[" + preferredFilenameNoextension + "] No saved settings asset found; promoting temp asset to permanent by saving it ");

				/** Promote the temp settings-asset into the real one */
				_sharedInstance = _newPrivateInstance;

				Debug.Log("[" + preferredFilenameNoextension + "] ...script files are in folder: " + AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(_sharedInstance)));

				CreateDiskFolderInsideAssetsFolder(localPathToAssetFolder); // will only create folders AS NEEDED
				AssetDatabase.CreateAsset(_sharedInstance, assetsPrefixedPathToAssetFile);
				AssetDatabase.SaveAssets();

				fileLoadedNotCreated = false;
			}
			else
			{
				//Debug.Log( "Successfully loaded existing settings asset; destroying temp asset");
				/** Since we have successfully loaded a previous settings-asset, we don't need the temp one */
				GameObject.DestroyImmediate(_newPrivateInstance);
				fileLoadedNotCreated = true;
			}

			return _sharedInstance;
		}

		public static void CreateDiskFolderInsideAssetsFolder(string localPathToFolder)
		{
			string[] pathSegments = localPathToFolder.Split(new char[] {'/'});

			//Debug.Log("pathSegments.len = "+pathSegments.Length+", first = "+(pathSegments.Length>0 ? pathSegments[0]: ""));
			if( pathSegments.Length < 2 )
			{
				AssetDatabase.CreateFolder("Assets", localPathToFolder);
			}
			else
			{

				string accumulatedSystemFolder = Application.dataPath;
				string accumulatedUnityFolder = ""; // Unity includes the substring "Assets" in the dataPath, but REQUIRES that you ALSO include it in the CreateFolder path - these things are in conflict!


				foreach( string folder in pathSegments )
				{
					if( !System.IO.Directory.Exists(accumulatedSystemFolder + "/" + accumulatedUnityFolder + (accumulatedUnityFolder.Length<1?"":"/") + folder) ) // Unity's CreateFolder method is full of bugs
					{
						//Debug.Log("Creating folder \""+folder+"\" in folder: \""+("Assets/"+accumulatedUnityFolder)+"\"");
						AssetDatabase.CreateFolder("Assets"+ (accumulatedUnityFolder.Length<1?"":"/") + accumulatedUnityFolder, folder);
					}

					if( accumulatedUnityFolder.Length < 1 )
						accumulatedUnityFolder = folder;
					else
						accumulatedUnityFolder += "/" + folder;
				}
			}
		}
	}
}
#endif