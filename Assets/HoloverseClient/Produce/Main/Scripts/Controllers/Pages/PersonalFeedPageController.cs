using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data;
	using Api.Data.Contents.Creators;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class PersonalFeedPageController : FeedPageController
	{
		protected override Node _mainNode => mainFlowMap.personalFeedNode;
		protected override Page _page => _mainFlowPersonalFeedMap.page;
		protected override VideoFeedScroll _videoFeed => _mainFlowPersonalFeedMap.videoFeed;
		protected override Section _videoFeedSection => _mainFlowPersonalFeedMap.videoSection;
		private Section _emptySection => _mainFlowPersonalFeedMap.emptySection;
		[Space]
		[SerializeField]
		private MainFlowPersonalFeedMap _mainFlowPersonalFeedMap = null;

		protected override CreatorQuery CreateCreatorQuery(HoloverseDataClient client) 
		{
			return new CreatorQuery(client, new FindCreatorsRegexSettings {
				searchQueries = new List<string>() { "Watame", "Matsuri", "Haato", "Subaru" }
			});
		}

		protected override async void OnNodeVisit()
		{
			_emptySection.gameObject.SetActive(false);
			base.OnNodeVisit();

			//await Task.CompletedTask;
			//_emptySection.gameObject.SetActive(true);
			//await _emptySection.LoadAsync();
		}
	}
}
