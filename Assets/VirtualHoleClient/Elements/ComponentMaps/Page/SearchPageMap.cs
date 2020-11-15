using UnityEngine;

namespace VirtualHole.Client.ComponentMaps
{
	using Client.UI;

	public class SearchPageMap : MonoBehaviour
	{
		public SearchView searchView => _searchView;
		[SerializeField]
		private SearchView _searchView = null;
	}
}
