using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Web;
using Midnight.Caching;
using Midnight.Concurrency;

namespace Holoverse.Client.Controllers
{
	using Api.Data.Common;
	using Api.Data.Contents.Videos;
	using Api.Data.Contents.Creators;

	using Client.Data;
	using Client.UI;

	public static class PageControllerFactory
	{
		public static async Task<IEnumerable<VideoScrollRectCellData>> ProcessVideoFeed(
			VideoFeedData feedData, VideoFeedData.Feed feed, 
			CancellationToken cancellationToken = default)
		{
			List<VideoScrollRectCellData> results = new List<VideoScrollRectCellData>();

			List<Video> videos = (await feed.LoadVideosAsync(cancellationToken)).ToList();
			await Concurrent.ForEachAsync(videos, PreloadResources, cancellationToken);
			foreach(Video video in videos) {
				Sprite thumbnail = DataCache.Get<Sprite>(DataCacheKeys.videoThumbnailGroup, video.id);
				Sprite creatorSprite = DataCache.Get<Sprite>(DataCacheKeys.creatorAvatarGroup, video.creatorIdUniversal);

				// Skip videos without thumbnails, possible reasons for these are
				// they maybe privated or deleted.
				// Mostly observed on scheduled videos or livestreams that are already
				// finished.
				if(thumbnail == null) { continue; }

				VideoScrollRectCellData cellData = new VideoScrollRectCellData() {
					thumbnailSprite = thumbnail,
					indicatorSprite = null,
					title = video.title,
					date = video.creationDate.ToString(),
					creatorSprite = creatorSprite,
					creatorName = video.creator,
					onOptionsClick = null,
					onCellClick = () => Application.OpenURL(video.url)
				};

				results.Add(cellData);
			}

			return results;

			async Task PreloadResources(Video video)
			{
				if(!DataCache.Contains(DataCacheKeys.videoThumbnailGroup, video.id)) {
					DataCache.Add(
						DataCacheKeys.videoThumbnailGroup, video.id,
						await ImageGetWebRequest.GetAsync(video.thumbnailUrl));
				}

				if(!DataCache.Contains(DataCacheKeys.creatorAvatarGroup, video.creatorIdUniversal)) {
					string creatorAvatarUrl = feedData.creatorLookup[video.creatorIdUniversal].avatarUrl;
					DataCache.Add(
						DataCacheKeys.creatorAvatarGroup, video.creatorIdUniversal,
						await ImageGetWebRequest.GetAsync(creatorAvatarUrl));
				}

				await Task.CompletedTask;
			}
		}
	}
}
