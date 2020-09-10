using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Holoverse.Backend.YouTube;
using Newtonsoft.Json;

namespace Holoverse.Scraper
{
	public class TestLoader : MonoBehaviour
	{
		[Header("Single")]
		public TextAsset videosJson = null;

		[Header("Multiple")]
		public TextAsset[] videoJsons = null;

		[Header("Debug")]
		public List<VideoInfo> loadedVideos = new List<VideoInfo>();

		[ContextMenu("Load Single")]
		public void Load()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			loadedVideos.AddRange(LoadVideoInfo(videosJson.text, 50));

			stopwatch.Stop();
			MLog.Log($"Load Single: {stopwatch.Elapsed}");
		}

		[ContextMenu("Load Multiple")]
		public void LoadMultiple()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			foreach(TextAsset videoJson in videoJsons) {
				loadedVideos.AddRange(LoadVideoInfo(videoJson.text, 50));
			}

			stopwatch.Stop();
			MLog.Log($"Load Multiple: {stopwatch.Elapsed}");
		}

		private IEnumerable<VideoInfo> LoadVideoInfo(string json, int amount)
		{
			List<VideoInfo> result = new List<VideoInfo>();

			byte[] byteArray = Encoding.UTF8.GetBytes(json);
			using(MemoryStream ms = new MemoryStream(byteArray)) {
				using(StreamReader sr = new StreamReader(ms)) {
					using(JsonReader reader = new JsonTextReader(sr)) {
						JsonSerializer serializer = new JsonSerializer();
						int count = 0;

						while(reader.Read() && count < amount) {
							if(reader.TokenType == JsonToken.StartObject) {
								VideoInfo video = serializer.Deserialize<VideoInfo>(reader);
								if(video != null) {
									result.Add(video);
									count++;
								}
							}
						}
					}
				}
			}

			return result;
		}
	}
}
