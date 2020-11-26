using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
    public class F4UUpgraderManualCorrectMissingRuntimeSettings : MonoBehaviour
    {
        [MenuItem( "Tools/Flexbox/Auto-discover all RuntimeSettings" )]
        public static void ToolsFlexboxAutodiscoverRuntimeSettings()
        {
            var allContainers = FindObjectsOfType<FlexContainer>();
            var allItems = FindObjectsOfType<FlexItem>();
            
            /** Check that the auto-detect project-settings code can find a settings file it's happy with */
            var globalProjectSettings = Flexbox4UnityProjectSettings.findProjectSettings;

            if( globalProjectSettings == null )
            {
                EditorUtility.DisplayDialog( "Flexbox error", "Could not auto-detect the Flexbox settings file in your project; please check the console log, you may need to create a settings file", "OK" );
            }
            else
            {
                /**
                 * ... an autodetected settings file exists, so we can now trigger each FlexContainer/Item to auto-assign
                 */
                int numMissingContainerSettings = 0;
                int numMissingItemSettings = 0;
                
                foreach( var c in allContainers )
                {
                    if( !c.hasSettings )
                    {
                        numMissingContainerSettings++;
                        var forceLoad = c.settings;
                    }
                }

                foreach( var c in allItems )
                {
                    if( !c.hasSettings )
                    {
                        numMissingContainerSettings++;
                        var forceLoad = c.settings;
                    }
                }

                EditorUtility.DisplayDialog( "Flexbox Re-check tool", "Checked: " + allContainers.Length + " containers, " + allItems.Length + " items.\n\nMissing: " + numMissingContainerSettings + " container-settings, " + numMissingItemSettings + " item-settings.\n\nTotal fixed/loaded: " + (numMissingContainerSettings + numMissingItemSettings), "OK" );
            }
        }
    }
}