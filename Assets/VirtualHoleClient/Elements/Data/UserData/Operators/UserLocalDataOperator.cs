using Midnight;

namespace VirtualHole.Client.Data
{
	public class UserLocalDataOperator
	{
		public T Load<T>(string subPath, string assetName = "")
		{
			return JsonUtilities.LoadFromDisk<T>(new JsonUtilities.LoadFromDiskParameters {
				 filePath = CreatePath<T>(subPath, assetName)
			});
		}

		public void Save<T>(T instance, string subPath, string assetName = "")
		{
			JsonUtilities.SaveToDisk(instance, new JsonUtilities.SaveToDiskParameters {
				filePath = CreatePath<T>(subPath, assetName)
			});
		}

		private string CreatePath<T>(string subPath, string assetName = "")
		{
			subPath = subPath.Insert(0, "UserData");
			if(string.IsNullOrEmpty(assetName)) { assetName = $"{nameof(T)}.json"; }
			return PathUtilities.CreateDataPath(subPath, assetName, PathType.Persistent);
		}
	}
}
