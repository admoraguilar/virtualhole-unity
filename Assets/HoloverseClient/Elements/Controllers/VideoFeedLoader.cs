using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Pages;
using Midnight.Concurrency;

namespace Holoverse.Client
{
	using Api.Data.Common;
	using Api.Data.Contents.Creators;
	using Api.Data.Contents.Videos;

	using Client.Pages;
	using MongoDB.Bson;

	public class VideoFeedLoader : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		[SerializeField]
		private Page _homePage = null;

		private async Task InitializeFeedAsync()
		{
			List<Creator> creators = new List<Creator>();

			using(new StopwatchScope("Getting creators data..", "Start", "End")) {
				FindResults<Creator> resultCreators = await _client
						.client.contents
						.creators.FindCreatorsAsync(
							new FindCreatorsSettings {
								isCheckForAffiliations = true,
								affiliations = new List<string> {
									"hololiveJapan"
								}
							});

				while(await resultCreators.MoveNextAsync()) {
					creators.AddRange(resultCreators.current);
				}
			}

			VideoFeedSection videoFeed = _homePage.GetSection<VideoFeedSection>();
			VideoFeedSection.ContentInfo[] contentInfos = new VideoFeedSection.ContentInfo[] {
				new VideoFeedSection.ContentInfo {
					type = "Discover",
					query = new FindCreatorVideosSettings<Video> {
						creatorIdsUniversal = creators.Select(c => c.universalId).ToList(),
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
					type = "Broadcasts",
					query = new FindCreatorVideosSettings<Video> {
						isBroadcast = true,
						creatorIdsUniversal = creators.Select(c => c.universalId).ToList(),
						sortMode = FindCreatorVideosSettings<Video>.SortMode.ByCreationDate,
						isSortAscending = false
					}
				},
			};
			videoFeed.Initialize(contentInfos);

			foreach(VideoFeedSection.ContentInfo info in contentInfos) {
				MLog.Log($"Type: {info.type} | Query: {info.query.GetFilterDocument().ToString()}");
			}

			//await _homePage.LoadAsync();
		}

		private void Start()
		{
			TaskExt.FireForget(InitializeFeedAsync());
		}
	}
}
