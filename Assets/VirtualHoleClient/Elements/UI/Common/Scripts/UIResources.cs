using System;
using UnityEngine;
using Midnight;

namespace VirtualHole.Client.UI
{
	using APIWrapper.Contents;
	
	[CreateAssetMenu(menuName = "VirtualHole/UI/UI Resources")]
	public class UIResources : SingletonObject<UIResources>
	{
		[Serializable]
		public class PlatformUI
		{
			public Platform platform = default;
			public Sprite logo = null;
		}

		public static Sprite GetIndicatorSprite(bool isLive)
		{
			if(isLive) { return _instance._liveIndicator; }
			else { return _instance._scheduledIndicator; }
		}

		public static PlatformUI GetPlatformUI(Platform platform)
		{
			foreach(PlatformUI platformUI in _instance._platformUIs) {
				if(platformUI.platform != platform) { continue; }
				return platformUI;
			}

			return null;
		}

		[SerializeField]
		private Sprite _liveIndicator = null;

		[SerializeField]
		private Sprite _scheduledIndicator = null;

		[SerializeField]
		private PlatformUI[] _platformUIs = null;
	}
}
