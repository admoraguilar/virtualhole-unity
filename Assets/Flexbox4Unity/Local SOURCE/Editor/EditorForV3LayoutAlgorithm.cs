using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IFlexboxLayoutAlgorithmV3), true)]
public class EditorForV3LayoutAlgorithm : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		IFlexboxLayoutAlgorithmV3 value = target as IFlexboxLayoutAlgorithmV3;
		
		GUIStyle wrappingStyle = new GUIStyle( GUI.skin.label )
			{
				wordWrap = true
				};
		using( new GUILayout.VerticalScope("Features", "Box") )
		{
			GUILayout.Space(25f ); // Unity Box layout is broken doesn't add padding-top correctly
			if( value != null )
			foreach( var feature in value.featureDescription )
				EditorGUILayout.LabelField( "", feature, wrappingStyle);
		}
	}
}