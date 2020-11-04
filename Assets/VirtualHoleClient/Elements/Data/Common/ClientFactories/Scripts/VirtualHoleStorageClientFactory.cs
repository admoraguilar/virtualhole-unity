using UnityEngine;
using Midnight;

namespace VirtualHole.Client.Data
{
	using Api.Storage;

	[CreateAssetMenu(menuName = "VirtualHole/Data/Client Factory/Storage")]
	public class VirtualHoleStorageClientFactory : SingletonObject<VirtualHoleStorageClientFactory>
	{
		private static VirtualHoleStorageClient _client = null;

		public static VirtualHoleStorageClient Get()
		{
			if(_client != null) { return _client; }
			_client = new VirtualHoleStorageClient(_instance._endpoint);
#if !UNITY_EDITOR
			_instance._endpoint = string.Empty;
#endif
			return _client;
		}

		[SerializeField]
		private string _endpoint = string.Empty;
	}
}
