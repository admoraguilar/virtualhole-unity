using System.Collections.Generic;
using UnityEngine;
using Midnight.Unity.FlowTree;
using VirtualHole.Client.UI;
using VirtualHole.Client.Data;
using VirtualHole.Client.ComponentMaps;
using VirtualHole.APIWrapper.Contents.Creators;

namespace VirtualHole.Client.Controllers
{
	public class CreatorPageController : MonoBehaviour
	{
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

			CreatorDTO creatorDTO = Selection.instance.creatorDTO;
			_creatorView.creatorDTO = creatorDTO;

			List<string> followedCreatorUniversalIds = (await _userDataClient.client.personalization.GetAsync()).followedCreatorUniversalIds;
			if(followedCreatorUniversalIds.Exists(c => c == creatorDTO.raw.universalId)) {
				_creatorView.SetFollowButtonState(true);
			} else {
				_creatorView.SetFollowButtonState(false);
			}

			_creatorView.followButton.onClick.RemoveAllListeners();
			_creatorView.followButton.onClick.AddListener(async () => {
				if(followedCreatorUniversalIds.Exists(c => c == creatorDTO.raw.universalId)) {
					followedCreatorUniversalIds.Remove(creatorDTO.raw.universalId);
					_creatorView.SetFollowButtonState(false);
				} else {
					followedCreatorUniversalIds.Add(creatorDTO.raw.universalId);
					_creatorView.SetFollowButtonState(true);
				}

				await _userDataClient.client.personalization.UpsertAsync();
			});

			_creatorView.feeds.Clear();

			IEnumerable<Creator> creators = new Creator[] { creatorDTO.raw };
			_creatorView.feeds.AddRange(new VideoFeedQuery[] {
				VideoFeedQuery.CreateDiscoverFeed(creators, 4),
				VideoFeedQuery.CreateCommunityFeed(creators, 4),
				VideoFeedQuery.CreateLiveFeed(creators, 4),
				VideoFeedQuery.CreateScheduledFeed(creators, 4)
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
