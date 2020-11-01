using UnityEngine;
using Midnight.Pages;

namespace VirtualHole.Client.ComponentMaps
{
	using Client.UI;
	
	public class CreatorPageFlowMap : MonoBehaviour
	{
		public CreatorView creatorView => _creatorView;
		[SerializeField]
		private CreatorView _creatorView = null;
	}
}
