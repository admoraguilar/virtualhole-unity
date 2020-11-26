#if UNITY_EDITOR // Workaround bugs in the UnityEditor build command (sometimes ignores the fact this file is inside an Editor folder!)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
	[CustomPropertyDrawer(typeof(CSS3Length))]
	public class PropertyDrawerForCSS3Width : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			SerializedProperty propMode = property.FindPropertyRelative("mode");

			/** Sadly, UnityEditor core API bug: it doesn't expose the VALUE of the enum, only the INDEX WITHIN THE INTERNALS,
			 * which no-one should be using - and Unity doesn't allow you to see the actual VALUE (which is usually what you need).
			 *
			 * So we have to use C# methods to reverse-grab the possible values, and later infer what the mode-enum's value is from
			 * that.
			 */
			int[] possibleModeValues = (int[]) Enum.GetValues(typeof(CSS3LengthType));
			CSS3LengthType mode = (CSS3LengthType) possibleModeValues[propMode.enumValueIndex];

			if( label != GUIContent.none )
			{
				// Draw label
				position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			}

			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			int spacerWidth = 5;
			int modeWidth;
			int valueWidth;
			int available = (int) position.width;
			bool showValue;
			int maxModeWidth = 0;
			int minValueWidth = 0;
			switch( mode )
			{
				case CSS3LengthType.PIXELS: // this ugliness to workaround Unity's bad design of SerializedProperty
					maxModeWidth = 70;
					minValueWidth = 35;
					showValue = true;
					break;

				case CSS3LengthType.PERCENT: // this ugliness to workaround Unity's bad design of SerializedProperty
					maxModeWidth = 90;
					minValueWidth = 25;
					showValue = true;
					break;

				case CSS3LengthType.NONE: // this ugliness to workaround Unity's bad design of SerializedProperty
					showValue = false;
					break;

				default:
					Debug.LogError("Error, impossible value = " + propMode.enumValueIndex);
					showValue = false;
					break;
			}

			modeWidth = showValue ? Math.Min(available - (minValueWidth + spacerWidth), maxModeWidth) : available;
			valueWidth = showValue ? Math.Max(minValueWidth, (int) position.width - (modeWidth + spacerWidth)) : 0;

			var valueRect = new Rect(position.x, position.y, valueWidth, position.height);
			var modeRect = new Rect(position.x + valueWidth + (showValue ? spacerWidth : 0), position.y, modeWidth, position.height);

			// Draw fields - pass GUIContent.none to each so they are drawn without labels
			if( showValue )
				EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);
			EditorGUI.PropertyField(modeRect, property.FindPropertyRelative("mode"), GUIContent.none);

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}
#endif