using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Web;
using Midnight.Concurrency;

namespace Holoverse.Client
{
	using Api.Data.YouTube;
	using Client.UI;

	public class VideoLoader : MonoBehaviour
	{
		private static string _debugPrepend => $"[{nameof(VideoLoader)}]";

		public VideoScrollView scrollView = null;
		public int amountPerLoad = 15;
		public int cellDistanceToLoad = 7;
		public bool isLoadOnStart = true;

		private List<VideoScrollViewCellData> _cellData = new List<VideoScrollViewCellData>();
		private Container<Video> _videoSource = null;
		private bool _isLoading = false;

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
					channel = videoInfo.author,
					onClick = () => Application.OpenURL(videoInfo.url)
				});
				scrollView.UpdateData(_cellData);
			}

			_isLoading = false;
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
