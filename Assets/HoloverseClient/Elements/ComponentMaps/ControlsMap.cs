using UnityEngine;

namespace Holoverse.Client.ComponentMaps
{
	using Client.UI;

	public class ControlsMap : MonoBehaviour
	{
		public NavigationBar navigationBar => _navigationBar;
		[SerializeField]
		private NavigationBar _navigationBar = null;
	}
}
