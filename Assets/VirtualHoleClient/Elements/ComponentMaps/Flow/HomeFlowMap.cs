using UnityEngine;

namespace VirtualHole.Client.ComponentMaps
{
	using Client.UI;
	
	public class HomeFlowMap : MonoBehaviour
	{
		public VideoFeedScroll videoFeed => _videoFeed;
		[SerializeField]
		private VideoFeedScroll _videoFeed = null;
	}
}
