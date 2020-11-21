using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
	public static class WorkaroundUnityMissingEditorAPIs
	{
		public static void IndentGUILayout_Begin(this Editor editor)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(16f * (1 + EditorGUI.indentLevel));
			GUILayout.BeginVertical();

			// Now you can call GUILayout.Button() etc (Unity's broken core methods, unfixed for more than 5 years)
		}

		public static void IndentGUILayout_End(this Editor editor)
		{
			// ...after you've called GUILayout.Button() etc

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		
		public static void TileTextureAcrossRect(Texture2D image, Rect locationRect)
		{
			float w = image.width / EditorGUIUtility.pixelsPerPoint;
			float h = image.height / EditorGUIUtility.pixelsPerPoint;
			GUI.DrawTextureWithTexCoords(
				//new Rect(Vector2.zero, headerRect.size),
				locationRect,
				image,
				new Rect(0, 0, locationRect.width / w, locationRect.height / h)
			);
		}
		private static Dictionary<Color,Texture2D> _textureColouredCache = new Dictionary<Color, Texture2D>();
		public static Texture2D TextureColoured(Color c)
		{
			Texture2D texture;
			if( _textureColouredCache.TryGetValue(c, out texture) )
				return texture;
		
			texture = new Texture2D(1,1);
			texture.SetPixel(0,0,c);
			texture.Apply();
			_textureColouredCache[c] = texture;
		
			return texture;
		}
	}
}