#if UNITY_EDITOR // Workaround bugs in the UnityEditor build command (sometimes ignores the fact this file is inside an Editor folder!)
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IntelligentPluginTools
{
	  public class Div : GUI.Scope
	  {
		  private bool _isCentered; 
		  public Rect rectCreated;
	    
	    public Div( bool center, params GUILayoutOption[] options)
	    {
		    _isCentered = center;
		    if( _isCentered )
		    {
			    rectCreated = EditorGUILayout.BeginHorizontal();
			    GUILayout.FlexibleSpace();
		    }

		    GUILayout.BeginVertical(options);
	    }
	    
      public Div(params GUILayoutOption[] options) : this(false, options)
      {
      }

      public Div(GUIStyle style, params GUILayoutOption[] options)
      {
        GUILayout.BeginVertical(style, options);
      }
      
      protected override void CloseScope()
      {
        GUILayout.EndVertical();

        if( _isCentered )
        {
	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();
        }
      }
    }
	  
	  public class DivHorizontal : GUI.Scope
	  {
		  private bool _isCentered; 
		  public Rect rectCreated;
		  
		  public DivHorizontal( params GUILayoutOption[] options) : this(false, options)
		  {
		  }
		  public DivHorizontal( bool center, params GUILayoutOption[] options)
		  {
			  _isCentered = center;
			  if( _isCentered )
			  {
				  rectCreated = EditorGUILayout.BeginHorizontal();
				  GUILayout.FlexibleSpace();
			  }

			  GUILayout.BeginHorizontal(options);
		  }
	    
		  protected override void CloseScope()
		  {
			  GUILayout.EndHorizontal();
			  if( _isCentered )
			  {
				  GUILayout.FlexibleSpace();
				  EditorGUILayout.EndHorizontal();
			  }
		  }
	  }

/**
	 * Version 2019.1
	 */
	public class ESimpleHTML
	{
		public static GUIStyle sHtml, sH1, sH2, sPara;

		static ESimpleHTML()
		{
			sHtml = new GUIStyle(GUI.skin.label);
			sHtml.richText = true;

			sH1 = new GUIStyle(sHtml);
			sH1.fontSize = 24;

			sH2 = new GUIStyle(sHtml);
			sH2.fontSize = 18;

			sPara = new GUIStyle(sHtml);
			sPara.wordWrap = true;
			sPara.fontSize = 12;
		}

		public static Rect Div()
		{
			return EditorGUILayout.BeginVertical();
		}

		public static Rect Div(params GUILayoutOption[] options)
		{
			return EditorGUILayout.BeginVertical(options);
		}
		
		public static void DivEnd()
		{
			EditorGUILayout.EndVertical();
		}

		public static Rect DivCentered()
		{
			var rectCreated = EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			Div();
			return rectCreated;
		}
		
		public static void DivCentered( params GUILayoutOption[] options)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			Div( options );
		}
		
		public static void DivCenteredEnd()
		{
			DivEnd();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		public static void RawText(string text, GUIStyle gsOverride = null)
		{
			GUILayout.Label(text, gsOverride == null ? sPara : gsOverride );
		}

		public static void H1(string text, bool centered = false)
		{
			H1(text, null, centered );
		}
		public static void H1(string text, GUIStyle gsOverride, bool centered = false)
		{
			GUILayout.Space(5);
			Div();
			if( centered )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
			}

			GUILayout.Label(text, gsOverride == null ? sH1 : gsOverride );
			if( centered )
			{
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}

			DivEnd();
			GUILayout.Space(10);
		}

		public static void H2(string text, bool centered = false)
		{
			H2(text, null, centered);
		}
		public static void H2(string text, Color overrideColour, bool centered = false)
		{
			H2(text, new GUIStyle(sH2) { normal = new GUIStyleState() { textColor = overrideColour } }, centered );
		}
		public static void H2(string text, GUIStyle gsOverride, bool centered = false )
		{
			Div();
			GUILayout.Space(10);
			
			if( centered )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
			}
			GUILayout.Label(text, gsOverride == null ? sH2 : gsOverride);
			if( centered )
			{
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
			
			DivEnd();
			GUILayout.Space(10);
		}

		public static void P(string text, bool centered = false)
		{
			P(text, null, centered );
		}
		public static void P(string text, GUILayoutOption lo, bool centered = false )
		{
			P(text, 15, lo, centered );
		}

		public static void P(string text, int indent, GUILayoutOption lo, bool centered = false )
		{
			P( text, indent, null, lo, centered );
		}

		public static void P(string text, Color overrideColour, bool centered = false)
		{
			P(text,15,overrideColour,centered);
		}
		public static void P(string text, int indent, Color overrideColour, bool centered = false)
		{
			P(text, indent, new GUIStyle(sPara) {normal = new GUIStyleState() {textColor = overrideColour}}, null, centered);
		}
		public static void P(string text, int indent, Color overrideColour, GUILayoutOption lo, bool centered = false)
		{
			P(text, indent, new GUIStyle(sPara) {normal = new GUIStyleState() {textColor = overrideColour}}, lo, centered);
		}
		
		public static void P(string text, int indent, GUIStyle gsOverride, GUILayoutOption lo, bool centered = false )
		{
			using( new Div() )
			{
				GUILayout.Space(2);
				if( lo != null )
					GUILayout.BeginHorizontal(lo);
				else
					GUILayout.BeginHorizontal();

				if( centered )
				{
					GUILayout.FlexibleSpace();
				}
				else
					GUILayout.Space(indent);

				GUILayout.Label(text, gsOverride == null ? sPara : gsOverride);
				if( centered )
				{
					GUILayout.FlexibleSpace();
				}

				GUILayout.EndHorizontal();
			}
		}
		
		/** Separate method because of bugs in Unity (all versions) that SelectableLabel is incompatible with Label and
		 * actually uses a different rendering path with different visual and layout behaviour, that messes-up layouts.
		 */
		public static void P_Selectable(string text, GUIStyle gsOverride, GUILayoutOption lo, bool centered = false, int indent = 15 )
		{
			using( new Div() )
			{
				GUILayout.Space(2);
				if( lo != null )
					GUILayout.BeginHorizontal(lo);
				else
					GUILayout.BeginHorizontal();

				if( centered )
				{
					GUILayout.FlexibleSpace();
				}
				else
					GUILayout.Space(indent);

				EditorGUILayout.SelectableLabel(text, gsOverride == null ? sPara : gsOverride);
				if( centered )
				{
					GUILayout.FlexibleSpace();
				}

				GUILayout.EndHorizontal();
			}
		}

		public enum SpanAlignment
		{
			LEFT,
			CENTER,
			RIGHT,
			JUSTIFY
		}
		public static void SPAN(string[] texts, int indent, SpanAlignment alignment, GUILayoutOption lo)
		{
			GUILayout.Space(2);
			if( lo != null )
				GUILayout.BeginHorizontal(lo);
			else
				GUILayout.BeginHorizontal();
			GUILayout.Space(indent);

			for( int i = 0; i < texts.Length; i++ )
			{
				switch( alignment )
				{
					case SpanAlignment.CENTER:
						if( i == 0 )
						GUILayout.FlexibleSpace();
						break;
					
					case SpanAlignment.RIGHT:
						if( i == 0 )
							GUILayout.FlexibleSpace();
						break;
					
					case SpanAlignment.JUSTIFY:
						if( i > 0 )
						GUILayout.FlexibleSpace();
						break;
				}
					
				GUILayout.Label(texts[i], sPara);
				
				switch( alignment )
				{
					case SpanAlignment.CENTER:
						if( i == texts.Length-1 )
							GUILayout.FlexibleSpace();
						break;
					
					case SpanAlignment.LEFT:
						if( i == texts.Length-1 )
							GUILayout.FlexibleSpace();
						break;
				}
			}

			GUILayout.EndHorizontal();
		}

		public void a()
		{
			GUIStyle sSizedBox = new GUIStyle(GUI.skin.box);
			sSizedBox.fixedWidth = 100.0f;
			sSizedBox.fixedHeight = 50f;
			GUILayout.Box("", sSizedBox);
			GUILayout.Box("labelled", sSizedBox);
		}
	}
}
#endif