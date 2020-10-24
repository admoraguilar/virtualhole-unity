﻿using System.Collections.Generic;

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
			await InitializeAsync();
		}

		private void OnNodeLeave()
		{

		}

		private void OnEnable()
		{
			flowTree.OnAttemptSetSameNodeAsCurrent += ScrollToTopIfSame;

			node.OnVisit += OnNodeVisit;
			node.OnLeave += OnNodeLeave;

			videoFeedSection.LoadContentTaskFactory += LoadContentAsync;

			videoFeed.videoScroll.OnScrollerPositionChanged += LoadIfNearEndScroll;
			videoFeed.dropdown.onValueChanged.AddListener(ClearAndRefreshFeed);
		}

		private void OnDisable()
		{
			flowTree.OnAttemptSetSameNodeAsCurrent -= ScrollToTopIfSame;

			node.OnVisit -= OnNodeVisit;
			node.OnLeave -= OnNodeLeave;

			videoFeed.videoScroll.OnScrollerPositionChanged -= LoadIfNearEndScroll;
			videoFeed.dropdown.onValueChanged.RemoveListener(ClearAndRefreshFeed);
		}
	}
}
