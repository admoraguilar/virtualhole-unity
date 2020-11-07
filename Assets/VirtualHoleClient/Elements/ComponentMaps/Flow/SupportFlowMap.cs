using UnityEngine;

namespace VirtualHole.Client.ComponentMaps
{
	using Client.UI;

	public class SupportFlowMap : MonoBehaviour
	{
		public SupportView supportView => _supportView;
		[SerializeField]
		private SupportView _supportView = null;
	}
}
