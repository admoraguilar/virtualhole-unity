//#define TEST_UPGRADE_SYSTEM

﻿using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using IntelligentPluginTools;
using UnityEditor;
using UnityEngine;
 using Random = UnityEngine.Random;

 public class TextureInGUI
 {
	 public Vector2 renderSize;
	 public string filenamePath;

	 public TextureInGUI( string fpath, Vector2 drawSize )
	 {
		 filenamePath = fpath;
		 renderSize = drawSize;
	 }
 }

 public class F4USplashWindow300 : IF4USplash
{
	public Vector2 DesiredSize()
	{
		return new Vector2(1200, 800 );
	}

	public Vector2 MinSize()
	{
		return new Vector2(700,500);
	}

	private void DisplayRowNewTextButton( EditorWindow window, float sizeMultiplier, TextureInGUI badge, float spaceAfterBadge, TextureInGUI icon, string firstText, GUIStyle textStyle, string buttonText, Action buttonScript, string suffixText = null )
	{
		using( new DivHorizontal() )
		{
			
				using( new DivHorizontal( GUILayout.Width( badge.renderSize.x + spaceAfterBadge )) ) /** This horrible mess is to get two images of slightly different heights to line up along centerline, which Unity doesn't support */
				{
					using( new Div( GUILayout.Height( icon.renderSize.y ) ) )
					{
						if( badge.filenamePath == null )
							DrawTextureExactly(Texture(Color.white), badge.renderSize.x + spaceAfterBadge, 1f); // Spacer to account for absent "NEW" image in this row
						else
						{
							GUILayout.Space( sizeMultiplier * ((icon.renderSize.y - badge.renderSize.y) / 2f) );
							DrawResourceImageInEditorWindow( window, badge );
						}
					}
				}
			
			GUILayout.Space( spaceAfterBadge );
			DrawResourceImageInEditorWindow( window, icon );

			GUILayout.Space(sizeMultiplier * 15f);
			//ESimpleHTML.RawText("Flex-wrap: You can enable this in the Advanced tab of any FlexContainer - please report any bugs via Forums or Discord (links below)", sMainTextNewFeature );
			ESimpleHTML.RawText( firstText, textStyle);

			if( buttonText != null )
			{
				if( GUILayout.Button( buttonText, _ClickableTextStyle( (int) (sizeMultiplier * 20) ) ) )
				{
					buttonScript();
				}
			}

			if( suffixText != null ) 
				ESimpleHTML.RawText( suffixText, textStyle );
						
			GUILayout.FlexibleSpace(); /** Required so that Unity doesn't space-around the different line-elements on this line */
		}
	}

	public void OnGUI( EditorWindow window, Rect windowSizePosition )
	{
		window.wantsMouseMove = true; /** Bug in Unity (all versions): this variable is "accidentally" deleted by UnityEditor on every reload of assemblies / recompile. We must re-set it every frame! */
		Vector2 windowSizeActualPixels = new Vector2( windowSizePosition.width*EditorGUIUtility.pixelsPerPoint, windowSizePosition.height*EditorGUIUtility.pixelsPerPoint);

		float wTarget = 1200f;
		float hTarget = 800f;
		float wFrac = windowSizeActualPixels.x / wTarget;
		float hFrac = windowSizeActualPixels.y / hTarget;
		float gDefault = 0.75f; // should be 1, but I ended up scaling it down late in design
		float g = windowSizeActualPixels.x < DesiredSize().x || windowSizeActualPixels.y < DesiredSize().y
			? (Mathf.Min(wFrac,hFrac) * gDefault)
			//? 0.85f
			: 1f * gDefault;
		
		Texture2D bgTexture = EditorGUIUtility.whiteTexture;
		if( bgTexture == null )
		{
			Debug.LogError("Missing plugin's Resources folder: should be a folder at " + window.FolderPathOfEditorClasses() + "/Resources");
			ESimpleHTML.H1("Welcome to Flexbox 4 Unity!", true);
			ESimpleHTML.P("Missing " + window.FolderPathOfEditorClasses() + "/Resources folder; please re-install this plugin from Asset Store");
			return;
		}

		float sw = windowSizePosition.width;
		float sh = windowSizePosition.height;
		sw = windowSizeActualPixels.x;
		sh = windowSizeActualPixels.y;
		GUI.DrawTextureWithTexCoords(new Rect(Vector2.zero, windowSizePosition.size), bgTexture, new Rect(0, 0, sw / bgTexture.width, sh / bgTexture.height));
		
		if( !(EditorProjectSettings.requireProjectSettings is Flexbox4UnityProjectSettings settings) )
		{
			ESimpleHTML.sH1.normal.textColor = ESimpleHTML.sH2.normal.textColor = ESimpleHTML.sPara.normal.textColor = Color.red;
			ESimpleHTML.sPara.fontSize = 15;

			ESimpleHTML.H1("Welcome to Flexbox 4 Unity!", true);

			GUILayout.Space(100);
			ESimpleHTML.P("Error: no settings file loaded. This should not be possible (settings file auto-loads when Unity Editor starts)");
		}
		else
		{
			ESimpleHTML.sH1.normal.textColor = ESimpleHTML.sH2.normal.textColor = ESimpleHTML.sPara.normal.textColor = new Color(0.35f, 0.35f, 0.35f);
			ESimpleHTML.sPara.fontSize = 15;

			using( new DivHorizontal() )
			{
				GUILayout.FlexibleSpace(); // forces justif-right
				ESimpleHTML.RawText("v" + Flexbox4UnityProjectSettings.builtVersion.ToStringFull()+" (Unity v" + Flexbox4UnityProjectSettings.builtForUnityVersion + ")", new GUIStyle() {fontSize = (int) (g * 12), padding = new RectOffset(0, 0, (int) (g * 4), 0), normal = new GUIStyleState() {textColor = new Color(0.25f, 0.25f, 0.25f)}});
			}
			using( var divHeader = new Div() )
			{
				GUILayout.Space(g * 40f);
				using( new Div(true) )
					DrawResourceImageInEditorWindow(window, "splash300-logotext", g);
				using( new Div(true) )
					DrawResourceImageInEditorWindow(window, "splash300-straplinetext", g);
				//ESimpleHTML.H2("Flexible UI-editing that adapts to your needs", true);
				GUILayout.Space(g * 10f);

				using( new Div(true) )
					DrawTextureExactly(Texture(Color.blue), Mathf.Max(1000f, 0.8f * windowSizeActualPixels.x), 6f);
			}

			GUILayoutExtensions.FlexibleSpace(2); /** Two of them, so that the space is twice as big above/below the main area as it is between the rows */

			Vector2 sizeImageNEW = g * new Vector2(105, 33);
			Vector2 sizeImageInfo = g * new Vector2(48, 48);
			TextureInGUI badgeNew = new TextureInGUI( "splash300-newtext", sizeImageNEW );
			TextureInGUI badgeNotNew = new TextureInGUI( null, sizeImageNEW );
			TextureInGUI iconInfo = new TextureInGUI( "Icons/icon-info", g * 48f * Vector2.one );
			TextureInGUI iconStar = new TextureInGUI( "Icons/icon-star", g * 48f * Vector2.one );
			TextureInGUI iconLink = new TextureInGUI( "Icons/icon-link", g * 48f * Vector2.one );
			TextureInGUI iconTicked = new TextureInGUI( "Icons/icon-ticked", g * 48f * Vector2.one );
			TextureInGUI iconUnticked = new TextureInGUI( "Icons/icon-unticked", g * 48f * Vector2.one );
			GUIStyle sMainTextNewFeature = new GUIStyle() {fontSize = (int) (g * 20), normal = new GUIStyleState() {textColor = Color.grey}, wordWrap = true};
			Action<TextureInGUI, string > AddRow_IconText = ( icon, mainText ) => DisplayRowNewTextButton( window, g, badgeNotNew, g * 7f, icon, mainText, sMainTextNewFeature, null, () => { } );
			Action<string, string, Action> AddRow_TextWithButton = ( mainText, buttonText, exec ) => DisplayRowNewTextButton( window, g, badgeNotNew, g * 7f, iconInfo, mainText, sMainTextNewFeature, buttonText, exec );
			Action<string, string, Action> AddRow_New_TextWithButton = ( mainText, buttonText, exec ) => DisplayRowNewTextButton( window, g, badgeNew, g * 7f, iconInfo, mainText, sMainTextNewFeature, buttonText, exec );
			Action<TextureInGUI, string, string, Action> AddRow_IconTextWithButton = ( icon, mainText, buttonText, exec ) => DisplayRowNewTextButton( window, g, badgeNotNew, g * 7f, icon, mainText, sMainTextNewFeature, buttonText, exec );
			Action<TextureInGUI, string, string, Action> AddRow_New_IconTextWithButton = ( icon, mainText, buttonText, exec ) => DisplayRowNewTextButton( window, g, badgeNew, g * 7f, icon, mainText, sMainTextNewFeature, buttonText, exec );
			Action<TextureInGUI, string, string, string, Action> AddRow_IconTextWithButtonText = ( icon, mainText, buttonText, suffixText, exec ) => DisplayRowNewTextButton( window, g, badgeNotNew, g * 7f, icon, mainText, sMainTextNewFeature, buttonText, exec, suffixText );

			/** Note: Massive Unity bug: GUILayout uses points, but GUI uses pixels. This line below (GUILayout.MaxWidth) is in points... */
			using( var divBodyWithSideMargins = new DivHorizontal( true, GUILayout.MaxWidth(Mathf.Min(800f, 0.65f * windowSizePosition.width))) )
			{
				GUILayout.Space(g * 25f); // with a relatively wide (0.65f*) width for the full div, it looks weirdly offset to left, so we shift the whole thing right

				float interLineSpace = 0f;
				using( var listInsideBody = new Div(true) )
				{
					AddRow_New_IconTextWithButton( iconInfo, "Release notes: ", "v" + Flexbox4UnityProjectSettings.builtVersion, () => { F4UReleaseNotesWindow.Init(); } );
					
					GUILayout.Space( interLineSpace );
					
					bool isFirstRun = ! settings.hasDisplayedFirstStartup;
					if( isFirstRun )
						AddRow_IconTextWithButton( iconLink, "Start here:", "Getting Started guide", () => Application.OpenURL( "http://flexbox4unity.com/2020/06/05/guide-using-flexbox-in-unity-2020/?utm_source=Editor&utm_medium=AboutScreen" ) );
					else 
						AddRow_IconTextWithButtonText( iconStar, "Have you ", "written a review", "yet?", () => {
							EditorPrefs.SetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "HasClickedWriteReview", true);
							Application.OpenURL("https://assetstore.unity.com/packages/tools/gui/flexbox-4-unity-139571#reviews");
						} );

					if( F4UUpgrader.lastUpgradedToVersion < Flexbox4UnityProjectSettings.builtVersion )
					{
						AddRow_TextWithButton( "<color=red>v" + F4UUpgrader.lastUpgradedToVersion+"</color>", "[UPGRADE NEEDED]", () =>
							{ 
								F4UUpgrader.AutoUpgradeSceneAndPrefabs();
							}
						);
					}
					#if TEST_UPGRADE_SYSTEM
					else // only if testing!
						AddRow_TextWithButton( "v" + F4UUpgrader.lastUpgradedToVersion, "[click to wipe]", () =>
						{
							F4UUpgrader.TESTING_wipeUpgradedVersionToZero();
						});
					#endif

					/*
					GUILayout.Space( interLineSpace );
					
					AddRow_New_TextWithButton( "Responsive Design:", "online guide", () => Application.OpenURL( "http://flexbox4unity.com/2020/06/26/guide-how-to-make-responsive-design-layouts-in-unity-3d/?utm_source=Editor&utm_medium=AboutScreen" ) );

					if( !F4UAnonymousEditorStats.isRecordingAnonymousUsage )
					{
						GUILayout.Space( interLineSpace );
						AddRow_TextWithButton( "Usage stats", "(click for more info)", () => F4UWindowUsageStats.Init() );
					}
					*/

					GUILayout.Space( interLineSpace );

					if( F4URegistration.IsRegistered() )
						AddRow_IconText( iconTicked, "<color=green>Thanks for registering!</color>" );
					else
						AddRow_IconTextWithButton( iconUnticked, "<color=orange>Register for faster support</color>", "(click to register (free))", () => F4UWindowRegistration.Init() );
					
				}
			}

			GUILayoutExtensions.FlexibleSpace(3); /** Two of them, so that the space is twice as big above/below the main area as it is between the rows */

			using( var divFooter = new Div() )
			{
				using( new DivHorizontal(true) )
				{
					GUILayout.Space(g * 10f);
					GUIStyle sFooterPlainText = new GUIStyle() {fontSize = (int) (g * 24), normal = new GUIStyleState() {textColor = Color.grey}};
					
					var sLinks = _ClickableTextStyle((int) (g * 22));
					DrawResourceImageInEditorWindow(window, "Icons/icon-textfile", g * 0.75f);
					if( GUILayout.Button("Docs (PDF)", sLinks) ) Application.OpenURL(FlexboxSettingsLoader.pathToPDFCurrentDocs);

					GUILayout.Space(g * 35f);
					DrawResourceImageInEditorWindow(window, "Icons/icon-link", g * 0.75f);
					if( GUILayout.Button("Forums", sLinks) ) Application.OpenURL("https://forum.unity.com/threads/released-flexbox-fast-easy-layout-from-html-css-in-unity-2017-2018-2019.699749/");

					ESimpleHTML.RawText(" | ", sFooterPlainText);
					if( GUILayout.Button("Website", sLinks) ) Application.OpenURL("https://flexbox4unity.com");

					ESimpleHTML.RawText(" | ", sFooterPlainText);
					if( GUILayout.Button("Discord", sLinks) ) Application.OpenURL("https://discord.gg/umXJq4c");

					GUILayout.Space(g * 35f);
					DrawResourceImageInEditorWindow(window, "Icons/icon-email", g * 0.75f);
					if( GUILayout.Button("Support", sLinks) )
					{
						F4URegistration.SendSupportEmail();
					}

					GUILayout.Space(g * 10f);
				}

				GUILayout.Space(g * 10f);
			}



			// Required by Unity's own official docs if you want button-hover rendering to actually work - ugly, but official!
			if( Event.current.type == EventType.MouseMove )
				window.Repaint();
		}
	}

	private GUIStyle _ClickableTextStyle(int fontSize)
	{
		/** NB: MASSIVE bug in Unity all versions since 2008: if a GUILayout.Button state has no background image, Unity IGNORES the settings
					 * ...you MUST use "ImagePosition" to remove the image (isn't this a terrible API? :))
					 */
		return new GUIStyle()
		{
			contentOffset = Vector2.zero,
			alignment = TextAnchor.MiddleCenter,
			imagePosition = ImagePosition.TextOnly,
			fontSize = fontSize,
			border = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(4, 4, 0, 4), /** Unity bug (all versions): Padding-top on Buttons moves ADJACENT items UP for no reason */
			margin = new RectOffset(0, 0, 0, 0),
			normal = new GUIStyleState() {textColor = Color.blue},
			hover = new GUIStyleState() {textColor = Color.grey, background = EditorGUIUtility.whiteTexture},
		};
	}

	private void DrawResourceImageInEditorWindow( EditorWindow window, TextureInGUI sizedImage, float sizeMultiplier = 1f )
	{
		DrawResourceImageInEditorWindow( window, sizedImage.filenamePath, sizedImage.renderSize.x, sizedImage.renderSize.y, sizeMultiplier );
	}

	private void DrawResourceImageInEditorWindow(EditorWindow window, string filename, Vector2 size)
	{
		DrawResourceImageInEditorWindow(window, filename, size.x, size.y);
	}
	
	/** SizeMultiplier so we can auto-scale entire GUI up/down */
	private void DrawResourceImageInEditorWindow(EditorWindow window, string filename, float sizeMultiplier )
	{
		DrawResourceImageInEditorWindow(window, filename, -1, -1, sizeMultiplier );
	}

	/** SizeMultiplier so we can auto-scale entire GUI up/down */
	private void DrawResourceImageInEditorWindow( EditorWindow window, string filename, float w, float h, float sizeMultiplier = 1f)
	{
		if( !filename.Contains(".") )
			filename = filename + ".png";
		
		Texture2D t_logo = window.LoadTextureFromRelativePath("Resources/"+filename);
		if( w > 0 && h > 0 )
			DrawTextureExactly(t_logo, w, h);
		else
			DrawTextureExactly(t_logo, sizeMultiplier );
	}
	
	/** SizeMultiplier so we can auto-scale entire GUI up/down */ 
	private void DrawTextureExactly( Texture2D texture, float sizeMultiplier = 1f )
	{
		DrawTextureExactly( texture, texture.width * sizeMultiplier, texture.height * sizeMultiplier);
	}
	
	/**
	 * Workaround MASSIVE (and undocumented) bugs in Unity's implementation of 4k / Retina monitors:
	 *
	 * If you tell Unity "draw 100x100px", Unity will draw "200x200" on a retina screen
	 * If you tell Unity "draw texture at (texture size)", Unity will draw it blurry and 2x too large on a retina screen
	 * If you tell Unity "draw (size of window)", Unity will draw THE SIZE OF THE WINDOW (note how this is incompatible with the two items above)
	 */
	private void DrawTextureExactly( Texture2D texture, float tWidth, float tHeight )
	{
		float twidthRetinaAdjusted = tWidth / EditorGUIUtility.pixelsPerPoint;
		float tHeightRetinaAdjusted = tHeight / EditorGUIUtility.pixelsPerPoint;
		GUI.DrawTexture(GUILayoutUtility.GetRect(twidthRetinaAdjusted, tHeightRetinaAdjusted, GUILayout.MaxWidth(twidthRetinaAdjusted), GUILayout.MaxHeight(tHeightRetinaAdjusted)), texture);
	}
	
	private Texture2D Texture(Color c)
	{
		Texture2D tex = new Texture2D(1, 1);
		tex.SetPixel(0, 0, c);
		tex.Apply();
		return tex;
	}
}