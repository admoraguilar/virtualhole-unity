using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
	public class F4UOldSettingsWindow : EditorWindow
	{
#if UNITY_2018_3_OR_NEWER
/**
 * From Unity 2018.3 onwards, we use the new SettingsProvider APIs,
 * instead of manually-created Settings Windows.
 */
#else
		[MenuItem("Window/Flexbox/Settings")]
		public static void Init()
		{
			float sw = Screen.currentResolution.width;
			float sh = Screen.currentResolution.height;
			float w = Math.Max(1280, sw * 0.25f);
			float h = Math.Max(768, sh * 0.25f);
			//Debug.Log("w, h = "+w+", "+h+", sw/h = "+sw+", "+sh);
			F4UOldSettingsWindow ew = EditorWindow.GetWindowWithRect<F4UOldSettingsWindow>(new Rect((sw - w) / 2f, (sh - h) / 2f, w, h), false, "Flexbox Settings");
			ew.maxSize = new Vector2(sw, sh);
			ew.minSize = new Vector2(1000f, 600f);
		}

		void OnGUI()
		{
			FlexboxProjectSettingsRenderer.RenderSettingsPanel();
		}
#endif
	}
}