using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public abstract class FeedPageController : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		private FlowTree _flowTree => _mainFlowMap.flowTree;
		private Node _creatorPageNode => _mainFlowMap.creatorPageNode;
		protected MainFlowMap mainFlowMap => _mainFlowMap;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		protected abstract Node _mainNode { get; }
		protected abstract Page _page { get; }
		protected abstract Section _videoFeedSection { get; }
		protected abstract VideoFeedScroll _videoFeed { get; }

		protected abstract CreatorQuery CreateCreatorQuery(HoloverseDataClient client);

		private async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			CreatorQuery creatorQuery = CreateCreatorQuery(_client.client);
			await creatorQuery.LoadAsync(cancellationToken);

			await _videoFeed.InitializeAsync(
				new VideoFeedQuery[] {
					VideoFeedQuery.CreateDiscoverFeed(_client.client, creatorQuery.creatorLookup.Values),
					VideoFeedQuery.CreateCommunityFeed(_client.client, creatorQuery.creatorLookup.Values),
					VideoFeedQuery.CreateLiveFeed(_client.client, creatorQuery.creatorLookup.Values),
					VideoFeedQuery.CreateScheduledFeed(_client.client, creatorQuery.creatorLookup.Values)
				}, 
				cancellationToken);
		}

		private async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			await _videoFeed.LoadAsync(cancellationToken);
		}

		private async Task UnloadAsync()
		{
			await _videoFeed.UnloadAsync();
		}

		protected virtual async void OnNodeVisit()
		{
			await _page.InitializeAsync();
		}

		private void OnCellDataCreated(VideoScrollCellData cellData)
		{
			cellData.onOptionsClick += () => {
				CreatorCache.creator = CreatorCache.Get(cellData.creatorUniversalId);
				_creatorPageNode.Set();
			};
		}

		private async void OnDropdownValueChanged(int value)
		{
			_videoFeed.ClearFeed();

			_videoFeedSection.SetDisplayActive(_videoFeedSection.GetDisplay(Section.DisplayType.LoadingIndicator), true);
			await _videoFeedSection.LoadAsync();
			_videoFeedSection.SetDisplayActive(_videoFeedSection.GetDisplay(Section.DisplayType.Content), true);
		}

		private async void OnNearScrollEnd()
		{
			await _page.LoadAsync();
		}

		private void OnAttemptSetSameNodeAsCurrent(Node node)
		{
			if(node != _mainNode) { return; }
			_videoFeed.ScrollToTop();
		}

		private void OnEnable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent += OnAttemptSetSameNodeAsCurrent;

			_mainNode.OnVisit += OnNodeVisit;

			_videoFeedSection.InitializeTaskFactory += InitializeAsync;
			_videoFeedSection.LoadTaskFactory += LoadAsync;
			_videoFeedSection.UnloadTaskFactory += UnloadAsync;

			_videoFeed.OnCellDataCreated += OnCellDataCreated;
			_videoFeed.OnDropdownValueChangedCallback += OnDropdownValueChanged;
			_videoFeed.OnNearScrollEnd += OnNearScrollEnd;
		}

		private void OnDisable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent -= OnAttemptSetSameNodeAsCurrent;

			_mainNode.OnVisit -= OnNodeVisit;

			_videoFeedSection.InitializeTaskFactory -= InitializeAsync;
			_videoFeedSection.LoadTaskFactory -= LoadAsync;
			_videoFeedSection.UnloadTaskFactory -= UnloadAsync;

			_videoFeed.OnCellDataCreated -= OnCellDataCreated;
			_videoFeed.OnDropdownValueChangedCallback -= OnDropdownValueChanged;
			_videoFeed.OnNearScrollEnd -= OnNearScrollEnd;
		}
	}
}
