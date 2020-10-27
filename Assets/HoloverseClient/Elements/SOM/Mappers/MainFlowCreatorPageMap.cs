using UnityEngine;
using Midnight.SOM;
using Midnight.Pages;

namespace Holoverse.Client.SOM
{
	using Client.UI;
	
	public class MainFlowCreatorPageMap : SceneObject
	{
		public Page page => _page;
		[SerializeField]
		private Page _page = null;

		public Section creatorPageSection => _creatorPageSection;
		[SerializeField]
		private Section _creatorPageSection = null;

		public CreatorView creatorView => _creatorView;
		[SerializeField]
		private CreatorView _creatorView = null;
	}
}
