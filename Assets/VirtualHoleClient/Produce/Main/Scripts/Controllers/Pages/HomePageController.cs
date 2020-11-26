using System.Collections.Generic;
using UnityEngine;
using Midnight.Unity.FlowTree;
using VirtualHole.Client.UI;
using VirtualHole.Client.Data;
using VirtualHole.Client.ComponentMaps;
using VirtualHole.APIWrapper.Contents.Creators;

namespace VirtualHole.Client.Controllers
{
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
