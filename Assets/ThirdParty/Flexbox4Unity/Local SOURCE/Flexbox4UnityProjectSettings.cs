#define UNITY_HAS_FIXED_THE_NESTED_PREFABS_FINDOBJECTSOFTYPE_BUG // Not fixed in 2018. Maybe they'll fix it in 2019?
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IntelligentPluginVersioning;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Version = IntelligentPluginVersioning.Version;

namespace Flexbox4Unity
{
	public enum GizmosRenderMode
	{
		WIREFRAME_CORNERS = 0,
		NONE = 1
	}

	public class Flexbox4UnityProjectSettings : ScriptableObject
	{
		public bool debugAutoUpgrades = false;

		#region Compile-time managed settings
		public static Type latestOfficialLayoutAlgorithm = typeof(FlexboxLayoutAlgorithm312);
		public static Version builtVersion = new Version(3, 3, 0);
		public static string builtForUnityVersion = UnityEditorVersionDetector.unityVersion;
		public static string EditorPrefsKeyPrefix = "com.flexbox4unity";
		#endregion
		
		
		#region Auto-managed settings and data

		/// <summary>
		/// This gets auto-updated (by F4UStartup.cs) every time a Settings file is loaded, and if different from its previous value,
		/// gets written back to disk, so that the project knows which version it was last loaded with.
		///
		/// This is a workaround for the fact that Unity still refuses to provide any API for asset-authors to detect
		/// and manage the installed / downloaded version, or to detect when an upgrade is happening.
		/// </summary>
		/// <seealso cref="Flexbox4Unity.F4UStartup"/>
		public Version lastLoadedVersion = new Version(0,0,0);
		
		[ContextMenu( "Apply to all in Scene" )]
		public void ApplyToAllInScene()
		{
			foreach( var fc in FindObjectsOfType<FlexContainer>() )
				fc.settings = this;
			foreach( var fi in FindObjectsOfType<FlexItem>() )
				fi.settings = this;
		}

		[ContextMenu( "DEBUG: find settings file" )]
		public void DEBUG_FindSettingsFiles()
		{
			Debug.Log( findProjectSettings );
		}

		public static Flexbox4UnityProjectSettings findProjectSettings
		{
			get
			{
				/**
				 * Undocumented: Resources.FindObjectsOfTypeAll was broken in Unity for many years
				 * (at least from 2011 through 2017), but appears to be working correctly now (in 2019.4 onwards).
				 *
				 * However: the docs for Resources.FindObjectsOfTypeAll are still incorrect/missing (they are self-contradictory!)
				 * so it's unclear how well this will work:
				 */
			#if UNITY_EDITOR
				/** To workaround bugs in the UnityEditor, we have to write 4 lines of code to simply "load the asset of type X",
				 * because the specific API call for doing that ... does not exist (the Editor implementation of FindObjectsOfTypeAll
				 * has never worked correctly, has never done what it says it does, and Unity seems happy to leave it broken foreever). 
				 */ 
				//Debug.Log("Not going to use FOOTA, but FYI it returned: "+Resources.FindObjectsOfTypeAll<Flexbox4UnityProjectSettings>().Length+" results");
				var settingsFileAssets = AssetDatabase.FindAssets("t:Flexbox4UnityProjectSettings");
				var settingsFiles = new Flexbox4UnityProjectSettings[ settingsFileAssets.Length ];
				for( int i=0; i<settingsFileAssets.Length; i++ )
				{
					settingsFiles[ i ] = AssetDatabase.LoadAssetAtPath<Flexbox4UnityProjectSettings>(AssetDatabase.GUIDToAssetPath(settingsFileAssets[ i ]));
				}
			#else
				/** NB: this call only RANDOMLY works correctly, but at runtime it's the only thing we have - we rely upon the
				 * undocumented behaviour of UnityRuntime that it preloads all referenced ScriptableObjects.
				 */
				var settingsFiles = Resources.FindObjectsOfTypeAll<Flexbox4UnityProjectSettings>();
			#endif
				

				if( settingsFiles.Length > 1 )
					throw new Exception( "Multiple Flexbox settings objects found in project - Unity does not support this, please delete one or more. Unity's names: " + string.Join( ";", settingsFiles.Select( settings => settings.name ) ) );

				if( settingsFiles.Length < 1 )
				{
					Debug.LogWarning( "Attempted use of Resources.FindObjectsOfTypeAll<> may have failed; please contact support for Flexbox4Unity and let me know what happened!" );
					
				}

				if( settingsFiles.Length < 1 )
					throw new Exception( "No Flexbox settings found in project" );

				return settingsFiles[0];
			}
		}
		
		public bool hasDisplayedFirstStartup = false;
		#endregion

		#region User-managed settings

			public bool autoUpgradePrefabsOnOpening = false;
			public bool autoUpgradeScenesOnOpening = false;
			
		public string invoiceNumber;

		[FormerlySerializedAs("layoutAlgorithm")] public IFlexboxLayoutAlgorithm v2layoutAlgorithm;
		public IFlexboxLayoutAlgorithmV3 v3layoutAlgorithm;

		public ScriptableObject currentLayoutAlgorithm
		{
			get
			{
				if( v3layoutAlgorithm != null )
					return v3layoutAlgorithm;
				else
					return v2layoutAlgorithm;
			}
		}

		public bool isLayoutInProgress
		{
			get
			{
				if( currentLayoutAlgorithm is IFlexboxLayoutAlgorithm v2Algorithm )
					return v2Algorithm.isLayoutInProgress;
				else if( currentLayoutAlgorithm is IFlexboxLayoutAlgorithmV3 v3Algorithm )
					return v3Algorithm.isLayoutInProgress;
				else
					throw new Exception( "Imposible: 3245l23ksdjf" );
			}
		}

		public GizmosRenderMode drawFlexHierarchyUsing;
		public Color flexHierarchyContainersOutlineColour = Color.green;
		[FormerlySerializedAs("gizmosInsetAmount")] public float flexHierarchyGizmosInsetAmount = 10f;

		public bool debugRefreshTriggers = false;
		public bool debugRelayoutCalls = false;
		public bool debugShowForceLayoutButton = false;
		#endregion

		/**
		 * This was only created so that build-preprocessing code can check whether the exported settings file matches
		 * the latest project-wide settings file; in reality, only the layoutAlgorithm's need to match right now
		 * (all other settings are ignored at runtime anyway)
		 */
		public override bool Equals(object otherObject)
		{
			Flexbox4UnityProjectSettings other = otherObject as Flexbox4UnityProjectSettings;

			if( other == null )
				return false;

			return    v2layoutAlgorithm == other.v2layoutAlgorithm
			       && v3layoutAlgorithm == other.v3layoutAlgorithm
			       && drawFlexHierarchyUsing == other.drawFlexHierarchyUsing
			       && flexHierarchyContainersOutlineColour == other.flexHierarchyContainersOutlineColour
			       && flexHierarchyGizmosInsetAmount == other.flexHierarchyGizmosInsetAmount
			       && debugRefreshTriggers == other.debugRefreshTriggers
			       && debugShowForceLayoutButton == other.debugShowForceLayoutButton;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode()
			       ^ (v2layoutAlgorithm != null ? v2layoutAlgorithm.GetHashCode() : 0) 
			       ^ (v3layoutAlgorithm != null ? v3layoutAlgorithm.GetHashCode() : 0)
				;
		}
	}
}