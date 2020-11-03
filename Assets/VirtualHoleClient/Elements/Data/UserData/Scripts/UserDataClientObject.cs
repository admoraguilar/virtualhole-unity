using UnityEngine;

namespace VirtualHole.Client.Data
{
	[CreateAssetMenu(menuName = "VirtualHole/Data/User Data Client Object")]
	public class UserDataClientObject : ScriptableObject
	{
		[SerializeField]
		private string _idToken = string.Empty;

		[SerializeField]
		private string _localRootPath = string.Empty;

		[SerializeField]
		private string _localSubPath = string.Empty;

		public UserDataClient client { get; private set; }

		public UserDataClient CreateClient()
		{
			return client = new UserDataClient(_idToken, _localRootPath, _localSubPath);
		}

		public UserDataClient CreateClient(string idToken, string localRootPath)
		{
			_idToken = idToken;
			_localRootPath = localRootPath;
			return client = new UserDataClient(_idToken, _localRootPath, _localSubPath);
		}
	}
}
