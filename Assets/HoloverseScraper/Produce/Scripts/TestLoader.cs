using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Holoverse.Scraper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TestLoader : MonoBehaviour
{
	[Header("Single")]
	public TextAsset videosJson = null;

	[Header("Multiple")]
	public TextAsset[] videoJsons = null;

	[Header("Debug")]
	public List<ChannelVideo> loadedVideos = new List<ChannelVideo>();

	[ContextMenu("Load Single")]
	public void Load()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();

		byte[] byteArray = Encoding.UTF8.GetBytes(videosJson.text);
		using(MemoryStream ms = new MemoryStream(byteArray)) {
			using(StreamReader sr = new StreamReader(ms)) {
				using(JsonReader reader = new JsonTextReader(sr)) {
					JsonSerializer serializer = new JsonSerializer();
					int count = 0;

					while(reader.Read() && count < 50) {
						if(reader.TokenType == JsonToken.StartObject) {
							ChannelVideo video = serializer.Deserialize<ChannelVideo>(reader);
							if(video != null) {
								loadedVideos.Add(video);
								count++;
							}
						}
					}
				}
			}
		}

		stopwatch.Stop();
		MLog.Log($"Load Single: {stopwatch.Elapsed}");
	}

	[ContextMenu("Load Multiple")]
	public void LoadMultiple()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();

		foreach(TextAsset videoJson in videoJsons) {
			byte[] byteArray = Encoding.UTF8.GetBytes(videoJson.text);
			using(MemoryStream ms = new MemoryStream(byteArray)) {
				using(StreamReader sr = new StreamReader(ms)) {
					using(JsonReader reader = new JsonTextReader(sr)) {
						JsonSerializer serializer = new JsonSerializer();
						int count = 0;

						while(reader.Read() && count < 50) {
							if(reader.TokenType == JsonToken.StartObject) {
								ChannelVideo video = serializer.Deserialize<ChannelVideo>(reader);
								if(video != null) {
									loadedVideos.Add(video);
									count++;
								}
							}
						}
					}
				}
			}
		}

		stopwatch.Stop();
		MLog.Log($"Load Multiple: {stopwatch.Elapsed}");
	}
}
