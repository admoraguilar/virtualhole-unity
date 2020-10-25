using UnityEngine;
using Midnight.SOM;
using Midnight.FlowTree;

namespace Holoverse.Client.SOM
{
	public class MainFlowMap : SceneObject
	{
		public FlowTree flowTree => _flowTree;
		[SerializeField]
		private FlowTree _flowTree = null;

		public Node homeNode => _homeNode;
		[SerializeField]
		private Node _homeNode = null;

		public Node personalFeedNode => _personalFeedNode;
		[SerializeField]
		private Node _personalFeedNode = null;

		public Node searchNode => _searchNode;
		[SerializeField]
		private Node _searchNode = null;

		public Node supportNode => _supportNode;
		[SerializeField]
		private Node _supportNode = null;

		public Node creatorPageNode => _creatorPageNode;
		[SerializeField]
		private Node _creatorPageNode = null;

		public Node videoOptionsNode => _videoOptionsNode;
		[SerializeField]
		private Node _videoOptionsNode = null;
	}
}
