using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using Api.DB;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public abstract class FeedPageController : MonoBehaviour
	{
		[SerializeField]
		private VirtualHoleDBClientObject _client = null;

		private FlowTree _flowTree => _mainFlowMap.flowTree;
		private Node _creatorPageNode => _mainFlowMap.creatorPageNode;
		protected MainFlowMap mainFlowMap => _mainFlowMap;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		protected abstract Node _mainNode { get; }
		protected abstract VideoFeedScroll _videoFeed { get; }

		protected abstract CreatorQuery CreateCreatorQuery(VirtualHoleDBClient client);

		private async Task VideoFeedDataFactoryAsync(CancellationToken cancellationToken = default)
		{
			CreatorQuery creatorQuery = CreateCreatorQuery(_client.GetClient());
			await creatorQuery.LoadAsync(cancellationToken);

			_videoFeed.feeds.Clear();
			_videoFeed.feeds.AddRange(new VideoFeedQuery[] {
				VideoFeedQuery.CreateDiscoverFeed(_client.GetClient(), creatorQuery.creatorLookup.Values),
				VideoFeedQuery.CreateCommunityFeed(_client.GetClient(), creatorQuery.creatorLookup.Values),
				VideoFeedQuery.CreateLiveFeed(_client.GetClient(), creatorQuery.creatorLookup.Values),
				VideoFeedQuery.CreateScheduledFeed(_client.GetClient(), creatorQuery.creatorLookup.Values)
			});
		}

		protected virtual async void OnNodeVisit()
		{
			_videoFeed.SetDataAsyncFactory(VideoFeedDataFactoryAsync);
			await _videoFeed.InitializeAsync();
		}

		private void OnCellDataCreated(VideoScrollCellData cellData)
		{
			cellData.onOptionsClick += () => {
				CreatorCache.selectedCreator = CreatorCache.Get(cellData.creatorUniversalId);
				_creatorPageNode.Set();
			};
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
			_videoFeed.OnCellDataCreated += OnCellDataCreated;
		}

		private void OnDisable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent -= OnAttemptSetSameNodeAsCurrent;
			_mainNode.OnVisit -= OnNodeVisit;
			_videoFeed.OnCellDataCreated -= OnCellDataCreated;
		}
	}
}
