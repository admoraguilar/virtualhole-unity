using System;
using System.Collections;
using System.Collections.Generic;
using IntelligentPluginVersioning;
using UnityEngine;
using Version = IntelligentPluginVersioning.Version;

namespace Flexbox4Unity
{
	public class Flexbox4UnityVersionHistory
	{
		public static VersionLog UpdateLog()
		{
			VersionLog log = new VersionLog();
			
			//
			log.AddVersion( new Version(3,3,0), new DateTime(2020,10,22) );
			log.AddVersion( new Version(3,2,1), new DateTime(2020,09,14) );
			log.AddVersion( new Version(3,2,0), new DateTime(2020,08,14) );
			log.AddVersion( new Version(3,1,2), new DateTime(2020,07,24) );
			log.AddVersion( new Version(3,1,0), new DateTime(2020,07,23) );
			log.AddVersion( new Version(3,0,0), new DateTime(2020,06,21) );
			log.AddVersion( new Version(2,4,0), new DateTime(2020,03,18) );
			log.AddVersion( new Version(2,3,2), new DateTime(2020,03,08) );
			log.AddVersion( new Version(2,3,1), new DateTime(2020,03,02) );
			log.AddVersion( new Version(2,3,0), new DateTime(2020,02,26) );
			log.AddVersion( new Version(2,2,0), new DateTime(2020,02,18) );
			log.AddVersion( new Version(2,1,0), new DateTime(2020,02,10) );
			log.AddVersion( new Version(2,0,0), new DateTime(2020,02,5) );
			log.AddVersion( new Version(1,5,2), new DateTime(2019,07,24) );
			log.AddVersion( new Version(1,5,0), new DateTime(2019,06,24) );
			log.AddVersion( new Version(1,4,7), new DateTime(2019,07,19) );
			log.AddVersion( new Version(1,4,4), new DateTime(2019,05,27) );
			log.AddVersion( new Version(1,4,2), new DateTime(2019,05,25) );
			log.AddVersion( new Version(1,4,0), new DateTime(2019,05,19) );
			log.AddVersion( new Version(1,3,0), new DateTime(2019,04,28) );
			log.AddVersion( new Version(1,1,0), new DateTime(2019,05,23) );

			log.Add(new Version(1, 2, 5), "Added support for basis = PERCENT", LogImportance.MAJOR);
			log.Add(new Version(1, 2, 6), "Improved editor for MARGINS");
			log.Add(new Version(1, 3, 0), "Added IntelligentPluginVersioner", LogImportance.MAJOR);
			log.Add(new Version(1, 3, 2), "Major performance optimizations.", LogImportance.MAJOR);
			log.Add(new Version(1, 3, 2), "Added support for PERCENT in many more places.", LogImportance.MAJOR);
			log.Add(new Version(1, 3, 2), "Fixed many edge-cases for cross-axis sizing.", LogImportance.MAJOR);
			log.Add(new Version(1, 3, 2), "Fixed some edge-cases for automatic (CONTENT) sizing", LogImportance.MAJOR);
			log.Add(new Version(1, 3, 3), "Added support for PERCENT to the Default Width and Height");
			log.Add(new Version(1, 3, 4), "Performance improvements for containers with size = CONTENT");
			log.Add(new Version(1, 4, 0), "Refactored some old structs, everything now uses CSS3WidthType internally (makes scripting cleaner/easier)");
			log.Add(new Version(1, 4, 1), "BUGFIX: old projects that were auto-upgraded generate errors when entering play-mode.");
			log.Add(new Version(1, 4, 1), "BUGFIX: the new first-time-install popup appears every time I open the project");
			log.Add(new Version(1, 4, 2), "Added ability to manually re-trigger the auto-upgrade process with right-click on Flex components in Editor");
			log.Add(new Version(1, 4, 3), "Added shortcuts for creating a new object with Flex item or Flex container directly from right-click menu in Hierarchy window");
			log.Add(new Version(1, 4, 4), "Added support for padding to all flex-containers", LogImportance.MAJOR);
			log.Add(new Version(1, 4, 5), "Added support for minWidth + maxWidth and minHeight + maxHeight");
			log.Add(new Version(1, 4, 6), "Fixed CONTENT sizing for Unity.UIText objects that use BestFit.", LogImportance.MAJOR);
			log.Add(new Version(1, 4, 6), "Added auto-updating contents of old-version Nested Prefabs when they're opened and require upgrades");
			log.Add(new Version(1, 4, 7), "Fixed some unusual bugs with large margins");
			log.Add(new Version(1, 4, 7), "Simplified margins and padding APIs");
			log.Add(new Version(1, 4, 7), "Fixed some bugs with min/max width and height");
			log.Add(new Version(1, 5, 0), "Replaced UnityUI's slow layout algorithm with 4x faster layout system.", LogImportance.MAJOR);
			log.Add(new Version(1, 5, 0), "Improved integration with Unity 2018.4");
			log.Add(new Version(2, 0, 0), "Added support for Unity 2020/2019");
			log.Add(new Version(2, 0, 0), "Updated Auto-Refresh to support latest Unity API changes", LogImportance.MAJOR);
			log.Add(new Version(2, 0, 0), "Added live-preview mode in-Scene", LogImportance.MAJOR);
			log.Add(new Version(2, 1, 0), "Converted to new modular layout-algorithm", LogImportance.MAJOR);
			log.Add(new Version(2, 1, 0), "Converted all classes to CSS3 official names: FlexItem and FlexContainer", LogImportance.MAJOR);
			log.Add(new Version(2, 2, 0), "Greatly improved the Inspector window for FlexItem and FlexContainer");
			log.Add(new Version(2, 2, 0), "Added Help tab and “Advanced” inspector for direct access to all flex params");
			log.Add(new Version(2, 3, 0), "Added full support for CONTENT-BOX and BORDER-BOX from CSS core", LogImportance.MAJOR);
			log.Add(new Version(2, 3, 0), "Updated layout algorithm (new version: 23) to handle all kinds of CSS3-box", LogImportance.MAJOR);
			log.Add(new Version(2, 3, 0), "Added auto-upgrade of layout algorithm if newer version detected than current when processing an upgrade");
			log.Add(new Version(2, 3, 0), "Added full support for AUTO", LogImportance.MAJOR);
			log.Add(new Version(2, 3, 0), "Re-implemented Padding and Margins, now works in all cases and nested cases", LogImportance.MAJOR);
			log.Add(new Version(2, 3, 0), "Added automatic layout of all built-in Unity UI elements");

			log.Add(new Version(2, 3, 1), "(VR) Added: support for Preview-mode on Worldspace canvases", LogImportance.MAJOR);
			log.Add(new Version(2, 3, 1), "Added: Auto-expand child UnityUI items to fit their parent FlexItem");
			log.Add(new Version(2, 3, 1), "Added: auto-sizing for UnityUI.RawImage");
			
			log.Add(new Version(2, 3, 2), "Fixed: flexOrder gave wrong ordering in some cases");
			log.Add(new Version(2, 3, 2), "Fixed: player-builds weren't loading their settings, and couldn't relayout after launch", LogImportance.MAJOR);
			
			log.Add(new Version(2, 4, 0), "Experimental: flex-wrap support", LogImportance.MAJOR);
			
			log.Add(new Version(3, 0, 0), "FIXED: When overflowing the Canvas, Justify was treated as Justify-start in all cases, (only happened when items were clamped)");
			log.Add(new Version(3, 0, 0), "FIXED: Changes in ADVANCED tab are now instantaneous in Editor");
			log.Add(new Version(3, 0, 0), "FIXED: Children of an Unstretched container were getting zero-size if had a default width or height");
			log.Add(new Version(3, 0, 0), "Right-click in Hierarchy and select Flexbox > Output as HTML to generate HTML code you can test in Chrome or experiment with", LogImportance.MAJOR);
			log.Add(new Version(3, 0, 0), "ADDED: UnityUI.RawImage: now autolayouted too");
			log.Add(new Version(3, 0, 0), "FIXED: auto-created settings files (if you deleted yours) were given a misleading filename");
			log.Add(new Version(3, 0, 0), "By default, objects no-longer re-run layout each time you select them in Editor - you can re-enable old behaviour using the ADVANCED tab", LogImportance.MAJOR);
			log.Add(new Version(3, 0, 0), "Added some useful API calls: CreateFlexContainerInParent() / CreateFlexItemInParent()", LogImportance.MAJOR);
			log.Add(new Version(3, 0, 0), "FIXED: F4U settings file had wrong hashcode");
			log.Add(new Version(3, 0, 0), "Added some new features as 'Flextensions' (Right click in Hierarchy, select Flexbox > Flextensions)", LogImportance.MAJOR);
			
			log.Add(new Version(3, 1, 0), "Improved the HTML-exporter to auto-detect child Images used as backgrounds on FlexContainers" );
			log.Add(new Version(3, 1, 0), "FIXED: responsive-design layouts will now work with ROW+WRAP embedded inside a Vertical Scrollview", LogImportance.MAJOR);
			log.Add(new Version(3, 1, 0), "FIXED: grow/shrink was getting overwritten if some siblings were frozen (grow/shrink == 0)");
			log.Add(new Version(3, 1, 0), "Layout Algorithms now have self-describing features (click one in Project view to see it)", LogImportance.MAJOR);
			
			log.Add(new Version(3, 1, 2), "New high-performance Layout Algorithm (3.1.2) including Flex-Wrap", LogImportance.MAJOR);
			log.Add(new Version(3, 1, 2), "New optional 'freeze layout' feature for Procedural GUI generation (increases performance 10x)", LogImportance.MAJOR);
			log.Add(new Version(3, 1, 2), "Improved built-in handling for TextMeshPro", LogImportance.MAJOR);
			
			log.Add(new Version(3, 1, 3), "FIXED: In Unity 2019 and 2020, TextMeshPro support was disabled (Unity 2019 bugs make it non-forwards-compatible)", LogImportance.MAJOR);
			log.Add( new Version( 3,1,3), "ADDED: several Flextensions and Flextemplates (right click in Hierarchy view, they're in the Flexbox submenus)" );
			
			log.Add( new Version( 3,2,0), "FIXED: Auto-sizing of Unity InputTextField was missing 3 pixels from Unity's undocumented internal padding"  );
			log.Add( new Version( 3, 2, 0 ), "ADDED: Internal cache for TextMeshPro's slow 'size' queries to greatly speed-up projects that use TMP", LogImportance.MAJOR );
			
			log.Add( new Version( 3, 2, 0 ), "Complete rewrite of Settings system: fixed 2 x building, fixed popup-spam", LogImportance.MAJOR );
			
			// Unreleased:

			log.AddVersion( new Version(3,2,1), new DateTime(2020,09,01) );
			log.Add( new Version( 3, 2, 1 ), "Auto-fix tool for new Settings system. Find it in menu: Tools > Flexbox > Auto-discover RuntimeSettings", LogImportance.MAJOR );
			log.Add( new Version( 3, 2, 1 ), "Added workaround for \"Type cannot be found\" bug in Unity v2020.1", LogImportance.MAJOR );
			log.Add( new Version( 3, 2, 1 ), "Removed errors on console when Inspector was active during playmode end" );
			log.Add( new Version( 3, 2, 1 ), "Auto-upgrade of all pre-v3.2.1 versions, detecting and fixing all prefabs", LogImportance.MAJOR );
			log.Add( new Version( 3, 2, 1 ), "(optional) auto-upgrade of all scenes when you open them", LogImportance.MAJOR );
			log.Add( new Version( 3, 2, 1 ), "(optional) auto-upgrade of all prefabs when you open them", LogImportance.MAJOR );
			log.Add( new Version( 3, 2, 1 ), "Manual-upgrader (in Tools>Flexbox>Upgrades menu) gives popup telling you what it's upgraded" );
			log.Add( new Version( 3, 2, 1 ), "Added Error to detect when you create a FlexContainer/FlexItem without a RectTransform", LogImportance.MAJOR );

			log.Add(new Version(3, 3, 0), "FIXED: build bug with runtime builds causing them not to compile");
			log.Add(new Version(3, 3, 0), "WORKAROUND: TextMeshPro has a bug which caused a crash, new workaround added to avoid that");
			log.Add(new Version(3, 3, 0), "FIXED: bug in UnityEditor 2020.1.x (fixed by Unity in 2020.2) preventing you from assigning some Algorithms in Settings");
			log.Add(new Version(3, 3, 0), "FIXED: creating a new GUI in a new project for the first time would produce error messages until you closed and reopened the scene");

			/*
			 Not included in build yet:
			 			 
			 			 Added: API methods: CreateFlexContainer, CreateFlexContainerAndFlexItem, CreateFlexItem			
			*/
			return log;
		}
	}
}