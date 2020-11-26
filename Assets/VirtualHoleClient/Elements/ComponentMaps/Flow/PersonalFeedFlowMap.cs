using UnityEngine;
using Midnight.Unity.Pages;
using VirtualHole.Client.UI;

namespace VirtualHole.Client.ComponentMaps
{
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
