﻿using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data.Contents.Creators;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class CreatorPageController : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;
		
		private FlowTree _flowTree => _mainFlowMap.flowTree;
		private Node _creatorPageNode => _mainFlowMap.creatorPageNode;
		private Node _creatorFeedNode => _mainFlowMap.creatorFeedNode;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private Page _page => _mainFlowCreatorPageMap.page;
		private Section _creatorPageSection => _mainFlowCreatorPageMap.creatorPageSection;
		private CreatorView _creatorView => _mainFlowCreatorPageMap.creatorView;
		[SerializeField]
		private CreatorPageFlowMap _mainFlowCreatorPageMap = null;

		private async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			Creator creator = CreatorCache.creator;
			if(creator == null) { return; }

			IEnumerable<Creator> creators = new Creator[] { creator };
			await _creatorView.LoadCreatorAsync(
				creator,
				new VideoFeedQuery[] {
					VideoFeedQuery.CreateDiscoverFeed(_client.client, creators, 4),
					VideoFeedQuery.CreateCommunityFeed(_client.client, creators, 4),
					VideoFeedQuery.CreateLiveFeed(_client.client, creators, 4),
					VideoFeedQuery.CreateScheduledFeed(_client.client, creators, 4)
				},
				cancellationToken);
		}

		private async Task UnloadAsync()
		{
			await _creatorView.UnloadAsync();
		}

		private async void OnCreatorPageVisit()
		{
			await _page.UnloadAsync();
			await _page.InitializeAsync();
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

			_creatorPageSection.InitializeTaskFactory += InitializeAsync;
			_creatorPageSection.UnloadTaskFactory += UnloadAsync;

			_creatorView.OnVideoPeekScrollProcess += OnVideoPeekScrollProcess;
		}

		private void OnDisable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent -= OnAttemptSetSameNodeAsCurrent;
			_creatorPageNode.OnVisit -= OnCreatorPageVisit;

			_creatorPageSection.InitializeTaskFactory -= InitializeAsync;
			_creatorPageSection.UnloadTaskFactory -= UnloadAsync;

			_creatorView.OnVideoPeekScrollProcess -= OnVideoPeekScrollProcess;
		}
	}
}
