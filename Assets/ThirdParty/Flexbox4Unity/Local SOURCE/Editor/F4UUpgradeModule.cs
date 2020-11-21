using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Version = IntelligentPluginVersioning.Version;

namespace Flexbox4Unity
{
	[System.Serializable]
	public class UpgradeResult
	{
		public bool success;
		public F4UUpgradeModule module;
		public string problemMessage, resolveMessage;
		public Action resolveAction;

		public static UpgradeResult Success { get; } = new UpgradeResult()
		{
			success = true, resolveAction = {}, problemMessage = "None", resolveMessage = "do nothing"
		};
	}
	
	public abstract class F4UUpgradeModule
	{
		public virtual Version minVersionToUpgrade { get { return new Version( -1, 0, 0 ); } }
		public virtual Version maxVersionToUpgrade { get { return new Version( -2, 0, 0 ); } }
		
		public bool shouldUpgradeVersion( Version v )
		{
			return v >= minVersionToUpgrade && v <= maxVersionToUpgrade;
		}
		
		public abstract void PrepareToProcessUpgrades( out UpgradeResult result );

		public abstract bool UpgradeProject( Version projectOldVersion, Version newVersion, List<string> allPrefabs, F4UUpgrader.UpgradeSettings settings );

		public virtual bool UpgradeSceneCurrentOnly( Version projectOldVersion, Version newVersion, List<string> allPrefabPaths, F4UUpgrader.UpgradeSettings settings )
		{
			return true;
		}
	}
}