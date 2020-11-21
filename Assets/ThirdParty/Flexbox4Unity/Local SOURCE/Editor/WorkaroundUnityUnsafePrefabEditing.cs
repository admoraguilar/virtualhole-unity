using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// All versions of Unity prior to 2020.1 require this class since Unity failed to fully design NestedPrefabs APIs.
/// 
/// Based on ideas in this thread:
///
///   https://forum.unity.com/threads/how-do-i-edit-prefabs-from-scripts.685711/
///
/// Note: our implementation here is bit more useful than Unity's (it tracks Exceptions), so we're not going to
/// use Unity's even in 2020.1 onwards.
/// </summary>
public class WorkaroundUnityUnsafePrefabEditing : IDisposable
{
	public readonly string assetPath;
	public readonly GameObject prefabRoot;
	public readonly Exception thrownException;

	public bool isValid { get { return thrownException == null; } }

	public WorkaroundUnityUnsafePrefabEditing(string assetPath)
	{
		this.assetPath = assetPath;
		try
		{
			prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
		}
		catch( Exception e )
		{
			thrownException = e;
		}
	}
     
	public void Dispose()
	{
		if( prefabRoot != null )
		{
			PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
			PrefabUtility.UnloadPrefabContents(prefabRoot);
		}
	}
}