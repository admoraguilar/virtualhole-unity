using UnityEngine;

namespace VirtualHole.Client.Data
{
	using Api.Storage;

	[CreateAssetMenu(menuName = "VirtualHole/Data/Storage Client Object")]
	public class VirtualHoleStorageClientObject : ScriptableObject
	{
		[SerializeField]
		private string _endpoint = string.Empty;

		public VirtualHoleStorageClient client
		{
			get {
				if(_client == null) {
					_client = new VirtualHoleStorageClient(_endpoint);
				}
				return _client;
			}
		}
		private VirtualHoleStorageClient _client = null;
	}
}
