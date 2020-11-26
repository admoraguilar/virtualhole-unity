using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorWindowExtensions
{
	public static Texture2D LoadTextureFromRelativePath( MonoBehaviour classFile, string relativePath)
	{
		return LoadTextureFromRelativePath(MonoScript.FromMonoBehaviour(classFile), relativePath);
	}

	public static Texture2D LoadTextureFromRelativePath(this EditorWindow window, string relativePath)
	{
		return LoadTextureFromRelativePath(MonoScript.FromScriptableObject(window), relativePath);
	}
	/** Because EditorGUIUtility.Load() still doesn't do what the docs say - it loads from one global shared path,
		 * which is incompatible with the asset store layout of files/folders - we do our own Editor-Resources-Loading here...
		 */
	private static Texture2D LoadTextureFromRelativePath( MonoScript ms, string relativePath)
	{
		string myPath = FolderPathOfEditorClasses( ms );

		if( relativePath.Contains("/") && !relativePath.StartsWith("/") )
			myPath += "/";

		myPath += relativePath;
		//This corrupts the texture's contents (Unity API bugs, apparently): return AssetDatabase.LoadAssetAtPath<Texture2D>(myPath) as Texture2D;

		Texture2D result = new Texture2D(1, 1);
		result.LoadImage( System.IO.File.ReadAllBytes( myPath ) );
		result.Apply();
		return result;
	}

	public static string FolderPathOfEditorClasses(this EditorWindow window)
	{
		return FolderPathOfEditorClasses(MonoScript.FromScriptableObject(window));
	}

	public static string FolderPathOfEditorClasses(ScriptableObject so)
	{
		return FolderPathOfEditorClasses(MonoScript.FromScriptableObject(so));
	}
	private static string FolderPathOfEditorClasses( MonoScript ms )
	{
		string assetPath = AssetDatabase.GetAssetPath(ms);
		string[] pathArray = assetPath.Split('/');
		return String.Join("/", pathArray, 0, pathArray.Length - 1);
	}
}