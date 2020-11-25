using UnityEngine;
using VirtualHole.Client.UI;

namespace VirtualHole.Client.ComponentMaps
{
	public class CreatorFeedFlowMap : MonoBehaviour
	{
		public VideoFeedScroll videoFeed => _videoFeed;
		[SerializeField]
		private VideoFeedScroll _videoFeed = null;
	}
}
