using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Midnight;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace Holoverse.Data.YouTube
{
	using ExChannel = YoutubeExplode.Channels.Channel;
	using ExVideo = YoutubeExplode.Videos.Video;
	using ExBroadcast = YoutubeExplode.Videos.Broadcast;

	public class YouTubeScraper
	{
		private YoutubeClient _client = null;

		public YouTubeScraper()
		{
			_client = new YoutubeClient();
		}

		public async Task<Channel> GetChannelInfo(string channelUrl)
		{
			ExChannel channel = await _client.Channels.GetAsync(channelUrl);
			return new Channel {
				url = channel.Url,
				id = channel.Id,
				name = channel.Title,
				avatarUrl = channel.LogoUrl
			};
		}

		public async Task<List<Video>> GetChannelVideos(string channelUrl)
		{
			List<Video> results = new List<Video>();
			
			IReadOnlyList<ExVideo> videos = await _client.Channels.GetUploadsAsync(channelUrl);
			DateTimeOffset lastVideoDate = default;
			foreach(ExVideo video in videos) {
				// We process the video date because sometimes
				// the dates are messed up, so we run a correction to
				// fix it
				ExVideo processedVideo = video;
				if(lastVideoDate != default && processedVideo.UploadDate.Subtract(lastVideoDate).TotalDays > 60) {
					MLog.Log($"Wrong date detected! Fixing {processedVideo.Title}...");
					processedVideo = await _client.Videos.GetAsync(processedVideo.Url);
				}
				lastVideoDate = processedVideo.UploadDate;

				results.Add(new Video {
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

		public async Task<List<Broadcast>> GetChannelLiveBroadcasts(string channelUrl)
		{
			return await GetChannelBroadcasts(channelUrl, BroadcastType.Now);
		}

		public async Task<List<Broadcast>> GetChannelUpcomingBroadcasts(string channelUrl)
		{
			return await GetChannelBroadcasts(channelUrl, BroadcastType.Upcoming);
		}

		private async Task<List<Broadcast>> GetChannelBroadcasts(string channelUrl, BroadcastType type)
		{
			List<Broadcast> results = new List<Broadcast>();

			IReadOnlyList<ExVideo> broadcasts = await _client.Channels.GetBroadcastsAsync(channelUrl, type);
			foreach(ExBroadcast broadcast in broadcasts.Select(v => v as ExBroadcast)) {
				results.Add(new Broadcast {
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
