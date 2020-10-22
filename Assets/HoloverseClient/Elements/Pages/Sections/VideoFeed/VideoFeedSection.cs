using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using Midnight;
using Midnight.Web;
using Midnight.Pages;
using Midnight.Concurrency;

namespace Holoverse.Client.Pages
{
	using Api.Data.Common;
	using Api.Data.Contents.Videos;

	using Client.UI;

	public class VideoFeedSection : Section
	{
		public class ContentInfo
		{
			public string type;
			public FindSettings<Video> query = null;
		}

		[Space]
		[SerializeField]
		private HoloverseDataClientObject _clientObject = null;

		[SerializeField]
		private VideoScrollRect _videoScroll = null;

		public int amountPerLoad = 15;
		public int cellRemainingToLoadMoreCount = 7;

		[Space]
		[SerializeField]
		private TMP_Dropdown _dropdown = null;

		private List<ContentInfo> _contentInfo = new List<ContentInfo>();
		private List<VideoScrollRectCellData> _cellData = new List<VideoScrollRectCellData>();
		private FindSettings<Video> _findSettings = null;
		private FindResults<Video> _findResults = null;

		private Dictionary<string, Sprite> _thumbnailsLookup = new Dictionary<string, Sprite>();
		private CancellationTokenSource _cts = new CancellationTokenSource();
		private bool _isLoading = false;

		public void Initialize(IEnumerable<ContentInfo> contentInfo)
		{
			Assert.IsNotNull(contentInfo);
			Assert.IsTrue(contentInfo.Count() > 0);

			Clear();

			_contentInfo.AddRange(contentInfo);

			_dropdown.ClearOptions();
			_dropdown.AddOptions(contentInfo.Select(c => c.type).ToList());

			SetContent(0);
		}

		public void SetContent(int index)
		{
			_findSettings = _contentInfo[index].query;
		}

		public void ScrollToTop()
		{
			_videoScroll.ScrollTo(0f, 1f);
		}

		public void Clear()
		{
			_cellData.Clear();
			_videoScroll.UpdateData(_cellData);
		}

		protected override async Task LoadContentAsync(CancellationToken cancellationToken = default)
		{
			_isLoading = true;

			Clear();

			_findResults = await _clientObject.client
				.contents.videos
				.FindVideosAsync(_findSettings, cancellationToken);

			_videoScroll.OnScrollerPositionChanged += OnScrollerPositionChanged;
			_dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

			await LoadMoreContentAsync(cancellationToken);

			_isLoading = false;
		}

		private async Task LoadMoreContentAsync(CancellationToken cancellationToken = default)
		{
			if(_findResults == null) { return; }

			_isLoading = true;

			if(!await _findResults.MoveNextAsync(cancellationToken)) {
				MLog.Log(nameof(VideoFeedSection), $"No more videos found!");
				_findResults.Dispose();
				_findResults = null;
				return; 
			}

			await Concurrent.ForEachAsync(_findResults.current.ToList(), LoadThumbnails, 3);

			foreach(Video video in _findResults.current) {
				if(!_thumbnailsLookup.TryGetValue(video.thumbnailUrl, out Sprite thumbnail)) {
					continue;
				}

				// Skip videos without thumbnails, possible reasons for these are
				// they maybe privated or deleted.
				// Mostly observed on scheduled videos or livestreams that are already
				// finished.
				if(thumbnail == null) { continue; }

				VideoScrollRectCellData cellData = new VideoScrollRectCellData {
					thumbnail = thumbnail,
					title = video.title,
					channel = video.creator,
					onClick = () => Application.OpenURL(video.url)
				};
				_cellData.Add(cellData);
			}

			_videoScroll.UpdateData(_cellData);
			_videoScroll.ScrollTo(0f, 0f);

			_isLoading = false;

			async Task LoadThumbnails(Video video)
			{
				if(_thumbnailsLookup.ContainsKey(video.thumbnailUrl)) {
					await Task.CompletedTask;
					return;
				}

				_thumbnailsLookup[video.thumbnailUrl] = await ImageGetWebRequest.GetAsync(video.thumbnailUrl);
			}
		}

		protected override async Task UnloadContentAsync()
		{
			await Task.CompletedTask;
			Clear();

			_videoScroll.OnScrollerPositionChanged -= OnScrollerPositionChanged;
			_dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
		}

		private CancellationTokenSource CreateCancelToken()
		{
			CancelToken();
			return _cts = new CancellationTokenSource();
		}

		private void CancelToken()
		{
			if(_cts != null) {
				_cts.Cancel();
				_cts.Dispose();
				_cts = null;
			}
		}

		private async void OnScrollerPositionChanged(float position)
		{
			if(position >= _videoScroll.itemCount - cellRemainingToLoadMoreCount) {
				if(!_isLoading) { await LoadMoreContentAsync(); }
			}
		}

		private async void OnDropdownValueChanged(int value)
		{
			SetContent(value);
			await RefreshAsync();
		}
	}
}
