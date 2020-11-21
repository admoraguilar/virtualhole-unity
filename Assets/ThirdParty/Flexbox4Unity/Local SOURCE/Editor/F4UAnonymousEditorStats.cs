using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Flexbox4Unity;
using UnityEditor;
using UnityEngine;

/**
 * This class is automatically compiled-out of Player builds, so it will never be included in any game, even by accident
 */
[InitializeOnLoad]
public class F4UAnonymousEditorStats
{
	#if UNITY_EDITOR
	[Serializable]
	public class AnonymizedUsageStats
	{
		public int containersCreated;
		public int itemsCreated;
		public int flexGrowUsed, flexShrinkUsed, flexPaddingUsed, flexMarginsUsed, flexDirectionUsed, flexSizeConstraintsUsed, flexDefaultSizesUsed, flexOrderUsed, flexExpandUsed, flexJustifyUsed, flexAlignUsed;
		public List<string> flexTemplatesUsed;

		public void LoadFromEditorKey(string key)
		{
			containersCreated = EditorPrefs.GetInt(key + "." + "containersCreated");
			itemsCreated = EditorPrefs.GetInt(key + "." + "itemsCreated");
			flexGrowUsed = EditorPrefs.GetInt(key + "." + "flexGrowUsed");
			flexShrinkUsed = EditorPrefs.GetInt(key + "." + "flexShrinkUsed");
			flexPaddingUsed = EditorPrefs.GetInt(key + "." + "flexPaddingUsed");
			flexMarginsUsed = EditorPrefs.GetInt(key + "." + "flexMarginsUsed");
			flexDirectionUsed = EditorPrefs.GetInt(key + "." + "flexDirectionUsed");
			flexSizeConstraintsUsed = EditorPrefs.GetInt(key + "." + "flexSizeConstraintsUsed");
			flexDefaultSizesUsed = EditorPrefs.GetInt(key + "." + "flexDefaultSizesUsed");
			flexOrderUsed = EditorPrefs.GetInt(key + "." + "flexOrderUsed");
			flexExpandUsed = EditorPrefs.GetInt(key + "." + "flexExpandUsed");
			flexJustifyUsed = EditorPrefs.GetInt(key + "." + "flexJustifyUsed");
			flexAlignUsed = EditorPrefs.GetInt(key + "." + "flexAlignUsed");
		}

		public void SaveToEditorKey(string key)
		{
			EditorPrefs.SetInt(key + "." + "containersCreated", containersCreated);
			EditorPrefs.SetInt(key + "." + "itemsCreated", itemsCreated);
			EditorPrefs.SetInt(key + "." + "flexGrowUsed",flexGrowUsed );
			EditorPrefs.SetInt(key + "." + "flexShrinkUsed", flexShrinkUsed );
			EditorPrefs.SetInt(key + "." + "flexPaddingUsed",flexPaddingUsed );
			EditorPrefs.SetInt(key + "." + "flexMarginsUsed",flexMarginsUsed );
			EditorPrefs.SetInt(key + "." + "flexDirectionUsed",flexDirectionUsed );
			EditorPrefs.SetInt(key + "." + "flexSizeConstraintsUsed",flexSizeConstraintsUsed );
			EditorPrefs.SetInt(key + "." + "flexDefaultSizesUsed",flexDefaultSizesUsed );
			EditorPrefs.SetInt(key + "." + "flexOrderUsed",flexOrderUsed );
			EditorPrefs.SetInt(key + "." + "flexExpandUsed",flexExpandUsed );
			EditorPrefs.SetInt(key + "." + "flexJustifyUsed",flexJustifyUsed );
			EditorPrefs.SetInt(key + "." + "flexAlignUsed",flexAlignUsed );
		}
	}

	private static AnonymizedUsageStats _liveStats;
	public static AnonymizedUsageStats liveStats
	{
		get { return isRecordingAnonymousUsage ? _liveStats : null; }
	}

	static F4UAnonymousEditorStats()
	{
		/** Load the saved Stats so we can show them to the user if they want to see their own data */
		{
			_liveStats = new AnonymizedUsageStats();
			_liveStats.LoadFromEditorKey( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "LocalUsageStatistics" );
		}

		/** Setup listeners (if the user has disabled anonymous stats gathering, each of these will do nothing) */
		{
			//Debug.Log("Reinitialized; adding self to actionhook");
			FlexboxActionHooks.shared.OnContainerCreated.AddListener(OnContainerCreated);
			FlexboxActionHooks.shared.OnItemCreated.AddListener(OnItemCreated);
			FlexboxActionHooks.shared.OnGrowSet.AddListener(OnGrowSet);
			FlexboxActionHooks.shared.OnShrinkSet.AddListener(OnShrinkSet);
			FlexboxActionHooks.shared.OnPaddingSet.AddListener(OnPaddingSet);
			FlexboxActionHooks.shared.OnMarginsSet.AddListener(OnMarginsSet);
			FlexboxActionHooks.shared.OnConstraintsSet.AddListener(OnConstraintsSet);
			FlexboxActionHooks.shared.OnDefaultWidthSet.AddListener(OnDefaultWidthSet);
			FlexboxActionHooks.shared.OnDefaultHeightSet.AddListener(OnDefaultHeightSet);
			FlexboxActionHooks.shared.OnOrderSet.AddListener(OnOrderSet);
			FlexboxActionHooks.shared.OnExpandChildrenToFitSelfSet.AddListener(OnExpandChildrenToFitSelfSet);
			FlexboxActionHooks.shared.OnDirectionSet.AddListener(OnDirectionSet);
			FlexboxActionHooks.shared.OnJustifySet.AddListener(OnJustifySet);
			FlexboxActionHooks.shared.OnAlignSet.AddListener(OnAlignSet);
			
		}
		
		/** Make sure we auto-save the local stats whenever the Editor quits, or reloads assemblies */
		{
			AssemblyReloadEvents.beforeAssemblyReload += SaveLocalUsageStatistics; 
		}

		/** Anonymise the session, make sure it's wiped every time the Editor restarts */
		{
			EditorApplication.quitting += EditorIsQuitting;

			isRecordingAnonymousUsage = EditorPrefs.GetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "RecordUsageAnonymously");
			string tempSessionID = EditorPrefs.GetString(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "TemporarySessionID");

			//Debug.Log("tempsessionID = \"" + tempSessionID + "\"");

			if( String.IsNullOrEmpty(tempSessionID) )
			{
				Guid newTempSessionID = Guid.NewGuid();
				EditorPrefs.SetString(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "TemporarySessionID", newTempSessionID.ToString());
				EditorStats.SetSessionID(newTempSessionID);
			}
			else
				EditorStats.SetSessionID(Guid.Parse(tempSessionID));
		}
		
		/** Send version-data (needed by support to know when to end-of-life old versions) */		
		EditorStats.sharedInstance.SendEvent("version","flexbox", ""+Flexbox4UnityProjectSettings.builtVersion, 1);
		EditorStats.sharedInstance.SendEvent("version","unity", ""+Flexbox4UnityProjectSettings.builtForUnityVersion, 1);
	}

	static void EditorIsQuitting()
	{
		/** Wipe the temporary session ID, so that a new random one will be auto-created next time the editor starts */
		EditorPrefs.SetString(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "TemporarySessionID", null);
	
		/** Save the local statistics (this is more performant than saving them continuously while in-editor) */
		SaveLocalUsageStatistics();
		
		/** Update the runs info */
		EditorPrefs.SetInt(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "NumLocalSessions", EditorPrefs.GetInt(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "NumLocalSessions") + 1 );
	}
	
	private static void SaveLocalUsageStatistics()
	{
		//Debug.Log("Saving local stats");
		_liveStats.SaveToEditorKey( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "LocalUsageStatistics" );
	}

	public static bool isRecordingAnonymousUsage;

	public static void SetRecordingUsageAnonymously(bool shouldRecord)
	{
		if( isRecordingAnonymousUsage != shouldRecord )
		{
			isRecordingAnonymousUsage = shouldRecord;
			EditorPrefs.SetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "RecordUsageAnonymously", shouldRecord);
		}
	}

	/**
	 * This method ensures that we ONLY record usage data when the user has pre-authorized it.
	 *
	 * It also enables us to disable usage-data when testing updates to this class
	 */
	private static void _RecordOnThing(string action, string label, ref int incrementableCounter )
	{
		if( isRecordingAnonymousUsage )
		{
			incrementableCounter++;
#if TESTING_NEW_VERSION
			EditorStats.sharedInstance.SendEvent("Developer:Flexbox", action, Application.isPlaying ? "rt:"+label : label, 1);
#else
		EditorStats.sharedInstance.SendEvent(  "Flexbox", action, Application.isPlaying ? "rt:"+label:label, 1);
#endif
		}
	}

	public static void OnContainerCreated( FlexContainer fc )
	{
		_RecordOnThing("FlexContainer", "created", ref _liveStats.containersCreated);
	}
		
	public static void OnItemCreated( FlexItem fi )
	{
		_RecordOnThing("FlexItem", "created", ref _liveStats.itemsCreated);
	}
	public static void OnGrowSet(FlexItem item)
	{
		_RecordOnThing("FlexItem", "grow", ref _liveStats.flexGrowUsed );
	}
	public static void OnShrinkSet( FlexItem item )
	{
		_RecordOnThing("FlexItem", "shrink", ref _liveStats.flexShrinkUsed );
	}
	public static void OnPaddingSet( FlexItem item )
	{
		_RecordOnThing("FlexItem", "padding", ref _liveStats.flexPaddingUsed );
	}
	public static void OnMarginsSet( FlexItem item )
	{
		_RecordOnThing("FlexItem", "margins", ref _liveStats.flexMarginsUsed );
	}
	public static void OnConstraintsSet( FlexItem item )
	{
		_RecordOnThing("FlexItem", "sizeConstraints", ref _liveStats.flexSizeConstraintsUsed );
	}
	public static void OnDefaultWidthSet( FlexItem item )
	{
		_RecordOnThing("FlexItem", "defaultWidth", ref _liveStats.flexDefaultSizesUsed );
	}
	public static void OnDefaultHeightSet( FlexItem item )
	{
		_RecordOnThing("FlexItem", "defaultHeight", ref _liveStats.flexDefaultSizesUsed );
	}
	public static void OnOrderSet( FlexItem item )
	{
		_RecordOnThing("FlexItem", "order", ref _liveStats.flexOrderUsed );
	}
	public static void OnExpandChildrenToFitSelfSet( FlexItem item )
	{
		_RecordOnThing("FlexItem", "expandChildrenToFit", ref _liveStats.flexExpandUsed );
	}
 
	public static void OnDirectionSet( FlexContainer container, FlexDirection direction )
	{
		_RecordOnThing("FlexContainer", "direction-"+direction, ref _liveStats.flexDirectionUsed );
	}
	public static void OnJustifySet( FlexContainer container, FlexJustify justify )
	{
		_RecordOnThing("FlexContainer", "justify-"+justify, ref _liveStats.flexJustifyUsed );
	}
	public static void OnAlignSet(FlexContainer container, AlignItems align)
	{
		_RecordOnThing("FlexContainer", "align-"+align, ref _liveStats.flexAlignUsed );
	}
#endif
}