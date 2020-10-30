using UnityEngine;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class SearchPageController : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		private Node _searchNode => _mainFlowMap.searchNode;
		private Node _creatorPageNode => _mainFlowMap.creatorPageNode;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private SearchView _searchView => _mainFlowSearchMap.searchView;
		[SerializeField]
		private MainFlowSearchMap _mainFlowSearchMap = null;

		protected void OnNodeVisit()
		{
			_searchView.Initialize(_client.client);
		}

		private void OnCellDataCreated(CreatorScrollCellData cellData)
		{
			cellData.onCellClick += () => {
				CreatorCache.creator = CreatorCache.Get(cellData.creatorId);
				_creatorPageNode.Push();
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
