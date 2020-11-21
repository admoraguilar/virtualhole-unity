using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
	public class F4UWindowFixMultipleSettingsFiles : EditorWindow
	{
		private void OnGUI()
		{
			float windowWidth = position.width;
			
			string[] guids = AssetDatabase.FindAssets( "t:" + nameof(Flexbox4UnityProjectSettings) ); //FindAssets uses tags check documentation for more info
			if( guids.Length == 1 ) 
				Close();
			
			List<string> paths = new List<string>();
			for( int i = 0; i < guids.Length; i++ )
			{
				paths.Add( AssetDatabase.GUIDToAssetPath( guids[i] ) );
				//@return.Add( AssetDatabase.LoadAssetAtPath<Flexbox4UnityProjectSettings>( path ) );
			}
			
			using( new GUILayout.HorizontalScope() )
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label( "Flexbox: Upgrader", new GUIStyle() {fontSize = 30} );
				GUILayout.FlexibleSpace();
			}
			GUILayout.Space( 50 );

			GUILayout.Label( "Too many Settings files", new GUIStyle() { fontStyle = FontStyle.Bold } );
			GUILayout.Space( 5 );
			using( new GUILayout.HorizontalScope() )
			{
				GUILayout.Space( 20 );
				using( new GUILayout.VerticalScope() )
				{
					GUILayout.Label( "From version 3.1.3 onwards, Flexbox4Unity only allows one settings file per project. Please delete 1 or more files until there is only one left", new GUIStyle() {wordWrap = true} );
					GUILayout.Space( 20 );
					foreach( var p in paths )
					{
						//var asset = AssetDatabase.LoadAssetAtPath<Flexbox4UnityProjectSettings>( p );

						using( new GUILayout.HorizontalScope(GUI.skin.box) )
						{
							GUILayout.Space( 20 );
							//GUILayout.Label( asset.name, GUILayout.Width( windowWidth * 0.25f ) );
							//GUILayout.Space( 10 );
							GUILayout.Label( p ); // GUILayout.Width( windowWidth * 0.95f ) );
							GUILayout.Space( 20 );
							if( GUILayout.Button( "Delete" ) )
							{
								AssetDatabase.DeleteAsset( p );
								AssetDatabase.Refresh();
							}
						}
					}
				}
			}

		}
	}
}