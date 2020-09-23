using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Midnight;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Channels;

namespace Holoverse.Data.YouTube
{
	public class YouTubeScraper
	{
		private YoutubeClient _client = null;

		public YouTubeScraper()
		{
			_client = new YoutubeClient();
		}

		public async Task<ChannelInfo> ScrapeChannelInfo(string channelUrl)
		{
			Channel channel = await _client.Channels.GetAsync(channelUrl);
			return new ChannelInfo {
				url = channel.Url,
				id = channel.Id,
				name = channel.Title,
				avatarUrl = channel.LogoUrl
			};
		}

		public async Task<List<VideoInfo>> ScrapeChannelVideos(string channelUrl)
		{
			List<VideoInfo> results = new List<VideoInfo>();
			
			IReadOnlyList<Video> videos = await _client.Channels.GetUploadsAsync(channelUrl);
			DateTimeOffset lastVideoDate = default;
			foreach(Video video in videos) {
				// We process the video date because sometimes
				// the dates are messed up, so we run a correction to
				// fix it
				Video processedVideo = video;
				if(lastVideoDate != default && processedVideo.UploadDate.Subtract(lastVideoDate).TotalDays > 60) {
					MLog.Log($"Wrong date detected! Fixing {processedVideo.Title}...");
					processedVideo = await _client.Videos.GetAsync(processedVideo.Url);
				}
				lastVideoDate = processedVideo.UploadDate;

				results.Add(new VideoInfo {
					url = processedVideo.Url,
					id = processedVideo.Id,
					title = processedVideo.Title,
					duration = processedVideo.Duration.ToString(),
					viewCount = processedVideo.Engagement.ViewCount,
					mediumResThumbnailUrl = processedVideo.Thumbnails.MediumResUrl,
					channel = processedVideo.Author,
					channelId = processedVideo.ChannelId,
					uploadDate = processedVideo.UploadDate.ToString()
				});
			}

			return results;
		}

		public async Task<List<BroadcastInfo>> ScrapeChannelBroadcasts(string channelUrl)
		{
			List<BroadcastInfo> results = new List<BroadcastInfo>();

			IReadOnlyList<Video> broadcasts = await _client.Channels.GetBroadcastsAsync(channelUrl, BroadcastType.Now);
			foreach(Broadcast broadcast in broadcasts.Select(v => v as Broadcast)) {
				results.Add(new BroadcastInfo {
					url = broadcast.Url,
					id = broadcast.Id,
					title = broadcast.Title,
					duration = broadcast.Duration.ToString(),
					viewCount = broadcast.Engagement.ViewCount,
					mediumResThumbnailUrl = broadcast.Thumbnails.MediumResUrl,
					channel = broadcast.Author,
					channelId = broadcast.ChannelId,
					uploadDate = broadcast.UploadDate.ToString(),
					IsLive = broadcast.IsLive,
					viewerCount = broadcast.ViewerCount,
					schedule = broadcast.Schedule.ToString()
				});
			}

			return results;
		}
	}
}
