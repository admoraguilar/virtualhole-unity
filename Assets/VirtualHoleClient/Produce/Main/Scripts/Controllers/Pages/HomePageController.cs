using System.Collections.Generic;
using UnityEngine;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using APIWrapper.Contents.Creators;
	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class HomePageController : FeedPageController
	{
		protected override Node _mainNode => mainFlowMap.homeNode;
		protected override VideoFeedScroll _videoFeed => _homeFlowMap.videoFeed;
		[Space]
		[SerializeField]
		private HomeFlowMap _homeFlowMap = null;

		protected override CreatorQuery CreateCreatorQuery()
		{
			return new CreatorQuery(
				new ListCreatorsRequest {
					isCheckForAffiliations = true,
					affiliations = new List<string>() { CreatorQuery.Affiliation.hololiveProduction },
					batchSize = 100
				});
		}
	}
}
