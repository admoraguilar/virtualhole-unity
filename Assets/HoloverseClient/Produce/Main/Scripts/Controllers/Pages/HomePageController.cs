using System.Collections.Generic;
using Midnight.SOM;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data;
	using Api.Data.Contents.Creators;

	using Client.SOM;
	using Client.Data;
	using Client.UI;

	public class HomePageController : FeedPageController
	{
		protected override CreatorQuery CreateCreatorQuery(HoloverseDataClient client) =>
			new CreatorQuery(
				client,
				new FindCreatorsSettings {
					isCheckForAffiliations = true,
					affiliations = new List<string>() {
						"hololiveProduction"
					},
					batchSize = 100
				});

		protected override void SetReferences(
			ref Page page, ref Section videoFeedSection, 
			ref VideoFeedScroll videoFeed, ref Node mainNode)
		{
			SceneObjectModel som = SceneObjectModel.Get(this);
			page = som.GetCachedComponent<MainFlowHomeMap>().page;
			videoFeedSection = som.GetCachedComponent<MainFlowHomeMap>().videoSection;
			videoFeed = som.GetCachedComponent<MainFlowHomeMap>().videoFeed;
			mainNode = som.GetCachedComponent<MainFlowMap>().homeNode;
		}
	}
}
