using UnityEngine;
using VirtualHole.Client.UI;

namespace VirtualHole.Client.ComponentMaps
{
	public class ControlsMap : MonoBehaviour
	{
		public NavigationBar navigationBar => _navigationBar;
		[SerializeField]
		private NavigationBar _navigationBar = null;
	}
}
