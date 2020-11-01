using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Pages;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using Api.DB;
	using Api.DB.Contents.Creators;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class PersonalFeedPageController : FeedPageController
	{
		protected override Node _mainNode => mainFlowMap.personalFeedNode;
		protected override VideoFeedScroll _videoFeed => _personalFeedFlowMap.videoFeed;
		private Section _emptySection => _personalFeedFlowMap.emptySection;
		[Space]
		[SerializeField]
		private PersonalFeedFlowMap _personalFeedFlowMap = null;

		protected override CreatorQuery CreateCreatorQuery(VirtualHoleDBClient client)
		{
			return new CreatorQuery(client, new FindCreatorsRegexSettings {
				searchQueries = new List<string>() { "Watame", "Matsuri", "Haato", "Subaru" }
			});
		}

		protected override async void OnNodeVisit()
		{
			await Task.CompletedTask;
			_emptySection.gameObject.SetActive(false);
			base.OnNodeVisit();

			//await Task.CompletedTask;
			//_emptySection.gameObject.SetActive(true);
			//await _emptySection.LoadAsync();
		}
	}
}
