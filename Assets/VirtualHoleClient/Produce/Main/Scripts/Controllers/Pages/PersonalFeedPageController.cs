using System.Collections.Generic;
using UnityEngine;
using Midnight.Pages;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using APIWrapper.Contents.Creators;
	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class PersonalFeedPageController : FeedPageController
	{
		[SerializeField]
		private UserDataClientObject _userDataClient = null;

		private List<string> _followedCreatorUniversalIds = new List<string>();

		protected override Node _mainNode => mainFlowMap.personalFeedNode;
		protected override VideoFeedScroll _videoFeed => _personalFeedFlowMap.videoFeed;
		private Section _emptySection => _personalFeedFlowMap.emptySection;
		[Space]
		[SerializeField]
		private PersonalFeedFlowMap _personalFeedFlowMap = null;

		protected override CreatorQuery CreateCreatorQuery()
		{
			return new CreatorQuery(new ListCreatorsRegexRequest {
				searchQueries = _followedCreatorUniversalIds
			});
		}

		protected override async void OnNodeVisit()
		{
			UserDataClient userDataClient = _userDataClient.client;
			_followedCreatorUniversalIds = (await userDataClient.personalization.GetAsync()).followedCreatorUniversalIds;

			if(_followedCreatorUniversalIds.Count > 0) {
				_videoFeed.gameObject.SetActive(true);
				_emptySection.gameObject.SetActive(false);
				base.OnNodeVisit();
			} else {
				_videoFeed.gameObject.SetActive(false);
				_emptySection.gameObject.SetActive(true);
				await _emptySection.LoadAsync();
			}
		}

		protected override async void OnNodeLeave()
		{
			await _videoFeed.UnloadAsync();
			_videoFeed.gameObject.SetActive(false);
			_emptySection.gameObject.SetActive(false);
		}
	}
}
