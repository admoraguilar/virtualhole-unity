using UnityEngine;
using Midnight.Pages;

namespace Holoverse.Client.ComponentMaps
{
	using Client.UI;

	public class MainFlowPersonalFeedMap : MonoBehaviour
	{
		public Page page => _page;
		[SerializeField]
		private Page _page = null;

		public Section videoSection => _videoSection;
		[SerializeField]
		private Section _videoSection = null;

		public VideoFeedScroll videoFeed => _videoFeed;
		[SerializeField]
		private VideoFeedScroll _videoFeed = null;

		public Section emptySection => _emptySection;
		[SerializeField]
		private Section _emptySection = null;
	}
}
