using System.Collections;
using System.Collections.Generic;
using IntelligentPluginVersioning;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
	public class FlexboxProjectSettingsRegisterer
	{
#if UNITY_2018_3_OR_NEWER
	[SettingsProvider] // Unity's bizarre "emergent" registration-system for SettingsProvider classes
	public static SettingsProvider CreateAndAutomaticallyRegister()
	{
		var p = new SettingsProvider("Project/Flexbox4", SettingsScope.Project)
		{
			// By default the last token of the path is used as display name if no label is provided.
			label = "Flexbox",
			// Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
			guiHandler = (searchContext) =>
			{
				FlexboxProjectSettingsRenderer.RenderSettingsPanel();
			},
			// Populate the search keywords to enable smart search filtering and label highlighting:
			keywords = new HashSet<string>(new[] {"Flexbox", "GUI", "UI", "CSS"})
		};

		return p;
	}
#endif

	}
}