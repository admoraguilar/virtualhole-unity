using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Web;
using Midnight.Concurrency;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Channels;
using System.Linq;

namespace Holoverse.Data.YouTube
{
	public partial class YouTubeScrapeOperation
	{
		private YouTubeScraperSettings _settings = null;
		private YouTubeScraper _scraper = null;
		private AggregateMap _map = null;

		public YouTubeScrapeOperation(YouTubeScraperSettings settings)
		{
			_settings = settings;
			_scraper = new YouTubeScraper();

			_map = new AggregateMap(
				PathUtilities.CreateDataPath("Holoverse", "", PathType.Data),
				settings
			);
		}

		public async Task Execute()
		{
			MLog.Log("Executing scraping operation....");

			List<string> channelUrls = new List<string>();
			channelUrls.AddRange(
				_settings.idols.SelectMany((YouTubeScraperSettings.ChannelGroup cg) => {
					return cg.channels.Select((Channel ch) => ch.url);
				})
			);
			channelUrls.AddRange(
				_settings.community.SelectMany((YouTubeScraperSettings.ChannelGroup cg) => {
					return cg.channels.Select((Channel ch) => ch.url);
				})
			);

			foreach(string channelUrl in channelUrls) {
				MLog.Log($"VIDEOS: {channelUrl}");
				List<Video> videos = await Retry(() => _scraper.GetChannelVideos(channelUrl));
				videos.ForEach((Video video) => {
					MLog.Log($"Scraping ARCHIVE video: {video.channel} | {video.title}");
					_map.Add(video);
				});

				MLog.Log($"UPCOMING broadcast: {channelUrl}");
				List<Broadcast> upcomingBroadcasts = await Retry(() => _scraper.GetChannelUpcomingBroadcasts(channelUrl));
				upcomingBroadcasts.ForEach((Broadcast broadcast) => {
					MLog.Log($"Scraping UPCOMING broadcast: {broadcast.channel} | {broadcast.title}");
					_map.Add(broadcast);
				});

				MLog.Log($"NOW broadcast: {channelUrl}");
				List<Broadcast> liveBroadcasts = await Retry(() => _scraper.GetChannelLiveBroadcasts(channelUrl));
				liveBroadcasts.ForEach((Broadcast broadcast) => {
					MLog.Log($"Scraping NOW broadcast: {broadcast.channel} | {broadcast.title}");
					_map.Add(broadcast);
				});
			}

			async Task<T> Retry<T>(Func<Task<T>> task)
			{
				T result = default;

				try {
					result = await task();
				} catch(Exception e) {
					MLog.LogError($"Encountered error: {e.Message}");
					MLog.LogWarning($"Retrying....");
					result = await Retry(task);
				}

				return result;
			}
		}

		public void Save()
		{
			MLog.Log("Saving scraped data...");
			_map.Save();
		}
	}
}
