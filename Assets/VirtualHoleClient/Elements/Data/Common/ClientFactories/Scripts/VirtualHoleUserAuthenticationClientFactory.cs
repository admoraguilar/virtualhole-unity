using UnityEngine;
using Midnight;

namespace VirtualHole.Client.Data
{
	using Api.UserAuthentication;

	[CreateAssetMenu(menuName = "VirtualHole/Data/Client Factory/User Authentication")]
	public class VirtualHoleUserAuthenticationClientFactory : SingletonObject<VirtualHoleUserAuthenticationClientFactory>
	{
		private static VirtualHoleUserAuthenticationClient _client = null;

		public static VirtualHoleUserAuthenticationClient Get(string name, string email)
		{
			if(_client != null) { return _client; }
			_client = new VirtualHoleUserAuthenticationClient(name, email);
			return _client;
		}
	}
}
