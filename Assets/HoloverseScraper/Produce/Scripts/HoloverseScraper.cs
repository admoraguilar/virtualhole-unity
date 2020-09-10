using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Concurrency;
using Holoverse.Backend.YouTube;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Channels;

namespace Holoverse.Scraper
{
	public class HoloverseScraper : MonoBehaviour
	{
		private static string _debugPrepend => $"{nameof(HoloverseScraper)}";

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
			MLog.Log($"{_debugPrepend} Scraping started");

			_isStopped = false;
			TaskExt.FireForget(RunAsync());

			async Task RunAsync()
			{
				while(!_isStopped) {
					Stopwatch runStopwatch = new Stopwatch();
					runStopwatch.Start();

					_lastRun = DateTime.Now;

					await ProcessChannelUrls("Idols", idolChannelUrlsTxt);
					await ProcessChannelUrls("Audiences", audienceChannelUrlsTxt);

					runStopwatch.Stop();
					MLog.LogWarning($"{_debugPrepend} Scraping run finished at: {runStopwatch.Elapsed}");

					DateTime nextRun = _lastRun.AddMinutes(frequencyMinutes);
					while(DateTime.Now < nextRun && !_isStopped) {
						MLog.Log("Cooldown...");
						await Task.Yield();
					}
				}
			}

			async Task ProcessChannelUrls(string header, TextAsset urlsSource)
			{
				List<ChannelInfo> channels = new List<ChannelInfo>();

				// videos.json
				List<VideoInfo> videos = new List<VideoInfo>();
				IReadOnlyList<string> channelUrls = GetNewLineSeparatedValues(urlsSource.text);
				await Concurrent.ForEachAsync(
					channelUrls,
					(string url) => {
						return ScrapeChannel(
							header, url,
							(ChannelInfo info) => { channels.Add(info); },
							(VideoInfo video) => { videos.Add(video); }
						);
					},
					5
				);
				videos = videos.OrderByDescending((VideoInfo video) => DateTimeOffset.Parse(video.uploadDate)).ToList();
				string videosJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{header}", "videos.json", false);
				JsonUtilities.SaveToDisk(videos, new JsonUtilities.SaveToDiskParameters() {
					filePath = videosJsonPath,
					onSave = (JsonUtilities.OperationResponse res) => {
						MLog.Log($"{_debugPrepend} {header} videos scraped.");
					}
				});

				// channels.json
				channels = channels.OrderBy((ChannelInfo info) => info.name).ToList();
				string channelsJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{header}", "channels.json", false);
				JsonUtilities.SaveToDisk(channels, new JsonUtilities.SaveToDiskParameters() {
					filePath = channelsJsonPath,
					onSave = (JsonUtilities.OperationResponse res) => {
						MLog.Log($"{_debugPrepend} {header} channels scraped.");
					}
				});
			}

			async Task ScrapeChannel(
				string subPath, string channelUrl, Action<ChannelInfo> onChannelScraped = null, 
				Action<VideoInfo> onVideoScraped = null)
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
						MLog.Log($"{_debugPrepend} Channel {channelInfo.name} info scraped.");
					}
				});

				// videos.json
				List<VideoInfo> VideoInfos = new List<VideoInfo>();
				IReadOnlyList<Video> videos = await client.Channels.GetUploadsAsync(channelUrl);
				foreach(Video video in videos) {
					VideoInfo VideoInfo = new VideoInfo() {
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
					VideoInfos.Add(VideoInfo);

					onVideoScraped?.Invoke(VideoInfo);
				}
				string videosJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{subPath}/{channel.Id}", "videos.json", false);
				JsonUtilities.SaveToDisk(VideoInfos, new JsonUtilities.SaveToDiskParameters() {
					filePath = videosJsonPath,
					onSave = (JsonUtilities.OperationResponse res) => {
						MLog.Log($"{_debugPrepend} Channel {channelInfo.name} videos scraped.");
					}
				});
			}
		}

		[ContextMenu("Stop")]
		public void Stop()
		{
			MLog.Log($"{_debugPrepend} Scraping stopped");

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
