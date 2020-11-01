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

		public VirtualHoleDBClient client
		{
			get {
				if(_client == null) {
					_client = new VirtualHoleDBClient(
						_connectionString, _userName,
						_password);
				}
				return _client;
			}
		}
		private VirtualHoleDBClient _client = null;
	}
}
