using System.IO;
using System.Threading.Tasks;
using Midnight.Unity;

namespace VirtualHole.Client.Data
{
	public class UserPersonalizationClient
	{
		private string _localPath => Path.Combine(
			_dataClient.localRootPath, _dataClient.localSubPath,
			"UserPersonalization.json");

		private UserPersonalizationV1 _data = null;
		private UserDataClient _dataClient = null;

		public UserPersonalizationClient(UserDataClient dataClient)
		{
			_dataClient = dataClient;
		}

		public async Task<UserPersonalizationV1> GetAsync()
		{
			await Task.CompletedTask;

			if(_data != null) { return _data; }
			return _data = JsonUtilities.LoadFromDisk<UserPersonalizationV1>(new JsonUtilities.LoadFromDiskParameters {
				filePath = _localPath
			});
		}

		public async Task UpsertAsync()
		{
			await Task.CompletedTask;

			JsonUtilities.SaveToDisk(_data, new JsonUtilities.SaveToDiskParameters {
				filePath = _localPath
			});
		}
	}
}
