using UnityEngine;

namespace Midnight.Pages
{
	[CreateAssetMenu(menuName = "Midnight/Objects/PagesSettings")]
	public class PagesSettings : SingletonObject<PagesSettings>
	{
		public static GameObject defaultLoadingIndicatorDisplay => _instance._defaultLoadingIndicatorDisplay;
		public static GameObject defaultGenericErrorDisplay => _instance._defaultGenericErrorDisplay;
		public static GameObject defaultInternetErrorDisplay => _instance._defaultInternetErrorDisplay;
		public static GameObject defaultAppIncompatibleErrorDisplay => _instance._defaultAppIncompatibleErrorDisplay;

		[SerializeField]
		private GameObject _defaultLoadingIndicatorDisplay = null;

		[SerializeField]
		private GameObject _defaultGenericErrorDisplay = null;

		[SerializeField]
		private GameObject _defaultInternetErrorDisplay = null;
		
		[SerializeField]
		private GameObject _defaultAppIncompatibleErrorDisplay = null;
	}
}
