using System.Collections;
using System.Collections.Generic;
using IntelligentPluginTools;
using UnityEditor;
using UnityEngine;
using static Flexbox4Unity.WorkaroundUnityMissingEditorAPIs;

namespace Flexbox4Unity
{
	public class F4UWindowRegistration : EditorWindow
	{
		private string _invoiceNumber;
		[MenuItem("Window/Flexbox/Register")]
		public static void Init()
		{
			float sw = Screen.currentResolution.width;
			float sh = Screen.currentResolution.height;

			Vector2 desiredSize = new Vector2(600f, 335);
			Vector2 minSize = new Vector2(600f, 335);

			F4UWindowRegistration ew = EditorWindow.GetWindow<F4UWindowRegistration>(false, "Register Flexbox 4 Unity...");

			/** Experimentally determined workarounds UNDOCUMENTED BY UNITY for UnityEditor bugs */
			ew.position = new Rect(
				new Vector2((sw - desiredSize.x) / 2f, (sh - desiredSize.y) / 2f) * 1f / EditorGUIUtility.pixelsPerPoint,
				desiredSize * 1f / EditorGUIUtility.pixelsPerPoint /** NB: Unity SOMETIMES multiplies by .pixelsPerPoint, and OTHER TIMES multiplies by 1.33, semi-randomly, and depending on if the Window was already on screen or not */
			);

			ew.minSize = new Vector2(minSize.x/ EditorGUIUtility.pixelsPerPoint, minSize.y / EditorGUIUtility.pixelsPerPoint);
			ew.maxSize = new Vector2(1000f/ EditorGUIUtility.pixelsPerPoint, ew.minSize.y);

			ew._invoiceNumber = F4URegistration.GetRegisteredInvoiceNumber();
			EditorStats.sharedInstance.SendEvent(  "editor", "app-menu","registration", 1);
		}

		void OnGUI()
		{
			Texture2D bgTexture = this.LoadTextureFromRelativePath("Resources/bg-grid2.png");
			if( bgTexture == null )
			{
				Debug.LogError("Missing plugin's Resources folder: should be a folder at " + this.FolderPathOfEditorClasses() + "/Resources");
				ESimpleHTML.H1("Welcome to Flexbox 4 Unity!", true);
				ESimpleHTML.P("Missing " + this.FolderPathOfEditorClasses() + "/Resources folder; please re-install this plugin from Asset Store");
				return;
			}

			float sw = position.width;
			float sh = position.height;
			

			{
				ESimpleHTML.sH1.normal.textColor = ESimpleHTML.sH2.normal.textColor = ESimpleHTML.sPara.normal.textColor = Color.white;
				ESimpleHTML.sPara.fontSize = 15;


				Rect rectFull = EditorGUILayout.BeginVertical("Box");
				TileTextureAcrossRect(bgTexture, rectFull );

				if( F4URegistration.IsRegistered() )
					EditorGUILayout.HelpBox("Already registered!", MessageType.Warning);
				else
					EditorGUILayout.HelpBox("If you register the plugin, you'll get faster support, and email updates about new features and upcoming releases.", MessageType.Info);
				
				EditorGUILayout.LabelField("Package name", "Flexbox4Unity");
				_invoiceNumber = EditorGUILayout.TextField("Invoice number", _invoiceNumber);
				EditorGUILayout.LabelField(" ", "e.g. IN010100000000");
				string instructionsText = "To find your Invoice number:\n 1. login to the asset Store from a web-browser\n 2.click your Avatar icon in top right\n 3. select 'My Orders'\n 4.copy/paste the Invoice Number for the order where you bought this package";
				float requiredHeight = EditorStyles.textArea.CalcHeight(new GUIContent(instructionsText), EditorGUIUtility.labelWidth);
				requiredHeight /= EditorGUIUtility.pixelsPerPoint; // - 75 /* bug in Unity: CalcHeight returns incorrect values */;
				EditorGUILayout.SelectableLabel(instructionsText, EditorStyles.textArea, GUILayout.Height(requiredHeight));
				if( GUILayout.Button("Register (free)") )
				{
					F4URegistration.RegisterViaEmail( _invoiceNumber );
				}

				GUILayout.EndVertical();
			}
		}
	}
}