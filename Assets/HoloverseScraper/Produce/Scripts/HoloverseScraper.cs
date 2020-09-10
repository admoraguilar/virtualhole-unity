using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
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
	[Serializable]
	public class ChannelInfo
	{
		public string url;
		public string id;
		public string name;
		public string avatarUrl;
	}

	[Serializable]
	public class ChannelVideo
	{
		public string url;
		public string id;
		public string title;
		public string duration;
		public string viewCount;
		public string thumbnailUrl;
		public string channel;
		public string channelId;
		public string uploadDate;
	}

	public class HoloverseScraper : MonoBehaviour
	{
		public TextAsset idolChannelUrlsTxt = null;
		public TextAsset audienceChannelUrlsTxt = null;

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
					Stopwatch runStopwatch = new Stopwatch();
					runStopwatch.Start();

					_lastRun = DateTime.Now;

					List<ChannelInfo> idolChannels = new List<ChannelInfo>();

					// idolVideos.json
					List<ChannelVideo> idolVideos = new List<ChannelVideo>();
					IReadOnlyList<string> idolChannelUrls = GetNewLineSeparatedValues(idolChannelUrlsTxt.text);
					await Concurrent.ForEachAsync(
						idolChannelUrls, 
						(string url) => {
							return ScrapeChannel(
								"Idols", url, 
								(ChannelInfo info) => { idolChannels.Add(info); },
								(ChannelVideo video) => { idolVideos.Add(video); }
							);
						},
						5
					);
					idolVideos = idolVideos.OrderByDescending((ChannelVideo video) => DateTimeOffset.Parse(video.uploadDate)).ToList();
					string idolVideosJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/Idols", "idolVideos.json", false);
					JsonUtilities.SaveToDisk(idolVideos, new JsonUtilities.SaveToDiskParameters() {
						filePath = idolVideosJsonPath,
						onSave = (JsonUtilities.OperationResponse res) => {
							MLog.Log($"Idol Videos scraped.");
						}
					});

					// idolChannels.json
					idolChannels = idolChannels.OrderBy((ChannelInfo info) => info.name).ToList();
					string idolChannelsJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/Idols", "idolChannels.json", false);
					JsonUtilities.SaveToDisk(idolChannels, new JsonUtilities.SaveToDiskParameters() {
						filePath = idolChannelsJsonPath,
						onSave = (JsonUtilities.OperationResponse res) => {
							MLog.Log($"Idol Channels scraped.");
						}
					});

					List<ChannelInfo> audienceChannels = new List<ChannelInfo>();

					// audienceVideos.json
					List<ChannelVideo> audienceVideos = new List<ChannelVideo>();
					IReadOnlyList<string> audienceChannelUrls = GetNewLineSeparatedValues(audienceChannelUrlsTxt.text);
					await Concurrent.ForEachAsync(
						audienceChannelUrls, 
						(string url) => {
							return ScrapeChannel(
								"Audiences", url,
								(ChannelInfo info) => { audienceChannels.Add(info); },
								(ChannelVideo video) => { audienceVideos.Add(video); }
							);
						},
						5
					);
					audienceVideos = audienceVideos.OrderByDescending((ChannelVideo video) => DateTimeOffset.Parse(video.uploadDate)).ToList();
					string audienceVideosJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/Audiences", "audienceVideos.json", false);
					JsonUtilities.SaveToDisk(audienceVideos, new JsonUtilities.SaveToDiskParameters() {
						filePath = audienceVideosJsonPath,
						onSave = (JsonUtilities.OperationResponse res) => {
							MLog.Log($"Audience Videos scraped.");
						}
					});

					// audienceChannels.json
					audienceChannels = audienceChannels.OrderBy((ChannelInfo info) => info.name).ToList();
					string audienceChannelsJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/Audiences", "audienceChannels.json", false);
					JsonUtilities.SaveToDisk(audienceChannels, new JsonUtilities.SaveToDiskParameters() {
						filePath = audienceChannelsJsonPath,
						onSave = (JsonUtilities.OperationResponse res) => {
							MLog.Log($"Audience Channels scraped.");
						}
					});

					runStopwatch.Stop();
					MLog.LogWarning($"Scraping run finished at: {runStopwatch.Elapsed}");

					DateTime nextRun = _lastRun.AddMinutes(frequencyMinutes);
					while(DateTime.Now < nextRun && !_isStopped) {
						MLog.Log("Cooldown...");
						await Task.Yield();
					}
				}
			}

			async Task ScrapeChannel(
				string subPath, string channelUrl, Action<ChannelInfo> onChannelScraped = null, 
				Action<ChannelVideo> onVideoScraped = null)
			{
				YoutubeClient client = new YoutubeClient();
				Channel channel = await client.Channels.GetAsync(new ChannelId(channelUrl));

				// info.json
				ChannelInfo channelInfo = new ChannelInfo() {
					url = channel.Url,
					id = channel.Id,
					name = channel.Title,
					avatarUrl = channel.LogoUrl,
				};
				onChannelScraped?.Invoke(channelInfo);
				string infoJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{subPath}/{channel.Id}", "info.json", false);
				JsonUtilities.SaveToDisk(channelInfo, new JsonUtilities.SaveToDiskParameters() {
					filePath = infoJsonPath,
					onSave = (JsonUtilities.OperationResponse res) => {
						MLog.Log($"Channel {channelInfo.name} info scraped.");
					}
				});

				// videos.json
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
						channel = video.Author,
						channelId = video.ChannelId,
						uploadDate = video.UploadDate.ToString()
					};
					channelVideos.Add(channelVideo);

					onVideoScraped?.Invoke(channelVideo);
				}
				string videosJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{subPath}/{channel.Id}", "videos.json", false);
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
			return content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		}

		private void Start()
		{
			if(shouldRunOnStart) { Run(); }
		}
	}
}
