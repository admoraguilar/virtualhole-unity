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
	[Serializable]
	public class AnonymizedUsageStats
	{
		public int containersCreated;
		public int itemsCreated;
		public List<string> flexTemplatesUsed;
	}
	
	public class F4UWindowUsageStats : EditorWindow
	{
		[MenuItem("Window/Flexbox/Usage Stats")]
		public static void WindowFlexboxUsageStats()
		{
			EditorStats.sharedInstance.SendEvent(  "editor", "app-menu","usage-stats", 1);
			Init();
		}
		
		public static void Init()
		{
			F4UWindowUsageStats ew = EditorWindow.GetWindow<F4UWindowUsageStats>( false, "Flexbox4Unity - Stats");
			//ew.ResizeWindowWorkaround( new Vector2(1800, 1200 ), new Vector2(1700,750) );
			ew.ResizeWindowWorkaround( new Vector2(1800, 1200 ), new Vector2(900,720) );
		}

		private Vector2 _scrollPosition;
		void OnGUI()
		{
			wantsMouseMove = true; /** Bug in Unity (all versions): this variable is "accidentally" deleted by UnityEditor on every reload of assemblies / recompile. We must re-set it every frame! */
			Vector2 windowSizeActualPixels = new Vector2( position.width*EditorGUIUtility.pixelsPerPoint, position.height*EditorGUIUtility.pixelsPerPoint);

			var stats = F4UAnonymousEditorStats.liveStats;
			
			Texture2D bgTexture = EditorGUIUtility.whiteTexture;
			GUI.DrawTextureWithTexCoords(new Rect(Vector2.zero, windowSizeActualPixels), bgTexture, new Rect(0, 0, windowSizeActualPixels.x / bgTexture.width, windowSizeActualPixels.y / bgTexture.height));
			
			ESimpleHTML.H1("Editor Stats for Flexbox4Unity", true );
			using( new DivHorizontal(true))
			ESimpleHTML.P("(access at any time from Menu: <color=blue>Window > Flexbox > Usage Stats</color>)" );
			
			using( new Div(true) )
			{
				ESimpleHTML.H2("Enable/disable:");
				using( new DivHorizontal() )
				{
					ESimpleHTML.P("Record usage-stats?" );

					if( F4UAnonymousEditorStats.isRecordingAnonymousUsage )
					{
						ESimpleHTML.P("<color=green>ON</color>" );
						if( GUILayout.Button("DISABLE") )
						{
							F4UAnonymousEditorStats.SetRecordingUsageAnonymously(false);
							Repaint();
						}
					}
					else
					{
						ESimpleHTML.P("<color=red>OFF</color>" );
						if( GUILayout.Button("ENABLE") )
						{
							F4UAnonymousEditorStats.SetRecordingUsageAnonymously(true);
							Repaint();
						}
					}

					GUILayout.FlexibleSpace();
				}
				
				if( F4UAnonymousEditorStats.isRecordingAnonymousUsage )
				ESimpleHTML.P("<color=green>Anonymous usage stats are being shared with: flexbox4unity.com (GoogleAnalytics)</color>" );

				bool hasData = F4UAnonymousEditorStats.isRecordingAnonymousUsage && (stats != null);
				ESimpleHTML.H2("Privacy:");
				ESimpleHTML.P("This info is ONLY used to help me fix bugs and prioritize new features (it tells me which features people are using)." );
				ESimpleHTML.P("NOTHING is included in your game/app (all usage-stats are Editor-only, and are automatically removed by Unity when creating builds)" );
				ESimpleHTML.P_Selectable("Any questions, please email me: adam.m.s.martin@gmail.com", null, null);
				ESimpleHTML.H2("Current stats");
				using( var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition) )
				{
					_scrollPosition = scrollView.scrollPosition;
					ESimpleHTML.P("Editor version: " + (hasData ? UnityEditorVersionDetector.unityVersion : "-"));
					ESimpleHTML.P("Editor random SessionID: " + (hasData ? EditorStats.anonymousSessionID.ToString() : "-"));
					ESimpleHTML.P("# FlexContainers created: " + (hasData ? "" + stats.containersCreated : "-"));
					ESimpleHTML.P("# FlexItems created: " + (hasData ? "" + stats.itemsCreated : "-"));

					ESimpleHTML.P("# flexGrowUsed: " + (hasData ? "" + stats.flexGrowUsed : "-"));
					ESimpleHTML.P("# flexShrinkUsed: " + (hasData ? "" + stats.flexShrinkUsed : "-"));
					ESimpleHTML.P("# flexPaddingUsed: " + (hasData ? "" + stats.flexPaddingUsed : "-"));
					ESimpleHTML.P("# flexMarginsUsed: " + (hasData ? "" + stats.flexMarginsUsed : "-"));
					ESimpleHTML.P("# flexDirectionUsed: " + (hasData ? "" + stats.flexDirectionUsed : "-"));
					ESimpleHTML.P("# flexSizeConstraintsUsed: " + (hasData ? "" + stats.flexSizeConstraintsUsed : "-"));
					ESimpleHTML.P("# flexDefaultSizesUsed: " + (hasData ? "" + stats.flexDefaultSizesUsed : "-"));
					ESimpleHTML.P("# flexOrderUsed: " + (hasData ? "" + stats.flexOrderUsed : "-"));
					ESimpleHTML.P("# flexExpandUsed: " + (hasData ? "" + stats.flexExpandUsed : "-"));
					ESimpleHTML.P("# flexJustifyUsed: " + (hasData ? "" + stats.flexJustifyUsed : "-"));
					ESimpleHTML.P("# flexAlignUsed: " + (hasData ? "" + stats.flexAlignUsed : "-"));

					ESimpleHTML.P("Flex-templates used: " + (hasData ? "" + stats.flexTemplatesUsed : "-"));
					ESimpleHTML.P("Screen resolution: " + (hasData ? Screen.currentResolution.width + " x " + Screen.currentResolution.height + " px" : "-"));
					ESimpleHTML.P("Screen 4k/HiDPI/Retina: " + (hasData ? "" + EditorGUIUtility.pixelsPerPoint : "-"));
				}
			}
		}
	}
}
#endif