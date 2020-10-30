using UnityEngine;

namespace Holoverse.Client.ComponentMaps
{
	using Client.UI;

	public class MainFlowSearchMap : MonoBehaviour
	{
		public SearchView searchView => _searchView;
		[SerializeField]
		private SearchView _searchView = null;
	}
}
