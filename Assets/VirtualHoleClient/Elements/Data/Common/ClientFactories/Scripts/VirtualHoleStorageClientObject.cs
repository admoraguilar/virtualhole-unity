using UnityEngine;

namespace VirtualHole.Client.Data
{
	using Api.Storage;

	[CreateAssetMenu(menuName = "VirtualHole/Data/Storage Client Object")]
	public class VirtualHoleStorageClientObject : ScriptableObject
	{
		[SerializeField]
		private string _endpoint = string.Empty;

		private VirtualHoleStorageClient _client = null;

		public VirtualHoleStorageClient GetClient()
		{
			if(_client != null) { return _client; }
			_client = new VirtualHoleStorageClient(_endpoint);
#if !UNITY_EDITOR
			_endpoint = string.Empty;
#endif
			return _client;
		}
	}
}
