using UnityEngine;
using VirtualHole.Client.UI;

namespace VirtualHole.Client.ComponentMaps
{
	public class CreatorPageFlowMap : MonoBehaviour
	{
		public CreatorView creatorView => _creatorView;
		[SerializeField]
		private CreatorView _creatorView = null;
	}
}
