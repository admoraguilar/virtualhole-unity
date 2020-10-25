using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Web;
using Midnight.Caching;
using Midnight.Concurrency;

namespace Holoverse.Client.Controllers
{
	using Api.Data.Contents.Videos;
	using Api.Data.Contents.Creators;

	using Client.Data;
	using Client.UI;

	public static class PageControllerFactory
	{
		public static async Task<IEnumerable<VideoScrollRectCellData>> CreateCellData(
			VideoFeedData feedData, VideoFeedData.Feed feed, 
			CancellationToken cancellationToken = default)
		{
			List<VideoScrollRectCellData> results = new List<VideoScrollRectCellData>();

			IEnumerable<Video> videoResult = await feed.LoadVideosAsync(cancellationToken);
			if(videoResult == null) { return results; }

			List<Video> videos = videoResult.ToList();
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
					title = video.title,
					date = video.creationDate.ToString(),
					creatorSprite = creatorSprite,
					creatorName = video.creator,
					onOptionsClick = () => { },
					onCellClick = () => Application.OpenURL(video.url)
				};

				if(video is Broadcast broadcast) {
					if(broadcast.isLive) { cellData.indicatorSprite = UIResources.GetIndicatorSprite(true); }
					else { cellData.indicatorSprite = UIResources.GetIndicatorSprite(false); }
				}

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
					if(feedData.creatorLookup.TryGetValue(video.creatorIdUniversal, out Creator creator)) {
						DataCache.Add(
							DataCacheKeys.creatorAvatarGroup, video.creatorIdUniversal,
							await ImageGetWebRequest.GetAsync(creator.avatarUrl));
					}
				}
			}
		}
	}
}
