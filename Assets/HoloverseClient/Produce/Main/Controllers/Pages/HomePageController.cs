using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data.Common;
	using Api.Data.Contents.Creators;
	using Api.Data.Contents.Videos;

	using Client.UI;
	using Client.Data;

	public class HomePageController : MonoBehaviour
	{
		public class ContentInfo
		{
			public string type;
			public FindSettings<Video> query = null;
		}

		[SerializeField]
		private int _cellRemainingThreashold = 7;

		[Space]
		[SerializeField]
		private Node _homeNode = null;

		[SerializeField]
		private HoloverseDataClientObject _client = null;

		[Header("References")]
		[SerializeField]
		private FlowTree _flowTree = null;

		[SerializeField]
		private Page _homePage = null;

		[SerializeField]
		private Section _videoFeedSection = null;

		[SerializeField]
		private VideoFeed _videoFeed = null;

		private List<VideoScrollRectCellData> _videoFeedCells = new List<VideoScrollRectCellData>();
		private VideoFeedData _videoFeedData = null;
		private bool _isLoading = false;

		private async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			_homePage.Prewarm();

			if(_videoFeedData == null) {
				_videoFeedData = new VideoFeedData(
					_client.client,
					new FindCreatorsSettings {
						isCheckForAffiliations = true,
						affiliations = new List<string>() {
							"hololiveProduction"
						},
						batchSize = 100
					});

				await _videoFeedData.InitializeAsync();

				_videoFeed.dropdown.AddOptions(_videoFeedData.feeds.Select(f => f.name).ToList());
			}

			ClearFeed();
			_videoFeed.dropdown.value = 0;
			_videoFeedSection.LoadContentTaskFactory += LoadContentAsync;

			await _homePage.LoadAsync(cancellationToken);
		}

		private async Task LoadContentAsync(CancellationToken cancellationToken = default)
		{
			if(_isLoading) { return; }
			_isLoading = true;

			bool isFirstLoad = _videoFeedCells.Count <= 0;

			VideoFeedData.Feed feed = _videoFeedData.feeds[_videoFeed.dropdown.value];
			_videoFeedCells.AddRange(await PageControllerFactory.ProcessVideoFeed(_videoFeedData, feed));

			_videoFeed.videoScroll.UpdateData(_videoFeedCells);

			if(isFirstLoad) {
				_videoFeed.videoScroll.ScrollTo(0f, 0f);

				_videoFeed.videoScroll.OnScrollerPositionChanged += OnScrollerPositionChanged;
				_videoFeed.dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
			}

			_isLoading = false;
		}

		private async Task UnloadContentAsync()
		{
			await Task.CompletedTask;
			ClearFeed();

			_videoFeed.videoScroll.OnScrollerPositionChanged -= OnScrollerPositionChanged;
			_videoFeed.dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
		}

		private void ScrollToTop()
		{
			_videoFeed.videoScroll.ScrollTo(0f, 1f);
		}

		private void ClearFeed()
		{
			_videoFeedCells.Clear();
			_videoFeed.videoScroll.UpdateData(_videoFeedCells);
		}

		private async void OnHomeVisit()
		{
			await InitializeAsync();
		}

		private void OnHomeLeave()
		{

		}

		private void OnAttemptSetNodeSameAsCurrent(Node node)
		{
			if(node != _homeNode) { return; }
			ScrollToTop();
		}

		private async void OnScrollerPositionChanged(float position)
		{
			if(position >= _videoFeed.videoScroll.itemCount - _cellRemainingThreashold) {
				await LoadContentAsync();
			}
		}

		private async void OnDropdownValueChanged(int value)
		{
			await _homePage.RefreshAsync();
		}

		private void OnEnable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent += OnAttemptSetNodeSameAsCurrent;

			_homeNode.OnVisit += OnHomeVisit;
			_homeNode.OnLeave += OnHomeLeave;
		}

		private void OnDisable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent -= OnAttemptSetNodeSameAsCurrent;

			_homeNode.OnVisit -= OnHomeVisit;
			_homeNode.OnLeave -= OnHomeLeave;
		}
	}
}
