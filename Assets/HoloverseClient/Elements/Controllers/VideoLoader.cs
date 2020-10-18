using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Web;
using Midnight.Concurrency;

namespace Holoverse.Client.Controllers
{
	using Api.Data.Common;
	using Api.Data.Contents.Creators;
	using Api.Data.Contents.Videos;
	using Client.UI;

	public class VideoLoader : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		public VideoScrollRect scrollView = null;
		public int amountPerLoad = 15;
		public int cellDistanceToLoad = 7;
		public bool isLoadOnStart = true;

		private List<VideoScrollRectCellData> _cellData = new List<VideoScrollRectCellData>();
		private bool _isLoading = false;

		private List<Creator> _creators = null;
		private FindResults<Broadcast> _broadcastsResults = null;
		private FindResults<Video> _videoResults = null;

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

			MLog.Log(nameof(VideoLoader), "Loading of videos started");
			//await LoadBroadcasts();
			await LoadVideosFromCreator();

			_isLoading = false;

			async Task LoadVideosFromCreator()
			{
				using(new StopwatchScope("Getting creators data..", "Start", "End")) {
					if(_creators == null) {
						_creators = new List<Creator>();

						FindResults<Creator> creators = await _client
							.client.contents
							.creators.FindCreatorsAsync(
								new FindCreatorsSettings {
									isCheckForAffiliations = true,
									affiliations = new List<string> {
										"hololiveJapan"
									}
								});

						while(await creators.MoveNextAsync()) {
							_creators.AddRange(creators.current);
						}
					}
				}

				using(new StopwatchScope("Getting creator related videos data..", "Start", "End")) {
					if(_videoResults == null) {
						_videoResults = await _client.client
							.contents.videos
							.FindVideosAsync(
								new FindCreatorRelatedVideosSettings<Video> {
									creators = _creators,
									sortMode = FindVideosSettings<Video>.SortMode.ByCreationDate,
									isSortAscending = false
								});
					}
				}

				bool canMoveNext = false;
				using(new StopwatchScope("Getting videos data..", "Start", "End")) {
					canMoveNext = await _videoResults.MoveNextAsync();
				}

				if(canMoveNext) {
					foreach(Video video in _videoResults.current) {
						VideoScrollRectCellData cellData = new VideoScrollRectCellData {
							thumbnail = await ImageGetWebRequest.GetAsync(video.thumbnailUrl),
							title = video.title,
							channel = video.creator,
							onClick = () => Application.OpenURL(video.url)
						};
						_cellData.Add(cellData);
					}

					scrollView.UpdateData(_cellData);
				}
			}

			async Task LoadBroadcasts()
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
						VideoScrollRectCellData cellData = new VideoScrollRectCellData {
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
