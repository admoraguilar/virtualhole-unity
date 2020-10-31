using Holoverse.Client.UI;
using UnityEngine;

namespace Holoverse.Client.ComponentMaps
{
	public class SupportFlowMap : MonoBehaviour
	{
		public SupportView supportView => _supportView;
		[SerializeField]
		private SupportView _supportView = null;
	}
}
