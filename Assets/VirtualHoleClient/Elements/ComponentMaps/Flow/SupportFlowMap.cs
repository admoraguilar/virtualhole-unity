using VirtualHole.Client.UI;
using UnityEngine;

namespace VirtualHole.Client.ComponentMaps
{
	public class SupportFlowMap : MonoBehaviour
	{
		public SupportView supportView => _supportView;
		[SerializeField]
		private SupportView _supportView = null;
	}
}
