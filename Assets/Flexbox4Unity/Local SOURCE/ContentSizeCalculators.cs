#define OPTIMIZATION_TMP_ALLOW_ONE_FRAME_DELAY_ON_TEXT_CHANGE
#define OPTIMIZATION_CACHE_CALLS_TO_TMP_TEXT_SIZE
//#define INVALIDATE_CACHE_EACH_FRAME

using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEngine;
using UnityEngine.UI;

#if AVAILABLE_TEXTMESHPRO
using TMPro;
#endif

namespace Flexbox4Unity
{
	public class ContentSizeCalculators
	{
		public static Vector2 ContentSizeFor(Text uiText, bool showDebugMessages = false)
		{
			/**
			 * There are MAJOR BUGS in Unity's TextGenerator.cs - the values returned here are often incorrect for all except the most
			 * basic cases.
			 */
			return new Vector2(uiText.preferredWidth, uiText.preferredHeight);
		}
		
#if OPTIMIZATION_CACHE_CALLS_TO_TMP_TEXT_SIZE
		private class TMP_DATA
		{
			public string text;
			public float fontSize;
			public TMP_FontAsset fontPointer;
			public float fontScale;
			public TMP_TextInfo tmpInternalData;
			#if INVALIDATE_CACHE_EACH_FRAME
			public float lastCalcedInFrame;
			#endif
			public Vector2 parentSize;

			public static TMP_DATA NewFrom( TMP_Text t )
			{
				TMP_DATA @return = new TMP_DATA();
				@return.text = t.text;
				@return.fontPointer = t.font;
				@return.fontSize = t.fontSize;
				@return.fontScale = t.fontScale;
				@return.tmpInternalData = t.textInfo;
				@return.parentSize = (t.rectTransform.parent as RectTransform).rect.size;
				
				#if INVALIDATE_CACHE_EACH_FRAME
				@return.lastCalcedInFrame = Time.time;
				#endif
				return @return;
			}

			public override bool Equals( object obj )
			{
				return Equals( obj as TMP_DATA );
			}

			public bool Equals( TMP_DATA other )
			{
				return fontPointer == other.fontPointer
				       && fontSize == other.fontSize
				       && fontScale == other.fontScale
				       && tmpInternalData == other.tmpInternalData
				       && parentSize == other.parentSize
				       #if INVALIDATE_CACHE_EACH_FRAME
				       && lastCalcedInFrame == other.lastCalcedInFrame
				       #endif
				       && text == other.text;
			}

			public override int GetHashCode()
			{
				return text.GetHashCode() + (int)(fontSize * 10000 + fontScale * 100
				                                                   #if INVALIDATE_CACHE_EACH_FRAME
				                                                   + lastCalcedInFrame
				                                                   #endif
				                                                   )
				                                                   + parentSize.GetHashCode()
				                                                   + fontPointer.GetHashCode()
				                                                   + (tmpInternalData == null ? 0 : tmpInternalData.GetHashCode());
			}

			public override string ToString()
			{
				return "["+fontPointer+": "+fontSize+"["+fontScale+"] x "+text.Substring(0,Math.Min(text.Length, 10))+"]";
			}
		}
		#endif

#if OPTIMIZATION_CACHE_CALLS_TO_TMP_TEXT_SIZE
		private static Dictionary<TMP_DATA, Vector2> _TextMeshPro_Bugs_Cache;
		/// <summary>
		/// Any amount is fine, this just triggers a warning that you need to start purging the cache - set it to a number
		/// higher than the total number of unique sized/shaped/text-value TMP objects you expect to get in the lifetime of
		/// your game
		/// </summary>
		private static int MAX_EXPECTED_TMP_CACHE_ENTRIES = 1000;
		public static void PurgeTextMeshProCache()
		{
			_TextMeshPro_Bugs_Cache?.Clear();
		}
#endif
	
#if AVAILABLE_TEXTMESHPRO
		public static Vector2 ContentSizeFor(TMP_Text uiText, bool showDebugMessages = false)
		{
		#if OPTIMIZATION_CACHE_CALLS_TO_TMP_TEXT_SIZE
			if( _TextMeshPro_Bugs_Cache == null )
				_TextMeshPro_Bugs_Cache = new Dictionary<TMP_DATA, Vector2>();
			
			var data = TMP_DATA.NewFrom( uiText );
			if( _TextMeshPro_Bugs_Cache.ContainsKey( data ) )
			{
				//Debug.Log( "Matched: "+data+" with existing key that had value = "+_TextMeshPro_Bugs_Cache[data] );
				return _TextMeshPro_Bugs_Cache[data];
			}
		#endif
		
		#if OPTIMIZATION_TMP_ALLOW_ONE_FRAME_DELAY_ON_TEXT_CHANGE
		#else
			/**
			 * TMP is badly designed and cannot return correct values in the given frame if - undetectable, apparently! - any code
			 * altered the text earlier during this frame.
			 *
			 * Officially the workaround (rather than fix it) is to call the FORCE method ... and sadly we have to do that every frame, many
			 * times per frame, because of TMP's bad design. Note: this is not documented, but the TMP author wrote it in a
			 * Unity forums post.
			 */
			uiText.ForceMeshUpdate();
		#endif

			
     /**
      * Note: TextMeshPro (undocumented!) returns different values for size depending on whether you ask
      * it for its "real" size or for its "fake UnityUI" size. This may only happen if you didn't call ForceMeshUpdate?
      */
     var @return = ( uiText is ILayoutElement )
      ? new Vector2( (uiText as ILayoutElement).preferredWidth, (uiText as ILayoutElement).preferredHeight )
      : new Vector2( uiText.textBounds.size.x, uiText.textBounds.size.y );

     #if OPTIMIZATION_CACHE_CALLS_TO_TMP_TEXT_SIZE
			if( _TextMeshPro_Bugs_Cache.Count > MAX_EXPECTED_TMP_CACHE_ENTRIES )
				Debug.LogWarning( "WARNING! Internal cache of TMP's slow methods has exceeded expected capacity ("+MAX_EXPECTED_TMP_CACHE_ENTRIES+") - recommend you PURGE the cache (or find a better version of TMP that doesn't need to be cached ;) )" );
			
     //Debug.Log( "Storing: "+data+" = "+@return );
     _TextMeshPro_Bugs_Cache[data] = @return;
     #endif
			
			return @return;
     //return new Vector2(uiText.textBounds.size.x, uiText.textBounds.size.y);
		}
	#endif
		
		private static Vector2 _ContentSizeForTextPaddedElement(Text childText,  float minWidth, float minHeight, float padLeft, float padRight, float padTop, float padBottom)
		{
			Vector2 textSize = ContentSizeFor(childText );

			/**
			 * Unity reserves a very small amount of padding on each Button and doesn't display text outside that area,
			 * so this method allows you to specify corrective-padding to workaround this undocumented problem in UnityUI
			 */
			float w = Mathf.Max(minWidth, textSize.x + (padLeft + padRight));
			float h = Mathf.Max(minHeight, textSize.y + (padTop + padBottom));

			return new Vector2(w, h);
		}
		
		public static Vector2 ContentSizeFor( Image uiImage )
		{
			if( uiImage.sprite != null )
			{
				Vector2 intrinsicSize = uiImage.sprite.rect.size;

				return intrinsicSize;
			}
			else
			{
				return new Vector2(10,10); // this is Unity's own default
			}
		}
		
		public static Vector2 ContentSizeFor( RawImage uiRawImage )
		{
			if( uiRawImage.texture != null )
			{
				Vector2 intrinsicSize = new Vector2(uiRawImage.texture.width, uiRawImage.texture.height);

				return intrinsicSize;
			}
			else
			{
				return new Vector2(10,10); // this is Unity's own default
			}
		}

		public static Vector2 ContentSizeFor( Button uiButton, bool showDebugMessages = false)
		{
			Text buttonText = uiButton.GetComponentInChildren<Text>();
			/**
			 * Unity reserves a very small amount of padding on each Button and doesn't display text outside that area
			 *
			 * Undocumented, but experimentally: it's 3 pixels on each side
			 */
			if( buttonText != null )
				return _ContentSizeForTextPaddedElement(buttonText, 160, 30, 3, 3, 3, 3);
			else
				return new Vector2(160, 30 ); // Unity's hardcoded defaults for UIButton
		}

		public static Vector2 ContentSizeFor(Toggle uiToggle )
		{
			// TODO: we should ALSO calculate the Toggle's label-text-size and add it on to the width here (but not the height)
			return new Vector2(20, 20); /** Unity hardcodes Toggles to always be 20x20 pixels */
		}
		
		public static Vector2 ContentSizeFor( InputField uiInputField )
		{
			Text buttonText = uiInputField.textComponent;
			/**
			 * Unity reserves a very small amount of padding on each InputField and doesn't display text outside that area
			 *
			 * Undocumented, but experimentally in UnityEditor: it's 10 pixels on each side, and 7 pixels on top, 6 on bottom
			 */
			if( buttonText != null )
				return _ContentSizeForTextPaddedElement(buttonText, 160, 30, 10, 10, 7, 6);
			else
				return new Vector2(160, 30 ); // Unity's hardcoded defaults for UIButton
		}
	}
}