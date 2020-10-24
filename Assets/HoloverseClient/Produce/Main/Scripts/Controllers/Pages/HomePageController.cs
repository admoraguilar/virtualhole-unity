using System.Collections.Generic;

namespace Holoverse.Client.Controllers
{
	using Api.Data;
	using Api.Data.Contents.Creators;

	using Client.Data;

	public class HomePageController : FeedPageController
	{
		protected override VideoFeedData CreateVideoFeedData(HoloverseDataClient client) =>
			new VideoFeedData(
				client,
				new FindCreatorsSettings {
					isCheckForAffiliations = true,
					affiliations = new List<string>() {
						"hololiveProduction"
					},
					batchSize = 100
				});
	}
}
