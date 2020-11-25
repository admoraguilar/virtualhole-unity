using UnityEngine;
using VirtualHole.Client.UI;

namespace VirtualHole.Client.ComponentMaps
{
	public class HomeFlowMap : MonoBehaviour
	{
		public VideoFeedScroll videoFeed => _videoFeed;
		[SerializeField]
		private VideoFeedScroll _videoFeed = null;
	}
}
