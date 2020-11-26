using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
    public static class EditorProjectSettings
    {
        public static List<Flexbox4UnityProjectSettings> findAllPossibleProjectSettings
        {
            get
            {
                string[] guids = AssetDatabase.FindAssets( "t:" + nameof(Flexbox4UnityProjectSettings) ); //FindAssets uses tags check documentation for more info

                List<Flexbox4UnityProjectSettings> @return = new List<Flexbox4UnityProjectSettings>();
                for( int i = 0; i < guids.Length; i++ )
                {
                    string path = AssetDatabase.GUIDToAssetPath( guids[i] );
                    @return.Add( AssetDatabase.LoadAssetAtPath<Flexbox4UnityProjectSettings>( path ) );
                }

                return @return;
            }
        }
        
        public static Flexbox4UnityProjectSettings findProjectSettingsDontThrowExceptions
        {
            get
            {
                var allFound = findAllPossibleProjectSettings;

                if( allFound.Count > 1 )
                {
                    F4UUpgrader312DetectDoubleSettingsFiles.LaunchWindow();
                    //throw new Exception( "Multiple Flexbox settings objects found in project - Unity does not support this, please delete one or more. Unity's names: " + string.Join( ";", allFound.Select( settings => settings.name ) ) );
                }

                if( allFound.Count < 1 )
                {
                    Debug.LogError( "No Flexbox settings found in project" );
                    return null;
                }

                return allFound[0];
            }
        }
        
        public static Flexbox4UnityProjectSettings findAnyProjectSettings
        {
            get
            {
                var allFound = findAllPossibleProjectSettings;

                if( allFound.Count >= 1 )
                    return allFound[0];
                else
                    return null;
            }
        }

        public static Flexbox4UnityProjectSettings requireProjectSettings
        {
            get
            {
                Flexbox4UnityProjectSettings settings = findProjectSettingsDontThrowExceptions;

                if( settings == null )
                    throw new Exception( "Flexbox: Missing project-settings file (did you delete it?)" );
                else
                    return settings;
            }
        }
    }
}