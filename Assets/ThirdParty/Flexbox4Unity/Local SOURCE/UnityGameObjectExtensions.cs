using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public static class UnityGameObjectExtensions
{

	
	public static GameObject AddChildWithRectTransform( this GameObject parent, string goName )
	{
		// Create a custom game object
		GameObject go = new GameObject( goName, typeof(RectTransform) /* workaround for unfixed UnityEngine bug that RectTransform *cannot* be added to a GameObject after creation */);

		/** Add to the parent, or in root of Hierarchy */
		if( parent != null ) 
			go.transform.SetParent( parent.transform, false);
		
		return go;
	}
}