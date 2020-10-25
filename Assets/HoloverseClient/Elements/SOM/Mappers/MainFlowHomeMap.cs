using UnityEngine;
using Midnight.SOM;
using Midnight.Pages;

namespace Holoverse.Client.SOM
{
	using Client.UI;
	
	public class MainFlowHomeMap : SceneObject
	{
		public Page page => _page;
		[SerializeField]
		private Page _page = null;

		public Section videoSection => _videoSection;
		[SerializeField]
		private Section _videoSection = null;

		public VideoFeed videoFeed => _videoFeed;
		[SerializeField]
		private VideoFeed _videoFeed = null;
	}
}
