using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Pages;
using Midnight.Concurrency;

namespace Holoverse.Client.Controllers
{
	using Api.Data.Common;
	using Api.Data.Contents.Creators;
	using Api.Data.Contents.Videos;

	using Client.Pages;

	public class VideoFeedLoader : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		[SerializeField]
		private Page _homePage = null;

		private async Task InitializeFeedAsync()
		{
			_homePage.Prewarm();

			List<Creator> creators = new List<Creator>();

			using(new StopwatchScope("Getting creators data..", "Start", "End")) {
				FindCreatorsSettings findCreatorsSettings =
					new FindCreatorsSettings {
						isCheckForAffiliations = true,
						affiliations = new List<string>() {
							"hololiveProduction"
						},
						batchSize = 100
					};

				FindResults<Creator> resultCreators = await _client
					.client.contents
					.creators.FindCreatorsAsync(findCreatorsSettings);

				while(await resultCreators.MoveNextAsync()) {
					creators.AddRange(resultCreators.current);
				}

				resultCreators.Dispose();
			}

			VideoFeedSection videoFeed = _homePage.GetSection<VideoFeedSection>();
			VideoFeedSection.ContentInfo[] contentInfos = new VideoFeedSection.ContentInfo[] {
				new VideoFeedSection.ContentInfo {
					type = "Discover",
					query = new FindCreatorVideosSettings<Video> {
						creators = creators,
						sortMode = FindCreatorVideosSettings<Video>.SortMode.ByCreationDate,
						isSortAscending = false
					}
				},

				new VideoFeedSection.ContentInfo {
					type = "Community",
					query = new FindCreatorRelatedVideosSettings<Video> {
						creators = creators,
						sortMode = FindVideosSettings<Video>.SortMode.ByCreationDate,
						isSortAscending = false
					}
				},
				new VideoFeedSection.ContentInfo {
					type = "Live",
					query = new FindCreatorVideosSettings<Video> {
						isBroadcast = true,
						isLive = true,
						creators = creators,
						sortMode = FindCreatorVideosSettings<Video>.SortMode.BySchedule,
						isSortAscending = false
					}
				},
				new VideoFeedSection.ContentInfo {
					type  = "Scheduled",
					query = new FindCreatorVideosSettings<Video> {
						isBroadcast = true,
						isLive = false,
						creators = creators,
						sortMode = FindVideosSettings<Video>.SortMode.BySchedule,
						isSortAscending = false
					}
				}
			};
			videoFeed.Initialize(contentInfos);

			foreach(VideoFeedSection.ContentInfo info in contentInfos) {
				MLog.Log($"Type: {info.type} | Query: {info.query.GetFilterDocument().ToString()}");
			}

			await _homePage.LoadAsync();
		}

		private void Start()
		{
			TaskExt.FireForget(InitializeFeedAsync());
		}
	}
}
