using System.Collections.Generic;
using UnityEngine;
using Midnight.Unity;
using VirtualHole.APIWrapper.Storage.Dynamic;

namespace VirtualHole.Client.Data
{
	[CreateAssetMenu(menuName = "VirtualHole/Data/Support List Builder")]
	public class SupportListBuilder : ScriptableObject
	{
		public List<SupportInfo> supportList = new List<SupportInfo>();

		[ContextMenu("Serialize")]
		public void Serialize()
		{
			string dataPath = PathUtilities.CreateDataPath("Data", "support-list.json", PathType.Data);
			JsonUtilities.SaveToDisk(supportList, new JsonUtilities.SaveToDiskParameters {
				filePath = dataPath
			});
		}
	}
}
