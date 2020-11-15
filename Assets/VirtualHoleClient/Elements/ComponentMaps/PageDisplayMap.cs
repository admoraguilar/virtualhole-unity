using UnityEngine;

namespace VirtualHole.Client.ComponentMaps
{
	using Client.Pages;

	public class PageDisplayMap : MonoBehaviour
	{
		public LoadingDisplay loadingDisplay => _loadingDisplay;
		[SerializeField]
		private LoadingDisplay _loadingDisplay = null;

		public GameObject internetErrorDisplay => _internetErrorDisplay;
		[SerializeField]
		private GameObject _internetErrorDisplay = null;

		public GameObject appIncompatibleErrorDisplay => _appIncompatibleErrorDisplay;
		[SerializeField]
		private GameObject _appIncompatibleErrorDisplay = null;

		public GameObject genericErrorDisplay => _genericErrorDisplay;
		[SerializeField]
		private GameObject _genericErrorDisplay = null;
	}
}
