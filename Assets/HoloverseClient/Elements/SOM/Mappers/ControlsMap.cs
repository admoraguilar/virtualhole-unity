using UnityEngine;
using Midnight.SOM;

namespace Holoverse.Client.SOM
{
	using Client.UI;

	public class ControlsMap : SceneObject
	{
		public NavigationBar navigationBar => _navigationBar;
		[SerializeField]
		private NavigationBar _navigationBar = null;
	}
}
