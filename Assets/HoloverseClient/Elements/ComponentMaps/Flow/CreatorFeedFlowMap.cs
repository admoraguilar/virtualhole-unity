using UnityEngine;

namespace Holoverse.Client.ComponentMaps
{
	using Client.UI;

	public class CreatorFeedFlowMap : MonoBehaviour
	{
		public VideoFeedScroll videoFeed => _videoFeed;
		[SerializeField]
		private VideoFeedScroll _videoFeed = null;
	}
}
