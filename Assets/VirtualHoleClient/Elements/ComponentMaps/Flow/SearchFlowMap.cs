using UnityEngine;
using VirtualHole.Client.UI;

namespace VirtualHole.Client.ComponentMaps
{
	public class SearchFlowMap : MonoBehaviour
	{
		public SearchView searchView => _searchView;
		[SerializeField]
		private SearchView _searchView = null;
	}
}
