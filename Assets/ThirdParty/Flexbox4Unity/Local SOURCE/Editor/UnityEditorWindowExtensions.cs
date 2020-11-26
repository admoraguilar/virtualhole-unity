using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEditor;
using UnityEngine;

public static class UnityEditorWindowExtensions
{
	/** Workaround for many years-long bugs in Unity's APIs for editor windows and screensizes */
	public static void ResizeWindowWorkaround( this EditorWindow ew, Vector2 desiredSize, Vector2 minSize )
	{
		float sw = Screen.currentResolution.width;
		float sh = Screen.currentResolution.height;
		//Debug.Log("Screen: "+ Screen.width+"x"+ Screen.height+", ScreenResolution: "+sw+"x"+sh+", Unity retina multiple: "+EditorGUIUtility.pixelsPerPoint+" ... positionRect = "+positionRect);
			
		/**
		 * Note: after extensive testing, we can only conclude that UnityEditor's method "EditorWindow.GetWindowWithRect<>()" is BROKEN
		 */
			
		/** Experimentally determined workarounds UNDOCUMENTED by Unity */
		ew.position = new Rect(
			new Vector2( (sw-desiredSize.x)/2f, (sh-desiredSize.y)/2f) * 1f/EditorGUIUtility.pixelsPerPoint,
			// TODO: this should be * not /, I believe (June 2020)
			desiredSize * 1f/EditorGUIUtility.pixelsPerPoint /** NB: Unity SOMETIMES multiplies by .pixelsPerPoint, and OTHER TIMES multiplies by 1.33, semi-randomly, and depending on if the Window was already on screen or not */
		);

		if( sw < minSize.x )
			minSize.x = sw * 0.75f;
		if( sh < minSize.y )
			minSize.y = sh * 0.75f;
		
		// TODO: this should be * not /, I believe (June 2020)
		ew.minSize = new Vector2(minSize.x / EditorGUIUtility.pixelsPerPoint,minSize.y / EditorGUIUtility.pixelsPerPoint);
	}
}