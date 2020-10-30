using UnityEngine;
using Midnight.Pages;

namespace Holoverse.Client.ComponentMaps
{
	using Client.UI;

	public class PersonalFeedFlowMap : MonoBehaviour
	{
		public VideoFeedScroll videoFeed => _videoFeed;
		[SerializeField]
		private VideoFeedScroll _videoFeed = null;

		public Section emptySection => _emptySection;
		[SerializeField]
		private Section _emptySection = null;
	}
}
