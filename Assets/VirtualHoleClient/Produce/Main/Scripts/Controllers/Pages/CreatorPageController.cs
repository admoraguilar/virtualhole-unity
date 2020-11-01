using System.Collections.Generic;
using UnityEngine;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using Api.DB.Contents.Creators;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class CreatorPageController : MonoBehaviour
	{
		[SerializeField]
		private VirtualHoleDBClientObject _client = null;
		
		private FlowTree _flowTree => _mainFlowMap.flowTree;
		private Node _creatorPageNode => _mainFlowMap.creatorPageNode;
		private Node _creatorFeedNode => _mainFlowMap.creatorFeedNode;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private CreatorView _creatorView => _mainFlowCreatorPageMap.creatorView;
		[SerializeField]
		private CreatorPageFlowMap _mainFlowCreatorPageMap = null;

		private async void OnCreatorPageVisit()
		{
			await _creatorView.UnloadAsync();

			Creator creator = CreatorCache.creator;
			IEnumerable<Creator> creators = new Creator[] { creator };
			_creatorView.SetData(creator, new VideoFeedQuery[] {
				VideoFeedQuery.CreateDiscoverFeed(_client.client, creators, 4),
				VideoFeedQuery.CreateCommunityFeed(_client.client, creators, 4),
				VideoFeedQuery.CreateLiveFeed(_client.client, creators, 4),
				VideoFeedQuery.CreateScheduledFeed(_client.client, creators, 4)
			});

			await _creatorView.InitializeAsync();
		}

		private void OnAttemptSetSameNodeAsCurrent(Node node)
		{
			if(node != _creatorPageNode) { return; }
			_creatorView.ScrollToTop();
		}

		private void OnVideoPeekScrollProcess(VideoPeekScroll peekScroll)
		{
			peekScroll.optionButton.onClick.RemoveAllListeners();
			peekScroll.optionButton.onClick.AddListener(() => {
				_creatorFeedNode.Set();
			});
		}

		private void OnEnable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent += OnAttemptSetSameNodeAsCurrent;
			_creatorPageNode.OnVisit += OnCreatorPageVisit;
			_creatorView.OnVideoPeekScrollProcess += OnVideoPeekScrollProcess;
		}

		private void OnDisable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent -= OnAttemptSetSameNodeAsCurrent;
			_creatorPageNode.OnVisit -= OnCreatorPageVisit;
			_creatorView.OnVideoPeekScrollProcess -= OnVideoPeekScrollProcess;
		}
	}
}
