using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using APIWrapper.Contents.Creators;
	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public abstract class FeedPageController : MonoBehaviour
	{
		private FlowTree _flowTree => _mainFlowMap.flowTree;
		private Node _creatorPageNode => _mainFlowMap.creatorPageNode;
		protected MainFlowMap mainFlowMap => _mainFlowMap;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		protected abstract Node _mainNode { get; }
		protected abstract VideoFeedScroll _videoFeed { get; }

		protected abstract CreatorQuery CreateCreatorQuery();

		private async Task VideoFeedDataFactoryAsync(CancellationToken cancellationToken = default)
		{
			CreatorQuery creatorQuery = CreateCreatorQuery();
			IEnumerable<Creator> creators = await creatorQuery.GetRawAsync(cancellationToken);

			_videoFeed.feeds.Clear();
			_videoFeed.feeds.AddRange(new VideoFeedQuery[] {
				VideoFeedQuery.CreateDiscoverFeed(creators),
				VideoFeedQuery.CreateCommunityFeed(creators),
				VideoFeedQuery.CreateLiveFeed(creators),
				VideoFeedQuery.CreateScheduledFeed(creators)
			});
		}

		protected virtual async void OnNodeVisit()
		{
			_videoFeed.SetDataAsyncFactory(VideoFeedDataFactoryAsync);
			await _videoFeed.InitializeAsync();
		}

		protected virtual async void OnNodeLeave()
		{
			await Task.CompletedTask;
		}

		private void OnCellDataCreated(VideoScrollCellData cellData)
		{
			if(!cellData.videoDTO.creatorDTO.raw.affiliations.Contains(CreatorQuery.Affiliation.community)) {
				cellData.onOptionsClick += () => {
					Selection.instance.creatorDTO = cellData.videoDTO.creatorDTO;
					_creatorPageNode.Set();
				};
			}
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
			_mainNode.OnLeave += OnNodeLeave;
			_videoFeed.OnCellDataCreated += OnCellDataCreated;
		}

		private void OnDisable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent -= OnAttemptSetSameNodeAsCurrent;
			_mainNode.OnVisit -= OnNodeVisit;
			_mainNode.OnLeave -= OnNodeLeave;
			_videoFeed.OnCellDataCreated -= OnCellDataCreated;
		}
	}
}
