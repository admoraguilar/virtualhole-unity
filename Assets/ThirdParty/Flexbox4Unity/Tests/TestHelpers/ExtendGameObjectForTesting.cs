using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEngine;

public static class ExtendGameObjectForTesting
{
	public static RectTransform RT( this Component c )
	{
		return RT( c.transform );
	}
	public static RectTransform RT( this MonoBehaviour mb )
	{
		return RT( mb.transform );
	}
	public static RectTransform RT( this Transform t )
	{
		return t as RectTransform;
	}
	
	public static T AddGameObjectWith<T>( this GameObject parentGo, bool rectTransform = false, string optionalName = "[auto generated]" ) where T : Component
	{
		GameObject newGo = rectTransform ? new GameObject( optionalName, typeof(RectTransform) ) : new GameObject( optionalName );
		newGo.transform.SetParent( parentGo.transform, true );
		newGo.transform.position = Vector3.zero;
		T result = newGo.AddComponent<T>();
		return result;
	}
}