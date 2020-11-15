using System.Collections.Generic;
using UnityEngine;
using Midnight;

namespace VirtualHole.Client.Data
{
	using APIWrapper.Storage.Dynamic;

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
