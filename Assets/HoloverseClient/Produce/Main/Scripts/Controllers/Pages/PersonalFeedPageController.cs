using System.Threading.Tasks;
using UnityEngine;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class PersonalFeedPageController : FeedPageController
	{
		protected override Node _mainNode => mainFlowMap.personalFeedNode;
		protected override Page _page => _mainFlowPersonalFeedMap.page;
		protected override VideoFeedScroll _videoFeed => _mainFlowPersonalFeedMap.videoFeed;
		protected override Section _videoFeedSection => _mainFlowPersonalFeedMap.videoSection;
		public Section emptySection => _mainFlowPersonalFeedMap.emptySection;
		[Space]
		[SerializeField]
		private MainFlowPersonalFeedMap _mainFlowPersonalFeedMap = null;

		private Section _emptyFeedSection = null;

		protected override CreatorQuery CreateCreatorQuery(HoloverseDataClient client) => null;

		protected override async void OnNodeVisit()
		{
			await Task.CompletedTask;
			_emptyFeedSection.gameObject.SetActive(true);
			await _emptyFeedSection.LoadAsync();
		}
	}
}
