using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Humanizer;
using Midnight.Web;
using Midnight.Concurrency;

namespace Holoverse.Client.UI
{
	using Api.Data.Contents.Videos;
	using Api.Data.Contents.Creators;

	using Client.Data;
	
	public static class UIFactory
	{
		public static async Task<IEnumerable<CreatorScrollCellData>> CreateCreatorScrollCellDataAsync(
			CreatorQuery query, CancellationToken cancellationToken = default)
		{
			List<CreatorScrollCellData> results = new List<CreatorScrollCellData>();

			IEnumerable<Creator> creatorResult = await query.LoadAsync(cancellationToken);
			if(creatorResult == null) { return results; }

			List<Creator> creators = creatorResult.ToList();
			await Concurrent.ForEachAsync(creators, PreloadResources, 50, cancellationToken);

			foreach(Creator creator in query.creatorLookup.Values) {
				CreatorScrollCellData cellData = new CreatorScrollCellData {
					creatorAvatar = await CreatorCache.GetAvatarAsync(creator),
					creatorName = creator.universalName,
					creatorId = creator.universalId,
					onCellClick = () => { }
				};
				results.Add(cellData);
			}

			return results;

			async Task PreloadResources(Creator creator)
			{
				await CreatorCache.GetAvatarAsync(creator);
			}
		}

		public static async Task<IEnumerable<VideoScrollCellData>> CreateVideoScrollCellDataAsync(
			VideoFeedQuery query, CancellationToken cancellationToken = default)
		{
			List<VideoScrollCellData> results = new List<VideoScrollCellData>();

			IEnumerable<Video> videoResult = await query.LoadAsync(cancellationToken);
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
					date = video.creationDate.Humanize(),
					creatorSprite = creatorSprite,
					creatorName = video.creator,
					creatorUniversalId = video.creatorIdUniversal,
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
				await VideoCache.GetThumbnailAsync(video);
				await CreatorCache.GetAvatarAsync(video.creatorIdUniversal);
			}
		}

		public static async Task<IEnumerable<InfoButtonData>> CreateInfoButtonDataAsync(
			IEnumerable<SupportInfo> supportInfos, CancellationToken cancellationToken = default)
		{
			List<InfoButtonData> results = new List<InfoButtonData>();

			Dictionary<string, Sprite> _spriteLookup = new Dictionary<string, Sprite>();
			await Concurrent.ForEachAsync(supportInfos.ToList(), PreloadResources, cancellationToken);

			foreach(SupportInfo supportInfo in supportInfos) {
				InfoButtonData infoButtonData = new InfoButtonData() {
					header = supportInfo.header,
					content = supportInfo.content,
					onClick = () => Application.OpenURL(supportInfo.url)
				};

				_spriteLookup.TryGetValue(supportInfo.imageUrl, out Sprite sprite);
				infoButtonData.sprite = sprite;

				results.Add(infoButtonData);
			}

			return results;

			async Task PreloadResources(SupportInfo supportInfo)
			{
				if(!string.IsNullOrEmpty(supportInfo.imageUrl)) {
					_spriteLookup[supportInfo.imageUrl] = await ImageGetWebRequest.GetAsync(supportInfo.imageUrl);
				}
			}
		}
	}
}
