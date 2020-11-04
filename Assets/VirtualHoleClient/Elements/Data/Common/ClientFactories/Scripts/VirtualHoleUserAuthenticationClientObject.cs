using UnityEngine;

namespace VirtualHole.Client.Data
{
	using Api.UserAuthentication;

	[CreateAssetMenu(menuName = "VirtualHole/Data/User Authentication Client Object")]
	public class VirtualHoleUserAuthenticationClientObject : ScriptableObject
	{
		private VirtualHoleUserAuthenticationClient _client = null;

		public VirtualHoleUserAuthenticationClient GetClient(string name, string email)
		{
			if(_client != null) { return _client; }
			_client = new VirtualHoleUserAuthenticationClient(name, email);
			return _client;
		}
	}
}
