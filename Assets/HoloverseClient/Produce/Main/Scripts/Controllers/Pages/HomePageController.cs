using System.Collections.Generic;

namespace Holoverse.Client.Controllers
{
	using Api.Data.Contents.Creators;

	using Client.Data;

	public class HomePageController : FeedPageController
	{
		protected override VideoFeedData CreateVideoFeedData() =>
			new VideoFeedData(
				client,
				new FindCreatorsSettings {
					isCheckForAffiliations = true,
					affiliations = new List<string>() {
						"hololiveProduction"
					},
					batchSize = 100
				});

		private async void OnNodeVisit()
		{
			await InitializeAsync(CreateCancelToken().Token);
		}

		private async void OnNodeLeave()
		{
			await page.UnloadAsync();
		}
	}
}
