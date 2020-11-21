#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IntelligentPluginVersioning;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Version = IntelligentPluginVersioning.Version;

namespace Flexbox4Unity
{
    public class F4UUpgrader2xTo3xDeleteObsoleteClasses : F4UUpgradeModule
    {
        public override Version minVersionToUpgrade { get { return new Version( 1, 0, 0 ); } }
        public override Version maxVersionToUpgrade { get { return new Version( 2, 9, 9 ); } }

        public override void PrepareToProcessUpgrades( out UpgradeResult result )
        {
            result = UpgradeResult.Success;
        }

        public override bool UpgradeProject( Version projectOldVersion, Version newVersion, List<string> allPrefabPaths, F4UUpgrader.UpgradeSettings settings )
        {
            List<string> classRelativeNames = new List<string>()
            {
                "Editor/F4UUpgrader1xTo2x.cs",
                "Editor/F4UUpgrader0x1xTo15.cs",
                "Editor/F4UFlexboxElementMenuItems.cs",
                "FlexboxLayoutAlgorithm23AttemptCSSSpecFailed.cs",
                "FlexboxElement.cs",
                "FlexboxLayoutGroup.cs",
                "FlexboxLayoutAlgorithm15.cs",
                //"Flexbox4Unity-UserGuide-v2.4.pdf"
            };
            /**
             * Find the victim classes relative to this class, and delete them
             */
            string thisName = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            
            //Debug.Log("Path to this class = "+thisName );
            
            var pathToFolderEditor = Path.GetDirectoryName(thisName);
            
            //Debug.Log("Path to Editor folder containing this class = "+pathToFolderEditor );

            var pathToFolderAllSource = Path.GetDirectoryName(pathToFolderEditor);
            
            //Debug.Log("Path to main source folder containing this class = "+pathToFolderAllSource );

            int filesDeleted = 0;
            foreach( var fileToDelete in classRelativeNames )
            {
                string fullPath = pathToFolderAllSource + Path.DirectorySeparatorChar + fileToDelete;
                
                if( File.Exists(fullPath) )
                {
                    Debug.Log( this.GetType().Name+": Deleting file at location: \"" + fullPath + "\"");
                    File.Delete(fullPath);
                    
                    string metaFullPath = fullPath + ".meta";
                    if( File.Exists( metaFullPath ) )
                        File.Delete(metaFullPath);
                    
                    filesDeleted++;
                }
            }

            return filesDeleted > 0;
        }

    }
}
#endif