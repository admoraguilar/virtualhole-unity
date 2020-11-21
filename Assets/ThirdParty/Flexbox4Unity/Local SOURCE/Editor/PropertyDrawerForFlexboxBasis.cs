#if UNITY_EDITOR // Workaround bugs in the UnityEditor build command (sometimes ignores the fact this file is inside an Editor folder!)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
	[CustomPropertyDrawer(typeof(FlexboxBasis))]
	public class PropertyDrawerForFlexboxBasis : PropertyDrawer
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
			int[] possibleModeValues = (int[]) Enum.GetValues(typeof(FlexBasis));
			FlexBasis mode = (FlexBasis) possibleModeValues[propMode.enumValueIndex];

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
			int available = (int) position.width;
			bool showValue = false;
			int maxModeWidth = 0;
			int minValueWidth = 0;
			switch( mode )
			{
				case FlexBasis.LENGTH: // this ugliness to workaround Unity's bad design of SerializedProperty
					maxModeWidth = 70;
					minValueWidth = 35;
					showValue = true;
					break;

				case FlexBasis.PERCENT: // this ugliness to workaround Unity's bad design of SerializedProperty
					maxModeWidth = 90;
					minValueWidth = 25;
					showValue = true;
					break;

#if LITE_VERSION
#else
				case FlexBasis.ASPECT_FIT: // this ugliness to workaround Unity's bad design of SerializedProperty
					maxModeWidth = 120;
					minValueWidth = 22;
					showValue = true;
					break;
#endif

				case FlexBasis.CONTENT: // this ugliness to workaround Unity's bad design of SerializedProperty
				case FlexBasis.AUTO: // this ugliness to workaround Unity's bad design of SerializedProperty
					showValue = false;
					break;
			}

			int modeWidth = showValue ? Math.Min(available - (minValueWidth + spacerWidth), maxModeWidth) : available;
			int valueWidth = showValue ? Math.Max(minValueWidth, (int) position.width - (modeWidth + spacerWidth)) : 0;

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