using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IntelligentPluginTools;
using IntelligentPluginVersioning;
using UnityEditor;
using UnityEngine;
using static Flexbox4Unity.WorkaroundUnityMissingEditorAPIs;

namespace Flexbox4Unity
{

	public class F4UReleaseNotesWindow : EditorWindow
	{
		public VersionLog versionLog;
		private Vector2 _lastScrollPositionLHS, _lastScrollPositionRHS;

		[MenuItem("Window/Flexbox/Release Notes")]
		public static void Init()
		{
			float sw = Screen.currentResolution.width;
			float sh = Screen.currentResolution.height;
			
			Vector2 desiredSize = new Vector2(Math.Min(1600, sw * 0.5f), Math.Min(1000, sh * 0.5f));
			
			F4UReleaseNotesWindow ew = EditorWindow.GetWindow<F4UReleaseNotesWindow>(false, "Release Notes");
			
			/** Experimentally determined workarounds UNDOCUMENTED BY UNITY for UnityEditor bugs */
			ew.position = new Rect(
				new Vector2((sw - desiredSize.x) / 2f, (sh - desiredSize.y) / 2f) * 1f / EditorGUIUtility.pixelsPerPoint,
				desiredSize * 1f / EditorGUIUtility.pixelsPerPoint /** NB: Unity SOMETIMES multiplies by .pixelsPerPoint, and OTHER TIMES multiplies by 1.33, semi-randomly, and depending on if the Window was already on screen or not */
			);

			ew.maxSize = new Vector2(sw / EditorGUIUtility.pixelsPerPoint, sh / EditorGUIUtility.pixelsPerPoint);
			ew.minSize = new Vector2(800f / EditorGUIUtility.pixelsPerPoint, 600f / EditorGUIUtility.pixelsPerPoint);
			ew.versionLog = Flexbox4UnityVersionHistory.UpdateLog();
		}

		void OnGUI()
		{
			Texture2D bgTexture = this.LoadTextureFromRelativePath("Resources/bg-grid1.png");
			if( bgTexture == null )
			{
				Debug.LogError("Missing plugin's Resources folder: should be a folder at " + this.FolderPathOfEditorClasses() + "/Resources");
				ESimpleHTML.H1("Welcome to Flexbox 4 Unity!", true);
				ESimpleHTML.P("Missing " + this.FolderPathOfEditorClasses() + "/Resources folder; please re-install this plugin from Asset Store");
				return;
			}

			float sw = position.width;
			float sh = position.height;
			TileTextureAcrossRect(bgTexture, new Rect(Vector2.zero, position.size));

			{
				ESimpleHTML.sH1.normal.textColor = ESimpleHTML.sH2.normal.textColor = ESimpleHTML.sPara.normal.textColor = Color.white;
				ESimpleHTML.sPara.fontSize = 15;




				Rect headerRect = ESimpleHTML.Div(); // Header
				Texture2D headerBackground = this.LoadTextureFromRelativePath("Resources/logocss3.png");
				TileTextureAcrossRect(headerBackground, headerRect);
				TileTextureAcrossRect(TextureColoured(new Color(0f,0f,0f,0.35f)), headerRect);


				ESimpleHTML.DivCentered();
				Texture2D t_logo = this.LoadTextureFromRelativePath("Resources/logo-text-only.png");
				// NB: some major bug in Unity GUILayout gets image-rects wrong for textures by a consistent factor of 1.25x height on my screen
				// ... I suspect it's a bug in their handling of Retina displays, which Unity was very slow to fix generally.
				float tw = t_logo.width / EditorGUIUtility.pixelsPerPoint;
				float th = t_logo.height / EditorGUIUtility.pixelsPerPoint;
				GUI.DrawTexture(GUILayoutUtility.GetRect(tw, th, GUILayout.MaxWidth(tw), GUILayout.MaxHeight(th)), t_logo);
				ESimpleHTML.DivCenteredEnd();


				ESimpleHTML.DivEnd(); // Header

				Rect subHeaderRect = ESimpleHTML.Div();
				TileTextureAcrossRect(TextureColoured(new Color(0.8f, 0.8f, 0.8f)), subHeaderRect);
				ESimpleHTML.DivCentered();
				ESimpleHTML.H1("Release Notes", new GUIStyle(ESimpleHTML.sH1) {normal = new GUIStyleState() {textColor = new Color(0.8f, 0.4f, 0)}});
				ESimpleHTML.DivCenteredEnd();
				ESimpleHTML.DivEnd();

				/*
				GUILayout.Space(1);
				ESimpleHTML.H2("2020 / Version ", true);
				GUILayout.Space(30f);
				ESimpleHTML.H1("<b>Latest update:</b> New docs + Custom Inspectors", true);
	
				GUILayout.FlexibleSpace();
	*/

				EditorGUILayout.BeginHorizontal();

				Rect lhsRect = ESimpleHTML.Div(GUILayout.Width(400f / EditorGUIUtility.pixelsPerPoint));
				TileTextureAcrossRect(TextureColoured(new Color(0.89f, 0.89f, 0.89f)), lhsRect);

				_lastScrollPositionLHS = EditorGUILayout.BeginScrollView(_lastScrollPositionLHS);
				ESimpleHTML.H2("Asset Store releases", new Color(0.1f, 0.1f, 0.1f));
				int maxRowsToShow = 100;
				ESimpleHTML.sPara.normal.textColor = Color.black;
				foreach( var row in Flexbox4UnityVersionHistory.UpdateLog().AllVersions().Reverse() )
					//foreach( var row in settings.UpdateLog().AllSignificantsAndPatchesFrom( settings.lastRuntimeVersion ).Reverse() )
				{
					ESimpleHTML.SPAN(new string[] {"v" + row.version.ToStringFull() /** extra space required due to vicious bug inside Unity's core rendering algorithm for Label */ + " ", row.releaseDate.ToString("d MMM yyyy")}, 30, ESimpleHTML.SpanAlignment.JUSTIFY, null);
					maxRowsToShow--;
					if( maxRowsToShow <= 0 )
						break;
				}

				GUILayout.FlexibleSpace();
				ESimpleHTML.P("Powered by IntelligentVersionManager v2020.2", 0, Color.blue);
				EditorGUILayout.EndScrollView();

				ESimpleHTML.DivEnd();

				Rect rhsRect = ESimpleHTML.Div();
				Texture2D gridBackgroundLight = this.LoadTextureFromRelativePath("Resources/bg-grid1.png");
				TileTextureAcrossRect(gridBackgroundLight, rhsRect);
				//TileTextureAcrossRect(TextureColoured(new Color(0f,0f,0f,0.00135f)), rhsRect);

				_lastScrollPositionRHS = EditorGUILayout.BeginScrollView(_lastScrollPositionRHS);
				Color unBoldRowColor = new Color(0.85f,0.85f,0.85f);
				Color boldRowColor = new Color(0.5f,1f,1f);
				Color TitleRowColor = new Color(1f,1f,1f);
				ESimpleHTML.sPara.normal.textColor = unBoldRowColor;
				maxRowsToShow = 100;
				IntelligentPluginVersioning.Version _lastRowVersion = IntelligentPluginVersioning.Version.zero;
				bool isFirstRow = true;
				foreach( var row in Flexbox4UnityVersionHistory.UpdateLog().All().Reverse() )
					//foreach( var row in settings.UpdateLog().AllSignificantsAndPatchesFrom( settings.lastRuntimeVersion ).Reverse() )
				{
					if( _lastRowVersion != row.version )
					{
						if( isFirstRow )
							isFirstRow = false;
						else
						{
							EditorGUILayout.EndVertical();
							EditorGUILayout.EndHorizontal();
						}

						GUILayout.Space(15f);
						ESimpleHTML.sPara.normal.textColor = TitleRowColor;
						ESimpleHTML.P("Version: " + row.version, 30, null);
						_lastRowVersion = row.version;
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(50f / EditorGUIUtility.pixelsPerPoint);
						EditorGUILayout.BeginVertical();
					}

					using( new DivHorizontal() )
					{
						ESimpleHTML.sPara.normal.textColor = row.isMajor ? boldRowColor : unBoldRowColor;
						ESimpleHTML.P("• ", 30, null);
						string text = row.description;
						ESimpleHTML.P(text, null);
					}

					maxRowsToShow--;
					if( maxRowsToShow <= 0 )
						break;

				}

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndScrollView();
				ESimpleHTML.DivEnd();


				EditorGUILayout.EndHorizontal();
			}
		}

	}
}