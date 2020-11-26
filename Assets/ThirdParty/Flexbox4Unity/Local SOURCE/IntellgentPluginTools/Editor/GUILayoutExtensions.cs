using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GUILayoutExtensions
{
	/**
	 * Allows for double-space, triple-space, etc
	 */
	public static void FlexibleSpace( int repeat)
	{
		for( int i = 0; i < repeat; i++ )
			GUILayout.FlexibleSpace();
	}
}