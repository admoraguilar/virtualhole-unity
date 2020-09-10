using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Concurrency;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Holoverse.Scraper
{
	public class ChannelInfo
	{
		public string url;
		public string id;
		public string name;
		public string avatarUrl;
	}

	public class ChannelVideo
	{
		public string url;
		public string id;
		public string title;
		public string duration;
		public string viewCount;
		public string thumbnailUrl;
		public string channel;
		public string uploadDate;
	}

	public class HoloverseScraper : MonoBehaviour
	{
		public TextAsset channelUrlsTxt = null;

		[Space]
		public bool shouldRunOnStart = false;
		public float frequencyMinutes = 5f;

		private DateTime _lastRun = DateTime.MinValue;
		private bool _isStopped = false;

		[ContextMenu("Run")]
		public void Run()
		{
			MLog.Log("Scraping started");

			_isStopped = false;
			TaskExt.FireForget(RunAsync());

			async Task RunAsync()
			{
				while(!_isStopped) {
					_lastRun = DateTime.Now;

					IReadOnlyList<string> channelUrls = GetNewLineSeparatedValues(channelUrlsTxt.text);
					await Concurrent.ForEachAsync(channelUrls, ScrapeChannel, 5);

					DateTime nextRun = _lastRun.AddMinutes(frequencyMinutes);
					while(DateTime.Now < nextRun && !_isStopped) {
						MLog.Log("Cooldown...");
						await Task.Yield();
					}
				}
			}

			async Task ScrapeChannel(string channelUrl)
			{
				YoutubeClient client = new YoutubeClient();
				Channel channel = await client.Channels.GetAsync(new ChannelId(channelUrl));

				string infoJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{channel.Id}", "info.json", false);
				ChannelInfo channelInfo = new ChannelInfo() {
					url = channel.Url,
					id = channel.Id,
					name = channel.Title,
					avatarUrl = channel.LogoUrl,
				};
				JsonUtilities.SaveToDisk(channelInfo, new JsonUtilities.SaveToDiskParameters() {
					filePath = infoJsonPath,
					onSave = (JsonUtilities.OperationResponse res) => {
						MLog.Log($"Channel {channelInfo.name} info scraped.");
					}
				});

				string videosJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{channel.Id}", "videos.json", false);
				List<ChannelVideo> channelVideos = new List<ChannelVideo>();
				IReadOnlyList<Video> videos = await client.Channels.GetUploadsAsync(channelUrl);
				foreach(Video video in videos) {
					ChannelVideo channelVideo = new ChannelVideo() {
						url = video.Url,
						id = video.Id,
						title = video.Title,
						duration = video.Duration.ToString(),
						viewCount = video.Engagement.ViewCount.ToString(),
						thumbnailUrl = video.Thumbnails.MediumResUrl,
						channel = video.ChannelId,
						uploadDate = video.UploadDate.ToString()
					};
					channelVideos.Add(channelVideo);
				}
				JsonUtilities.SaveToDisk(channelVideos, new JsonUtilities.SaveToDiskParameters() {
					filePath = videosJsonPath,
					onSave = (JsonUtilities.OperationResponse res) => {
						MLog.Log($"Channel {channelInfo.name} videos scraped.");
					}
				});
			}
		}

		[ContextMenu("Stop")]
		public void Stop()
		{
			MLog.Log("Scraping stopped");

			_isStopped = true;
		}

		private IReadOnlyList<string> GetNewLineSeparatedValues(string content)
		{
			return channelUrlsTxt.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		}

		private void Start()
		{
			if(shouldRunOnStart) { Run(); }
		}
	}
}
