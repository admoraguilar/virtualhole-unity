using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Concurrency;

namespace Holoverse.Client.UI
{
	using Api.Data.Contents.Videos;

	using Client.Data;

	public static class UIFactory
	{
		public static async Task<IEnumerable<VideoScrollCellData>> CreateVideoScrollCellData(
			VideoFeedQuery feed, CancellationToken cancellationToken = default)
		{
			List<VideoScrollCellData> results = new List<VideoScrollCellData>();

			IEnumerable<Video> videoResult = await feed.LoadAsync(cancellationToken);
			if(videoResult == null) { return results; }

			List<Video> videos = videoResult.ToList();
			VideoCache.Add(videos);

			await Concurrent.ForEachAsync(videos, PreloadResources, 50, cancellationToken);

			foreach(Video video in videos) {
				Sprite thumbnail = await VideoCache.GetThumbnailAsync(video.platform, video.id);
				Sprite creatorSprite = await CreatorCache.GetAvatarAsync(video.creatorIdUniversal);

				// Skip videos without thumbnails, possible reasons for these are
				// they maybe privated or deleted.
				// Mostly observed on scheduled videos or livestreams that are already
				// finished.
				if(thumbnail == null) { continue; }

				VideoScrollCellData cellData = new VideoScrollCellData() {
					thumbnailSprite = thumbnail,
					title = video.title,
					date = video.creationDate.ToString(),
					creatorSprite = creatorSprite,
					creatorName = video.creator,
					creatorUniversalId = video.creatorIdUniversal,
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
				await VideoCache.GetThumbnailAsync(video.platform, video.id);
				await CreatorCache.GetAvatarAsync(video.creatorIdUniversal);
			}
		}
	}
}
