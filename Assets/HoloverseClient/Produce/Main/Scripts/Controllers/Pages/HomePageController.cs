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

	public class HomePageController : FeedPageController
	{
		protected override Node _mainNode => mainFlowMap.homeNode;
		protected override Page _page => _mainFlowHomeMap.page;
		protected override VideoFeedScroll _videoFeed => _mainFlowHomeMap.videoFeed;
		protected override Section _videoFeedSection => _mainFlowHomeMap.videoSection;
		[Space]
		[SerializeField]
		private MainFlowHomeMap _mainFlowHomeMap = null;

		protected override CreatorQuery CreateCreatorQuery(HoloverseDataClient client) =>
			new CreatorQuery(
				client,
				new FindCreatorsSettings {
					isCheckForAffiliations = true,
					affiliations = new List<string>() { "hololiveProduction" },
					batchSize = 100
				});
	}
}
