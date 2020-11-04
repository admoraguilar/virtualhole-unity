using UnityEngine;
using Midnight;

namespace VirtualHole.Client.Data
{
	using Api.DB;

	[CreateAssetMenu(menuName = "VirtualHole/Data/Client Factory/DB")]
	public class VirtualHoleDBClientFactory : SingletonObject<VirtualHoleDBClientFactory>
	{
		private static VirtualHoleDBClient _client = null;

		public static VirtualHoleDBClient Get()
		{
			if(_client != null) { return _client; }
			_client = new VirtualHoleDBClient(
				_instance._connectionString, _instance._userName,
				_instance._password);

#if !UNITY_EDITOR
			_instance._connectionString = string.Empty;
			_instance._userName = string.Empty;
			_instance._password = string.Empty;
#endif

			return _client;
		}

		[SerializeField]
		private string _connectionString = string.Empty;

		[SerializeField]
		private string _userName = string.Empty;

		[SerializeField]
		private string _password = string.Empty;
	}
}
