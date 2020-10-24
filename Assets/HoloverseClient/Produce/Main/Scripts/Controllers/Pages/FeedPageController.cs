using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data;

	using Client.UI;
	using Client.Data;

	public abstract class FeedPageController : MonoBehaviour
	{
		[SerializeField]
		private Node _node = null;

		protected HoloverseDataClient client => _client.client;
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		[Space]
		[SerializeField]
		protected int _cellRemainingThreshold = 7;

		[SerializeField]
		protected Node _optionsNode = null;

		[Header("References")]
		[SerializeField]
		private FlowTree _flowTree = null;

		[SerializeField]
		private Page _page = null;

		[SerializeField]
		private Section _videoFeedSection = null;

		[SerializeField]
		private VideoFeed _videoFeed = null;

		protected List<VideoScrollRectCellData> _videoFeedCells { get; private set; } = new List<VideoScrollRectCellData>();
		protected VideoFeedData _videoFeedData { get; private set; } = null;
		protected bool _isLoading { get; private set; } = false;

		protected CancellationTokenSource _cts { get; private set; } = new CancellationTokenSource();

		protected abstract VideoFeedData CreateVideoFeedData();

		protected async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			_page.Prewarm();

			if(_videoFeedData == null) {
				_videoFeedData = CreateVideoFeedData();
				await _videoFeedData.InitializeAsync();

				_videoFeed.dropdown.ClearOptions();
				_videoFeed.dropdown.AddOptions(_videoFeedData.feeds.Select(f => f.name).ToList());
			}

			ClearFeed();
			_videoFeed.dropdown.value = 0;

			await _page.LoadAsync(cancellationToken);
		}

		protected async Task LoadContentAsync(CancellationToken cancellationToken = default)
		{
			VideoFeedData.Feed feed = _videoFeedData.feeds[_videoFeed.dropdown.value];

			if(_isLoading || feed.isDone) { return; }
			_isLoading = true;

			bool isFirstLoad = _videoFeedCells.Count <= 0;

			IEnumerable<VideoScrollRectCellData> cellData = await PageControllerFactory.CreateCellData(_videoFeedData, feed);
			foreach(VideoScrollRectCellData cell in cellData) {
				cell.onOptionsClick = () => {
					if(_optionsNode != null) { _optionsNode.Push(); }
				};
			}

			_videoFeedCells.AddRange(cellData);
			_videoFeed.videoScroll.UpdateData(_videoFeedCells);

			if(isFirstLoad) {
				_videoFeed.videoScroll.ScrollTo(0f, 0f);
			}

			_isLoading = false;
		}

		protected async Task UnloadContentAsync()
		{
			await Task.CompletedTask;

			ClearFeed();
			CancelToken();
		}

		protected void ClearFeed()
		{
			_videoFeedCells.Clear();
			_videoFeed.videoScroll.UpdateData(_videoFeedCells);

			VideoFeedData.Feed feed = _videoFeedData.feeds[_videoFeed.dropdown.value];
			feed.Clear();
		}

		protected void ScrollToTop()
		{
			_videoFeed.videoScroll.ScrollTo(0f, 1f);
		}

		protected CancellationTokenSource CreateCancelToken()
		{
			CancelToken();
			return _cts = new CancellationTokenSource();
		}

		protected void CancelToken()
		{
			if(_cts != null) {
				_cts.Cancel();
				_cts.Dispose();
				_cts = null;
			}
		}

		private async void OnNodeVisit()
		{
			await InitializeAsync(CreateCancelToken().Token);
		}

		private async void OnNodeLeave()
		{
			await _page.UnloadAsync();
		}

		private void OnAttemptSetSameNodeAsCurrent(Node node)
		{
			if(node != _node) { return; }
			ScrollToTop();
		}

		private async void OnScrollerPositionChanged(float position)
		{
			if(position >= _videoFeed.videoScroll.itemCount - _cellRemainingThreshold) {
				await LoadContentAsync();
			}
		}

		private async void OnDropdownValueChanged(int value)
		{
			ClearFeed();
			await _page.RefreshAsync();
		}

		private void OnEnable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent += OnAttemptSetSameNodeAsCurrent;

			_node.OnVisit += OnNodeVisit;
			_node.OnLeave += OnNodeLeave;

			_videoFeedSection.LoadContentTaskFactory += LoadContentAsync;
			_videoFeedSection.UnloadContentTaskFactory += UnloadContentAsync;

			_videoFeed.videoScroll.OnScrollerPositionChanged += OnScrollerPositionChanged;
			_videoFeed.dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
		}

		private void OnDisable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent -= OnAttemptSetSameNodeAsCurrent;

			_node.OnVisit -= OnNodeVisit;
			_node.OnLeave -= OnNodeLeave;

			_videoFeedSection.LoadContentTaskFactory -= LoadContentAsync;
			_videoFeedSection.UnloadContentTaskFactory -= UnloadContentAsync;

			_videoFeed.videoScroll.OnScrollerPositionChanged -= OnScrollerPositionChanged;
			_videoFeed.dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
		}
	}
}
