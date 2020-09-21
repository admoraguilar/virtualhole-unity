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

		public TextAsset contentFilterTxt = null;

		[Space]
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

					try {
						await ProcessChannelGroup("Idols", false, idolChannelUrlsTxt);
						await ProcessChannelGroup("Audiences", true, audienceChannelUrlsTxt);
					} catch (Exception e) {
						MLog.LogError(e.Message);
						//break;
					}

					runStopwatch.Stop();
					MLog.LogWarning($"{_debugPrepend} Scraping run finished at: {runStopwatch.Elapsed}");

					DateTime nextRun = _lastRun.AddMinutes(frequencyMinutes);
					while(DateTime.Now < nextRun && !_isStopped) {
						MLog.Log("Cooldown...");
						await Task.Yield();
					}
				}
			}

			async Task ProcessChannelGroup(string header, bool shouldFilterContent, TextAsset channelGroupSource)
			{
				List<ChannelInfo> channels = new List<ChannelInfo>();

				// videos.json
				List<VideoInfo> videos = new List<VideoInfo>();
				IReadOnlyList<string> channelUrls = GetNLSV(channelGroupSource.text);
				await Concurrent.ForEachAsync(
					channelUrls,
					(string url) => {
						return ScrapeChannel(
							header, url, shouldFilterContent,
							(ChannelInfo info) => { channels.Add(info); },
							(VideoInfo video) => { videos.Add(video); }
						);
					},
					5
				);
				videos = videos.OrderByDescending((VideoInfo video) => DateTimeOffset.Parse(video.uploadDate)).ToList();
				string videosJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{header}", "videos.json", PathType.Data);
				JsonUtilities.SaveToDisk(videos, new JsonUtilities.SaveToDiskParameters {
						filePath = videosJsonPath,
						onSave = (JsonUtilities.OperationResponse res) => {
							MLog.Log($"{_debugPrepend} {header} videos scraped.");
						}
					}
				);

				// channels.json
				channels = channels.OrderBy((ChannelInfo info) => info.name).ToList();
				string channelsJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{header}", "channels.json", PathType.Data);
					JsonUtilities.SaveToDisk(channels, new JsonUtilities.SaveToDiskParameters {
						filePath = channelsJsonPath,
						onSave = (JsonUtilities.OperationResponse res) => {
							MLog.Log($"{_debugPrepend} {header} channels scraped.");
						}
					}
				);
			}

			async Task ScrapeChannel(
				string subPath, string channelUrl, bool shouldFilterContent, 
				Action<ChannelInfo> onChannelScraped = null, Action<VideoInfo> onVideoScraped = null)
			{
				// So we can have some form of identification per link
				// easily keep track of which channels we have
				channelUrl = GetCSV(channelUrl)[0];

				YoutubeClient client = new YoutubeClient();
				Channel channel = await client.Channels.GetAsync(new ChannelId(channelUrl));
				MLog.Log($"{_debugPrepend} Init channel scrape: {channel.Title}");

				// info.json
				MLog.Log($"{_debugPrepend} [Start] Channel info scrape: {channel.Title}");
				ChannelInfo channelInfo = new ChannelInfo() {
					url = channel.Url,
					id = channel.Id,
					name = channel.Title,
					avatarUrl = channel.LogoUrl,
				};
				onChannelScraped?.Invoke(channelInfo);
				string infoJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{subPath}/{channel.Id}", "info.json", PathType.Data);
				JsonUtilities.SaveToDisk(channelInfo, new JsonUtilities.SaveToDiskParameters {
						filePath = infoJsonPath,
						onSave = (JsonUtilities.OperationResponse res) => {
							MLog.Log($"{_debugPrepend} Channel {channelInfo.name} info scraped.");
						}
					}
				);

				// videos.json
				MLog.Log($"{_debugPrepend} [Start] Video infos scrape: {channel.Title}");
				List<VideoInfo> videoInfos = new List<VideoInfo>();
				IReadOnlyList<Video> videos = await client.Channels.GetUploadsAsync(channelUrl);
				foreach(Video video in videos) {
					// Filter
					if(shouldFilterContent && contentFilterTxt != null) {
						List<string> keywords = new List<string>(GetNLSV(contentFilterTxt.text));
						
						List<string> sources = new List<string>() {
							video.Id, video.Title,
							video.Author, video.Description,
						};
						sources.AddRange(video.Keywords);

						if(!IsMatch(keywords, sources, out IEnumerable<string> matches)) {
							MLog.Log($"{_debugPrepend} Skipping video: {video.Title}, non-holoverse...");
							continue;
						} else {
							MLog.Log($"{_debugPrepend} Video matched with keywords: [{string.Join(",", matches)}]");
						}
					}

					VideoInfo videoInfo = new VideoInfo() {
						url = video.Url,
						id = video.Id,
						title = video.Title,
						duration = video.Duration.ToString(),
						viewCount = video.Engagement.ViewCount.ToString(),
						mediumResThumbnailUrl = video.Thumbnails.MediumResUrl,
						channel = video.Author,
						channelId = video.ChannelId,
						uploadDate = video.UploadDate.ToString()
					};
					videoInfos.Add(videoInfo);

					MLog.Log($"{_debugPrepend} Scrapped video {videoInfo.title} - {videoInfo.channel}");
					onVideoScraped?.Invoke(videoInfo);
				}
				string videosJsonPath = PathUtilities.CreateDataPath($"HoloverseScraper/{subPath}/{channel.Id}", "videos.json", PathType.Data);
				JsonUtilities.SaveToDisk(videoInfos, new JsonUtilities.SaveToDiskParameters {
						filePath = videosJsonPath,
						onSave = (JsonUtilities.OperationResponse res) => {
							MLog.Log($"{_debugPrepend} Channel {channelInfo.name} videos scraped.");
						}
					}
				);
			}

			bool IsMatch(
				IReadOnlyList<string> keywords, IReadOnlyList<string> sources, 
				out IEnumerable<string> matches)
			{
				matches = sources.Where(item => keywords.Any(n => item.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0));
				return matches.Count() > 0;
			}
		}

		[ContextMenu("Stop")]
		public void Stop()
		{
			MLog.Log($"{_debugPrepend} Scraping stopped");

			_isStopped = true;
		}

		[ContextMenu("Test Run")]
		public void TestRun()
		{
			TaskExt.FireForget(DebugVideo());

			async Task DebugVideo()
			{
				YoutubeClient client = new YoutubeClient();

				//Video video = await client.Videos.GetAsync("https://www.youtube.com/watch?v=ePiRDXav8qo");
				//MLog.Log(video.UploadDate);

				IReadOnlyList<Video> videos = await client.Playlists.GetVideosAsync("https://www.youtube.com/playlist?list=UU1CfXB_kRs3C-zaeTG3oGyg");
				MLog.Log($"Videos count: {videos.Count}");
			}
		}

		private IReadOnlyList<string> GetCSV(string content)
		{
			return content.Split(',');
		}

		private IReadOnlyList<string> GetNLSV(string content)
		{
			return content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		}

		private void Start()
		{
			if(shouldRunOnStart) { Run(); }
		}
	}
}
