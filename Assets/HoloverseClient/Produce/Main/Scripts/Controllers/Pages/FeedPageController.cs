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

		protected CancellationTokenSource _cts { get; private set; } = new CancellationTokenSource();

		protected abstract VideoFeedData CreateVideoFeedData(HoloverseDataClient client);

		private async Task LoadContentAsync(CancellationToken cancellationToken = default)
		{
			if(_videoFeedData == null) {
				VideoFeedData data = CreateVideoFeedData(_client.client);
				if(data == null) { return; }

				await data.InitializeAsync(cancellationToken);

				_videoFeed.dropdown.ClearOptions();
				_videoFeed.dropdown.AddOptions(data.feeds.Select(f => f.name).ToList());

				ClearFeed();
				_videoFeed.dropdown.value = 0;

				_videoFeedData = data;
			}

			cancellationToken.ThrowIfCancellationRequested();

			VideoFeedData.Feed feed = _videoFeedData.feeds[_videoFeed.dropdown.value];
			if(feed.isDone) { return; }

			IEnumerable<VideoScrollRectCellData> cellData = await PageControllerFactory.CreateCellData(
				_videoFeedData, feed, cancellationToken);
			foreach(VideoScrollRectCellData cell in cellData) {
				cell.onOptionsClick = () => { if(_optionsNode != null) { _optionsNode.Push(); } };
			}

			bool isFromTop = _videoFeedCells.Count <= 0;

			_videoFeedCells.AddRange(cellData);
			_videoFeed.videoScroll.UpdateData(_videoFeedCells);

			if(isFromTop) {
				_videoFeed.videoScroll.ScrollTo(0f, 0f);
			}
		}

		private async Task UnloadContentAsync()
		{
			await Task.CompletedTask;
			ClearFeed();
		}

		private void ClearFeed()
		{
			_videoFeedCells.Clear();
			_videoFeed.videoScroll.UpdateData(_videoFeedCells);

			if(_videoFeedData != null && _videoFeedData.feeds.Count > _videoFeed.dropdown.value) {
				VideoFeedData.Feed feed = _videoFeedData.feeds[_videoFeed.dropdown.value];
				feed.Clear();
			}
		}

		private void ScrollToTop()
		{
			_videoFeed.videoScroll.ScrollTo(0f, 1f);
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

		protected virtual async void OnNodeVisit()
		{
			await _page.LoadAsync(CreateCancelToken().Token);
		}

		private async void OnNodeLeave()
		{
			await _page.UnloadAsync();
			CancelToken();
		}

		private void OnAttemptSetSameNodeAsCurrent(Node node)
		{
			if(node != _node) { return; }
			ScrollToTop();
		}

		private async void OnScrollerPositionChanged(float position)
		{
			if(position >= _videoFeed.videoScroll.itemCount - _cellRemainingThreshold) {
				await _videoFeedSection.LoadAsync();
			}
		}

		private async void OnDropdownValueChanged(int value)
		{
			await _page.RefreshAsync(CreateCancelToken().Token);
		}

		private void OnEnable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent += OnAttemptSetSameNodeAsCurrent;

			_node.OnVisit += OnNodeVisit;
			_node.OnLeave += OnNodeLeave;

			_videoFeedSection.LoadTaskFactory += LoadContentAsync;
			_videoFeedSection.UnloadTaskFactory += UnloadContentAsync;

			_videoFeed.videoScroll.OnScrollerPositionChanged += OnScrollerPositionChanged;
			_videoFeed.dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
		}

		private void OnDisable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent -= OnAttemptSetSameNodeAsCurrent;

			_node.OnVisit -= OnNodeVisit;
			_node.OnLeave -= OnNodeLeave;

			_videoFeedSection.LoadTaskFactory -= LoadContentAsync;
			_videoFeedSection.UnloadTaskFactory -= UnloadContentAsync;

			_videoFeed.videoScroll.OnScrollerPositionChanged -= OnScrollerPositionChanged;
			_videoFeed.dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
		}
	}
}
