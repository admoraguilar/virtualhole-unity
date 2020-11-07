using System.Collections.Generic;
using UnityEngine;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using Api.DB.Contents.Creators;
	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class SearchPageController : MonoBehaviour
	{
		private Node _searchNode => _mainFlowMap.searchNode;
		private Node _creatorPageNode => _mainFlowMap.creatorPageNode;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private SearchView _searchView => _mainFlowSearchMap.searchView;
		[SerializeField]
		private SearchFlowMap _mainFlowSearchMap = null;

		private CreatorQuery CreatorQueryFactory(string searchString)
		{
			return new CreatorQuery(
				new FindCreatorsRegexSettings {
					searchQueries = new List<string>() { searchString },
					isCheckSocialName = false,
					isCheckForAffiliations = true,
					affiliations = new List<string>() { "hololiveProduction" }
				});
		}

		private async void OnNodeVisit()
		{
			_searchView.creatorQueryFactory = CreatorQueryFactory;
			await _searchView.InitializeAsync();	
		}

		private void OnCellDataCreated(CreatorScrollCellData cellData)
		{
			cellData.onClick += () => {
				Selection.instance.creatorDTO = cellData.creatorDTO;
				_creatorPageNode.Set();
			};
		}

		private void OnEnable()
		{
			_searchNode.OnVisit += OnNodeVisit;

			_searchView.OnCellDataCreated += OnCellDataCreated;
		}

		private void OnDisable()
		{
			_searchNode.OnVisit -= OnNodeVisit;

			_searchView.OnCellDataCreated -= OnCellDataCreated;
		}
	}
}
