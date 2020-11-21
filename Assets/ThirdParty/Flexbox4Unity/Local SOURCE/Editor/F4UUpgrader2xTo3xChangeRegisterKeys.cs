#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Version = IntelligentPluginVersioning.Version;

namespace Flexbox4Unity
{
    public class F4UUpgrader2xTo3xChangeRegisterKeys : F4UUpgradeModule
    {
        public override Version minVersionToUpgrade { get { return new Version( 2, 2, 0 ); } }
        public override Version maxVersionToUpgrade { get { return new Version( 2, 9, 9 ); } }

        public override void PrepareToProcessUpgrades( out UpgradeResult result )
        {
            result = UpgradeResult.Success;
        }

        public override bool UpgradeProject( Version projectOldVersion, Version newVersion, List<string> allPrefabPaths, F4UUpgrader.UpgradeSettings settings )
        {
            var OldPrefix = "com.intelligentunity3d.flexbox4unity";
            
            if( EditorPrefs.GetBool(OldPrefix + "." + "IsRegistered") )
                {
                    var invoiceNumber = EditorPrefs.GetString(OldPrefix + "." + "Flexbox4UnityInvoiceNumber");
                    
                    EditorPrefs.SetBool( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix+ "." + "IsRegistered", true);
                    EditorPrefs.SetString( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix+ "." + "Flexbox4UnityInvoiceNumber", invoiceNumber);
                    
                    EditorPrefs.DeleteKey( OldPrefix + "." + "IsRegistered");
                    EditorPrefs.DeleteKey( OldPrefix + "." + "Flexbox4UnityInvoiceNumber" );

                    return true;
                }
            else
                return false;
        }

    }
}
#endif