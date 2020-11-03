﻿using System.Collections.Generic;
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
		private VirtualHoleDBClientObject _dbClient = null;

		[SerializeField]
		private UserDataClientObject _userDataClient = null;

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

			Creator creator = CreatorCache.selectedCreator;
			IEnumerable<Creator> creators = new Creator[] { creator };

			_creatorView.creator = creator;

			List<string> followedCreatorUniversalIds = (await _userDataClient.client.personalization.GetAsync()).followedCreatorUniversalIds;
			if(followedCreatorUniversalIds.Exists(c => c == creator.universalId)) {
				_creatorView.SetFollowButtonState(true);
			} else {
				_creatorView.SetFollowButtonState(false);
			}

			_creatorView.followButton.onClick.RemoveAllListeners();
			_creatorView.followButton.onClick.AddListener(async () => {
				if(followedCreatorUniversalIds.Exists(c => c == creator.universalId)) {
					followedCreatorUniversalIds.Remove(creator.universalId);
					_creatorView.SetFollowButtonState(false);
				} else {
					followedCreatorUniversalIds.Add(creator.universalId);
					_creatorView.SetFollowButtonState(true);
				}

				await _userDataClient.client.personalization.UpsertAsync();
			});

			_creatorView.feeds.Clear();
			_creatorView.feeds.AddRange(new VideoFeedQuery[] {
				VideoFeedQuery.CreateDiscoverFeed(_dbClient.GetClient(), creators, 4),
				VideoFeedQuery.CreateCommunityFeed(_dbClient.GetClient(), creators, 4),
				VideoFeedQuery.CreateLiveFeed(_dbClient.GetClient(), creators, 4),
				VideoFeedQuery.CreateScheduledFeed(_dbClient.GetClient(), creators, 4)
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
