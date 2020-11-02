using UnityEngine;

namespace VirtualHole.Client.Data
{
	using Api.User;

	[CreateAssetMenu(menuName = "VirtualHole/Data/User Client Object")]
	public class VirtualHoleUserClientObject : ScriptableObject
	{
		private VirtualHoleUserClient _client = null;

		public VirtualHoleUserClient GetClient(string name, string email)
		{
			if(_client != null) { return _client; }
			_client = new VirtualHoleUserClient(name, email);
			return _client;
		}
	}
}
