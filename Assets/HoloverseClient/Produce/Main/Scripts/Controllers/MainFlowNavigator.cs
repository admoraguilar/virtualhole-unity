using System;
using System.Collections.Generic;
using UnityEngine;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Client.UI;

	public class MainFlowNavigator : MonoBehaviour
	{
		[Serializable]
		public class NavigationItem
		{
			public Sprite sprite = null;
			public string text = null;
			public Node node = null;
		}

		[Header("Navigations")]
		[SerializeField]
		private NavigationItem[] _navigationItems = null;

		[Header("References")]
		[SerializeField]
		private FlowTree _flowTree = null;
		
		[SerializeField]
		private NavigationBar _navigationBar = null;

		private void Start()
		{
			List<NavigationBar.ItemData> itemData = new List<NavigationBar.ItemData>();
			foreach(NavigationItem item in _navigationItems) {
				itemData.Add(new NavigationBar.ItemData {
					sprite = item.sprite,
					text = item.text,
					onClick = item.node.Set
				});
			}
			_navigationBar.UpdateEntries(itemData);

			_navigationBar.backButton.onClick.AddListener(_flowTree.Backward);
		}
	}
}
