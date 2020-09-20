using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Newtonsoft.Json;
using Holoverse.Backend.YouTube;
using Holoverse.Client.UI;
using Euphoria.Backend;

namespace Holoverse.Client
{
	public class VideoLoaderTest : MonoBehaviour
	{
		public VideoScrollView videoScrollView = null;
		public TextAsset videoSource = null;
		public int videoAmount = 50;

		private async Task LoadVideos()
		{
			MLog.Log($"[{nameof(VideoLoaderTest)}] Loading of videos started");

			List<VideoScrollViewCellData> videoScrollViewCellData = new List<VideoScrollViewCellData>();
			foreach(VideoInfo videoInfo in LoadVideoInfo(videoSource.text, videoAmount)) {
				videoScrollViewCellData.Add(new VideoScrollViewCellData {
					thumbnail = await UnityWebRequestUtilities.SendImageRequestAsync(videoInfo.mediumResThumbnailUrl),
					title = videoInfo.title,
					channel = videoInfo.channel,
					onClick = () => Application.OpenURL(videoInfo.url)
				});
				videoScrollView.UpdateData(videoScrollViewCellData);
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
