using UnityEngine;

namespace Holoverse.Client
{
	using Api.Data;

	[CreateAssetMenu(menuName = "Holoverse/Data Client Object")]
	public class HoloverseDataClientObject : ScriptableObject
	{
		[SerializeField]
		private string _connectionString = string.Empty;

		[SerializeField]
		private string _userName = string.Empty;

		[SerializeField]
		private string _password = string.Empty;

		public HoloverseDataClient client
		{
			get {
				if(_client == null) {
					_client = new HoloverseDataClient(
						_connectionString, _userName,
						_password);
				}
				return _client;
			}
		}
		private HoloverseDataClient _client = null;
	}
}
