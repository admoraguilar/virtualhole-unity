﻿using UnityEngine;
using Midnight;
using VirtualHole.APIWrapper;

namespace VirtualHole.Client.Data
{
	[CreateAssetMenu(menuName = "VirtualHole/Client Factory/API Wrapper")]
	public class VirtualHoleAPIWrapperClientFactory : SingletonObject<VirtualHoleAPIWrapperClientFactory>
	{
		private static VirtualHoleAPIWrapperClient _apiWrapperClient = null;
		
		public static VirtualHoleAPIWrapperClient Get()
		{
			if(_apiWrapperClient != null) { return _apiWrapperClient; }
			_apiWrapperClient = new VirtualHoleAPIWrapperClient(_instance.apiDomain, _instance.storageDomain);
			return _apiWrapperClient;
		}

		[SerializeField]
		private string apiDomain = string.Empty;

		[SerializeField]
		private string storageDomain = string.Empty;
	}
}
