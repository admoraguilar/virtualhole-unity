using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Web;
using Midnight.Concurrency;

namespace Holoverse.Client
{
	using Api.Data;
	using Api.Data.Contents.Creators;
	using Api.Data.Contents.Videos;
	using Client.UI;
	using Holoverse.Api.Data.Common;

	public class VideoLoader : MonoBehaviour
	{
		private static string _debugPrepend => $"[{nameof(VideoLoader)}]";

		public VideoScrollView scrollView = null;
		public int amountPerLoad = 15;
		public int cellDistanceToLoad = 7;
		public bool isLoadOnStart = true;

		private List<VideoScrollViewCellData> _cellData = new List<VideoScrollViewCellData>();
		private bool _isLoading = false;

		private HoloverseDataClient _client = null;
		private FindResults<Broadcast> _broadcastsResults = null;

		private Container<Video> _videoSource = null;

		private void OnScrollerPositionChanged(float position)
		{
			if(position >= scrollView.itemCount - cellDistanceToLoad) {
				TaskExt.FireForget(LoadVideos());
			}
		}

		private async Task LoadVideos()
		{
			if(_isLoading) {
				await Task.CompletedTask;
				return; 
			}
			_isLoading = true;

			MLog.Log($"{_debugPrepend} Loading of videos started");

			await LoadVideosUsingApi();
			//await LoadVideosUsingUrl();

			_isLoading = false;

			async Task LoadVideosUsingApi()
			{
				if(_client == null) {
					_client = new HoloverseDataClient(
						"mongodb+srv://<username>:<password>@us-east-1-free.41hlb.mongodb.net/test",
						"holoverse-client",
						"xKxKY4Nd2EBKwWN9");
				}

				if(_broadcastsResults == null) {
					_broadcastsResults = await _client
						.contents.videos
						.FindVideosAsync<Broadcast>(
							new FindVideosFilterSettings {
								isBroadcast = true
							});
				}

				if(await _broadcastsResults.MoveNextAsync()) {
					foreach(Broadcast broadcast in _broadcastsResults.current) {
						VideoScrollViewCellData cellData = new VideoScrollViewCellData {
							thumbnail = await ImageGetWebRequest.GetAsync(broadcast.thumbnailUrl),
							title = broadcast.title,
							channel = broadcast.creator,
							onClick = () => Application.OpenURL(broadcast.url)
						};
						_cellData.Add(cellData);
					}

					scrollView.UpdateData(_cellData);
				}
			}

			async Task LoadVideosUsingUrl()
			{
				if(_videoSource == null) {
					_videoSource = new Container<Video>(
						PathUtilities.CreateDataPath(
							string.Empty, "videos.json",
							PathType.StreamingAssets
						)
					);
				}

				foreach(Video videoInfo in await _videoSource.LoadAsync(amountPerLoad)) {
					_cellData.Add(new VideoScrollViewCellData {
						thumbnail = await ImageGetWebRequest.GetAsync(videoInfo.thumbnailUrl),
						title = videoInfo.title,
						channel = videoInfo.creator,
						onClick = () => Application.OpenURL(videoInfo.url)
					});
					scrollView.UpdateData(_cellData);
				}
			}
		}

		private void Start()
		{
			if(isLoadOnStart) { TaskExt.FireForget(LoadVideos()); }
		}

		private void OnEnable()
		{
			scrollView.OnScrollerPositionChanged += OnScrollerPositionChanged;
		}

		private void OnDisable()
		{
			scrollView.OnScrollerPositionChanged -= OnScrollerPositionChanged;
		}
	}
}
