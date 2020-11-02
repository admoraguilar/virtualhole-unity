using UnityEngine;

namespace VirtualHole.Client.Data
{
	using Api.DB;

	[CreateAssetMenu(menuName = "VirtualHole/Data/DB Client Object")]
	public class VirtualHoleDBClientObject : ScriptableObject
	{
		[SerializeField]
		private string _connectionString = string.Empty;

		[SerializeField]
		private string _userName = string.Empty;

		[SerializeField]
		private string _password = string.Empty;

		private VirtualHoleDBClient _client = null;

		public VirtualHoleDBClient GetClient()
		{
			if(_client != null) { return _client; }
			_client = new VirtualHoleDBClient(
				_connectionString, _userName,
				_password);
#if !UNITY_EDITOR
			_connectionString = string.Empty;
			_userName = string.Empty;
			_password = string.Empty;
#endif
			return _client;
		}
	}
}
