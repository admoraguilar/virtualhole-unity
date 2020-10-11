using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Web;
using Midnight.Concurrency;

namespace Holoverse.Client
{
	using Api.Data.Common;
	using Api.Data.Contents.Videos;
	using Client.UI;

	public class VideoLoader : MonoBehaviour
	{
		private static string _debugPrepend => $"[{nameof(VideoLoader)}]";

		[SerializeField]
		private HoloverseDataClientObject _client = null;

		public VideoScrollView scrollView = null;
		public int amountPerLoad = 15;
		public int cellDistanceToLoad = 7;
		public bool isLoadOnStart = true;

		private List<VideoScrollViewCellData> _cellData = new List<VideoScrollViewCellData>();
		private bool _isLoading = false;

		private FindResults<Broadcast> _broadcastsResults = null;

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

			_isLoading = false;

			async Task LoadVideosUsingApi()
			{
				using(new StopwatchScope("Getting broadcasts cursor..", "Start", "End")) {
					if(_broadcastsResults == null) {
						_broadcastsResults = await _client.client
							.contents.videos
							.FindVideosAsync(
								new FindCreatorVideosSettings<Broadcast> {
									isBroadcast = true,
									sortMode = FindVideosSettings<Broadcast>.SortMode.BySchedule,
									isSortAscending = false,
								});
					}
				}

				bool canMoveNext = false;
				using(new StopwatchScope("Getting broadcasts data..", "Start", "End")) {
					canMoveNext = await _broadcastsResults.MoveNextAsync();
				}

				if(canMoveNext) {
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
