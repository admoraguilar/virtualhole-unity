using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.SOM;
using Midnight.Pages;
using Midnight.FlowTree;
using Midnight.Concurrency;

namespace Holoverse.Client.Controllers
{
	using Api.Data;

	using Client.UI;
	using Client.SOM;
	using Client.Data;
	using Midnight;

	public abstract class FeedPageController : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		[Space]
		[SerializeField]
		private int _cellRemainingThreshold = 7;

		private List<VideoScrollRectCellData> _videoFeedCells = new List<VideoScrollRectCellData>();
		private VideoFeedData _videoFeedData = null;

		private CancellationTokenSource _cts = new CancellationTokenSource();

		private SceneObjectModel _som = null;
		private Node _mainNode = null;
		private Node _optionsNode = null;
		private Page _page = null;
		private Section _videoFeedSection = null;
		private VideoFeed _videoFeed = null;

		protected abstract VideoFeedData CreateVideoFeedData(HoloverseDataClient client);

		private async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			_videoFeedData = CreateVideoFeedData(_client.client);
			if(_videoFeedData == null) { return; }
			
			await _videoFeedData.InitializeAsync(cancellationToken);

			_videoFeed.dropdown.ClearOptions();
			_videoFeed.dropdown.AddOptions(_videoFeedData.feeds.Select(f => f.name).ToList());

			ClearFeed();
			_videoFeed.dropdown.value = 0;

			cancellationToken.ThrowIfCancellationRequested();
			await LoadAsync(cancellationToken);
		}

		private async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			if(_videoFeedData == null) { return; }

			VideoFeedData.Feed feed = _videoFeedData.feeds[_videoFeed.dropdown.value];
			if(feed.isDone) { return;  }

			IEnumerable<VideoScrollRectCellData> cellData = await PageControllerFactory.CreateCellData(
				_videoFeedData, feed, cancellationToken);
			foreach(VideoScrollRectCellData cell in cellData) {
				cell.onOptionsClick = () => { if(_optionsNode != null) { _optionsNode.Push(); } };
			}

			bool isFromTop = _videoFeedCells.Count <= 0;

			_videoFeedCells.AddRange(cellData);
			_videoFeed.scroll.UpdateData(_videoFeedCells);

			if(isFromTop) {
				_videoFeed.scroll.ScrollTo(0f, 0f);
			}
		}

		private async Task UnloadAsync()
		{
			await Task.CompletedTask;
			ClearFeed();
		}

		private void ClearFeed()
		{
			_videoFeedCells.Clear();
			_videoFeed.scroll.UpdateData(_videoFeedCells);

			if(_videoFeedData == null) {
				VideoFeedData.Feed feed = _videoFeedData.feeds[_videoFeed.dropdown.value];
				feed.Clear();
			}
		}

		protected virtual async void OnNodeVisit()
		{
			await _page.InitializeAsync();
		}

		private async void OnScrollerPositionChanged(float position)
		{
			if(position >= _videoFeed.scroll.itemCount - _cellRemainingThreshold) {
				await _page.LoadAsync();
			}
		}

		private async void OnDropdownValueChanged(int value)
		{
			ClearFeed();

			_videoFeedSection.SetDisplayActive(_videoFeedSection.GetDisplay(Section.DisplayType.LoadingIndicator), true);
			await _videoFeedSection.LoadAsync();
			_videoFeedSection.SetDisplayActive(_videoFeedSection.GetDisplay(Section.DisplayType.Content), true);
		}

		protected virtual void SetReferences(
			ref Page page, ref Section videoFeedSection,
			ref VideoFeed videoFeed, ref Node mainNode) { }

		private void Awake()
		{
			_som = SceneObjectModel.Get(this);
			_optionsNode = _som.GetCachedComponent<MainFlowMap>().creatorPageNode;

			SetReferences(
				ref _page, ref _videoFeedSection, 
				ref _videoFeed, ref _mainNode);
		}

		private void OnEnable()
		{
			_mainNode.OnVisit += OnNodeVisit;

			_videoFeedSection.InitializeTaskFactory += InitializeAsync;
			_videoFeedSection.LoadTaskFactory += LoadAsync;
			_videoFeedSection.UnloadTaskFactory += UnloadAsync;

			_videoFeed.scroll.OnScrollerPositionChanged += OnScrollerPositionChanged;
			_videoFeed.dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
		}

		private void OnDisable()
		{
			_mainNode.OnVisit -= OnNodeVisit;

			_videoFeedSection.InitializeTaskFactory -= InitializeAsync;
			_videoFeedSection.LoadTaskFactory -= LoadAsync;
			_videoFeedSection.UnloadTaskFactory -= UnloadAsync;

			_videoFeed.scroll.OnScrollerPositionChanged -= OnScrollerPositionChanged;
			_videoFeed.dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
		}
	}
}
