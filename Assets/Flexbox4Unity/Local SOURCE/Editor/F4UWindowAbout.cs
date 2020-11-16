#if UNITY_EDITOR // Workaround bugs in the UnityEditor build command (sometimes ignores the fact this file is inside an Editor folder!)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IntelligentPluginVersioning;
using Version = IntelligentPluginVersioning.Version;
using UnityEditor;
using UnityEngine;
using IntelligentPluginTools;

namespace Flexbox4Unity
{
	public interface IF4USplash
	{
		void OnGUI(EditorWindow window, Rect windowSizePosition);
		Vector2 DesiredSize();
		Vector2 MinSize();
	}

	public class F4UWindowAbout : EditorWindow
	{
		private static IF4USplash _windowRenderer;
		private static IF4USplash windowRenderer {
			get
			{
				if(_windowRenderer==null)
					_windowRenderer = new F4USplashWindow300();

				return _windowRenderer;
			}
		}

		[MenuItem("Window/Flexbox/About")]
		[MenuItem("Tools/Flexbox/About")]
		public static void WindowFlexboxAbout()
		{
			EditorStats.sharedInstance.SendEvent(  "editor", "app-menu","about", 1);
			Init();
		}
		
		public static void Init()
		{
			F4UWindowAbout ew = EditorWindow.GetWindow<F4UWindowAbout>( false, "Flexbox4Unity");
			ew.ResizeWindowWorkaround( windowRenderer.DesiredSize(), windowRenderer.MinSize() );
		}

		void OnGUI()
		{
			windowRenderer.OnGUI( this, position );
		}

		/**
		 * Converts Unity's silly "Assets/something/something" paths (which don't even work in Unity! Because Unity requries SUB paths of Assets!)
		 * into legal, useable, absolute file:// paths
		 */
		public string AbsoluteURLFromProjectPath()
		{
			var projectFolder = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')); // required if being used with Unity's own APIs later
			return projectFolder;
		}

	}
}
#endif