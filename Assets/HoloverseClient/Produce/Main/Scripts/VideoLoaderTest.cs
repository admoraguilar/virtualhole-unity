using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Midnight;
using TMPro;
using Newtonsoft.Json;
using Holoverse.Backend.YouTube;
using Euphoria.Backend;

namespace Holoverse.Client
{
	public class VideoLoaderTest : MonoBehaviour
	{
		public TextAsset videoSource = null;
		public int videoAmount = 50;

		public RectTransform scrollViewContent = null;
		public Button prefab = null;

		private List<VideoInfo> _loadedVideos = new List<VideoInfo>();

		private async Task LoadVideos()
		{
			MLog.Log($"[{nameof(VideoLoaderTest)}] Loading of videos started");

			_loadedVideos.AddRange(LoadVideoInfo(videoSource.text, videoAmount));
			foreach(VideoInfo videoInfo in _loadedVideos) {
				Button button = Instantiate(prefab, scrollViewContent, false);
				button.gameObject.SetActive(true);
				button.onClick.AddListener(() => {
					Application.OpenURL(videoInfo.url);
				});

				Image thumbnail = button.transform.Find("Thumbnail").GetComponent<Image>();
				TMP_Text title = button.transform.Find("Title").GetComponent<TMP_Text>();
				TMP_Text channel = button.transform.Find("Channel").GetComponent<TMP_Text>();

				thumbnail.sprite = await UnityWebRequestUtilities.SendImageRequestAsync(videoInfo.mediumResThumbnailUrl);
				title.text = videoInfo.title;
				channel.text = videoInfo.channel;

				LayoutGroup layoutGroup = button.GetComponent<LayoutGroup>();
				LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
			}
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

						while(reader.Read()) {
							if(amount >= 0 && count >= amount) { break; }

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

		private void Start()
		{
			TaskExt.FireForget(LoadVideos());
		}
	}
}
