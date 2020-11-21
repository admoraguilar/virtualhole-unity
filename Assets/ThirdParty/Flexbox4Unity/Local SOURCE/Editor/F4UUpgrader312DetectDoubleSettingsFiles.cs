using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Flexbox4Unity
{
	[InitializeOnLoad]
	public class F4UUpgrader312DetectDoubleSettingsFiles
	{
		private static void _Process()
		{
			var settingsFiles = EditorProjectSettings.findAllPossibleProjectSettings;
			if( settingsFiles.Count > 1 )
			{
				Debug.LogError( "Multiple settings files found; let's deal with it" );
				LaunchWindow();
			}
		}

		public static void LaunchWindow()
		{
			var win = EditorWindow.GetWindow<F4UWindowFixMultipleSettingsFiles>();

			float sw = Screen.currentResolution.width;
			float sh = Screen.currentResolution.height;

			Vector2 minSize = new Vector2( 200, 200 );
			Vector2 desiredSize = new Vector2( sw / 4f, sh / 4f );

			/** Experimentally determined workarounds UNDOCUMENTED by Unity */
			win.position = new Rect(
				new Vector2( (sw - desiredSize.x) / 2f, (sh - desiredSize.y) / 2f ) * 1f / EditorGUIUtility.pixelsPerPoint,
				// TODO: this should be * not /, I believe (June 2020)
				desiredSize * 1f / EditorGUIUtility.pixelsPerPoint /** NB: Unity SOMETIMES multiplies by .pixelsPerPoint, and OTHER TIMES multiplies by 1.33, semi-randomly, and depending on if the Window was already on screen or not */
			);

			if( sw < minSize.x )
				minSize.x = sw * 0.75f;
			if( sh < minSize.y )
				minSize.y = sh * 0.75f;

			// TODO: this should be * not /, I believe (June 2020)
			win.minSize = new Vector2( minSize.x / EditorGUIUtility.pixelsPerPoint, minSize.y / EditorGUIUtility.pixelsPerPoint );

			win.Show();
		}

		
		static F4UUpgrader312DetectDoubleSettingsFiles()
		{
			_Process();
		}
	}
}