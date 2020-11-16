using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using IntelligentPluginTools;
using UnityEditor;
using UnityEngine;

public class F4USplashWindow200 : IF4USplash
{
	public Vector2 DesiredSize()
	{
		return new Vector2(1800, 1200 );
	}

	public Vector2 MinSize()
	{
		return new Vector2(1700,750);
	}

	public void OnGUI( EditorWindow window, Rect windowSizePosition )
	{
		Texture2D bgTexture = window.LoadTextureFromRelativePath("Resources/bg-grid1.png");
		if( bgTexture == null )
		{
			Debug.LogError("Missing plugin's Resources folder: should be a folder at " + window.FolderPathOfEditorClasses() + "/Resources");
			ESimpleHTML.H1("Welcome to Flexbox 4 Unity!", true);
			ESimpleHTML.P("Missing " + window.FolderPathOfEditorClasses() + "/Resources folder; please re-install this plugin from Asset Store");
			return;
		}

		float sw = windowSizePosition.width;
		float sh = windowSizePosition.height;
		GUI.DrawTextureWithTexCoords(new Rect(Vector2.zero, windowSizePosition.size), bgTexture, new Rect(0, 0, sw / bgTexture.width, sh / bgTexture.height));

		if( !(EditorProjectSettings.requireProjectSettings is Flexbox4UnityProjectSettings settings) )
		{
			ESimpleHTML.sH1.normal.textColor = ESimpleHTML.sH2.normal.textColor = ESimpleHTML.sPara.normal.textColor = Color.white;
			ESimpleHTML.sPara.fontSize = 15;

			ESimpleHTML.H1("Welcome to Flexbox 4 Unity!", true);

			GUILayout.Space(100);
			ESimpleHTML.P("Error: no settings file loaded. This should not be possible (settings file auto-loads when Unity Editor starts)");
		}
		else
		{
			/** Load all the textures, since Unity GUI isn't good at this*/
			//	Texture texBigLogo = Resources.Load ("logo-200x200") as Texture;
			//  GUIContent unityBigLogo = new GUIContent (texBigLogo);
//          int paddingBetweenColumns = 50;
//          int marginsEitherSideOfPage = 25;

			ESimpleHTML.sH1.normal.textColor = ESimpleHTML.sH2.normal.textColor = ESimpleHTML.sPara.normal.textColor = Color.white;
			ESimpleHTML.sPara.fontSize = 15;

			Texture2D imagePreHeader = window.LoadTextureFromRelativePath("Resources/paint-2924891_1920-flipped.jpg");

			float preHeaderHeight = imagePreHeader.height;
			preHeaderHeight = Math.Min(preHeaderHeight, sh * 0.2f); /** when window is small, full height image looks crappy */
			float preHeaderHeight_workaroundUnityRetina_Bugs = preHeaderHeight / EditorGUIUtility.pixelsPerPoint;
			Rect rectPreHeader = EditorGUILayout.BeginHorizontal(GUILayout.Height(preHeaderHeight_workaroundUnityRetina_Bugs), GUILayout.MaxHeight(preHeaderHeight_workaroundUnityRetina_Bugs)); // have to pre-assign it here AND insert an explicit .Space below (bugs in Unity)				
			GUI.DrawTextureWithTexCoords(
				rectPreHeader,
				imagePreHeader,
				new Rect(0, 0, 1f, 1f)
			);
			GUILayout.Space(preHeaderHeight_workaroundUnityRetina_Bugs); // have to insert explicit .Space here AND pre-assign it in .BeginHorizontal (bugs in Unity)
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(20f);
			ESimpleHTML.DivCentered();
			Texture2D t_logo = window.LoadTextureFromRelativePath("Resources/logo-text-only.png");
			// NB: some major bug in Unity GUILayout gets image-rects wrong for textures by a consistent factor of 1.25x height on my screen
			// ... I suspect it's a bug in their handling of Retina displays, which Unity was very slow to fix generally.
			GUI.DrawTexture(GUILayoutUtility.GetRect(t_logo.width, t_logo.height, GUILayout.MaxWidth(t_logo.width), GUILayout.MinHeight(t_logo.height * 1.25f)), t_logo);
			ESimpleHTML.DivCenteredEnd();

			ESimpleHTML.H2("Version " + Flexbox4UnityProjectSettings.builtVersion.ToStringFull() + " / Built in Unity <b>" + Flexbox4UnityProjectSettings.builtForUnityVersion + "</b>", true);

			ESimpleHTML.H1("<b>Latest update:</b> <color=red>Experimental flex-wrap implementation</color>, please register to test", true);
			ESimpleHTML.DivCentered();
			if( GUILayout.Button("Release notes...") )
			{
				F4UReleaseNotesWindow.Init();
			}

			ESimpleHTML.DivCenteredEnd();
			GUILayout.Space(10f);



			GUILayout.FlexibleSpace();



			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			ESimpleHTML.Div(GUILayout.Width(300f), GUILayout.MaxWidth(300f));
			GUILayout.FlexibleSpace();

			ESimpleHTML.P("Flexbox4Unity global settings", new Color(0.5f, 1f, 0f), true);
			if( GUILayout.Button("Edit...") )
			{
#if UNITY_2018_3_OR_NEWER
				SettingsService.OpenProjectSettings("Project/Flexbox4");
#else
					F4UOldSettingsWindow.Init();	
#endif
			}

			GUILayout.FlexibleSpace();
			ESimpleHTML.DivEnd();

			GUILayout.FlexibleSpace();

			Texture2D t3 = window.LoadTextureFromRelativePath("Resources/html5-and-css3-transparent-background-html-logo-hd.png");
			ESimpleHTML.Div(GUILayout.MaxWidth(t3.width / 3f), GUILayout.MaxHeight(t3.height / 3f)); // bizarre bug in Unity 2018 onwards: UnityEditor multiplies texture width and height by two for SOME images only!

			Rect rectCentralLogos = GUILayoutUtility.GetRect(t3.width / 3f, t3.height / 3f); // bizarre bug in Unity 2018 onwards: UnityEditor multiplies texture width and height by two for SOME images only!
			GUI.DrawTexture(rectCentralLogos, t3);

			ESimpleHTML.DivEnd();

			GUILayout.FlexibleSpace();

			ESimpleHTML.Div(GUILayout.Width(300f), GUILayout.MaxWidth(300f));
			GUILayout.FlexibleSpace();
			if( F4URegistration.IsRegistered() )
			{
				ESimpleHTML.P("Registered!" /** Note: major bug in UnityEditor, GUILayout.Label often deletes the last word in a label if it has no punctuation after it */, new Color(0f, 1f, 0), true);
				if( GUILayout.Button("View/Edit Registration") )
				{
					F4UWindowRegistration.Init();
				}
			}
			else
			{
				ESimpleHTML.P("Register for faster support " /** Note: major bug in UnityEditor, GUILayout.Label often deletes the last word in a label if it has no punctuation after it */, new Color(1f, 0.5f, 0), true);
				if( GUILayout.Button("Register (free)") )
				{
					F4UWindowRegistration.Init();
				}
			}

			GUILayout.FlexibleSpace();
			ESimpleHTML.DivEnd();

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();



			GUILayout.Space(60f);

			GUILayout.FlexibleSpace();

			Texture2D imageFooter = window.LoadTextureFromRelativePath("Resources/paint-2924891_1920.jpg");
			float footerHeight = imageFooter.height;
			/** when window is small, full height image looks crappy */
			footerHeight = Math.Min(footerHeight, sh * 0.2f);
			float footerHeight_workaroundUnityRetina_Bugs = footerHeight / EditorGUIUtility.pixelsPerPoint;
			Rect rectFooter; // = EditorGUILayout.GetControlRect(false, footerHeight); 
			rectFooter = EditorGUILayout.BeginHorizontal(GUILayout.Height(footerHeight_workaroundUnityRetina_Bugs), GUILayout.MaxHeight(footerHeight_workaroundUnityRetina_Bugs)); // have to pre-assign it here AND insert an explicit .Space below (bugs in Unity)

			//rectFooter.size = new Vector2(rectFooter.size.x, 50f);

			//texCoordY = 0.75f;
			/**
			 * Multiple horrific bugs in UnityEditor: ...basically: whoever at Unity added the Retina/4k/HiDPI monitor support to UnityEditor did a shamefully bad job, failed to edit half the
			 * places they were supposed to, CLEARLY NEVER TESTED THEIR CODE, and most of all: was too lazy to document ANYTHING they did (so the official
			 * docs still have NO MENTION of any of this).
			 */
			GUI.DrawTextureWithTexCoords(
				rectFooter,
				imageFooter,
				new Rect(0, 0, 1f, 1f)
			);
			GUILayout.Space(footerHeight_workaroundUnityRetina_Bugs); // have to insert explicit .Space here AND pre-assign it in .BeginHorizontal (bugs in Unity)
			EditorGUILayout.EndHorizontal();

			//EditorGUILayout.Space();
		}
	}
}