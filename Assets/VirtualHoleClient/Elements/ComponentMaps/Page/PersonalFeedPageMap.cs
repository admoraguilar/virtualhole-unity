using UnityEngine;

namespace VirtualHole.Client.ComponentMaps
{
	using Client.UI;

	public class PersonalFeedPageMap : MonoBehaviour
	{
		public VideoFeedScroll videoFeed => _videoFeed;
		[SerializeField]
		private VideoFeedScroll _videoFeed = null;

		public GameObject emptyDisplay => _emptyDisplay;
		[SerializeField]
		private GameObject _emptyDisplay = null;
	}
}
