using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Midnight;

namespace VirtualHole.Client.UI
{
	public class NavigationBar : MonoBehaviour
	{
		[Serializable]
		public class ItemData
		{
			public Sprite sprite = null;
			public string text = string.Empty;
			public Action onClick = null;
		}

		public Image background { get => _background; set => _background = value; }
		[SerializeField]
		private Image _background = null;

		public Button backButton { get => _backButton; set => _backButton = value; }
		[SerializeField]
		private Button _backButton = null;

		public Transform itemContainer { get => _itemContainer; set => _itemContainer = value; }
		[Space]
		[SerializeField]
		private Transform _itemContainer = null;

		public NavigationBarItem template { get => _template; set => _template = value; }
		[SerializeField]
		private NavigationBarItem _template = null;

		private List<NavigationBarItem> _items = new List<NavigationBarItem>();

		public IReadOnlyList<ItemData> itemData => _itemData;
		private List<ItemData> _itemData = new List<ItemData>();

		public new Transform transform => this.GetComponent(ref _transform, () => base.transform);
		private Transform _transform = null;

		public void UpdateEntries(IEnumerable<ItemData> entries)
		{
			_itemData.Clear();
			_itemData.AddRange(entries);

			if(!_template.gameObject.activeSelf) {
				_template.gameObject.SetActive(true);
			}

			int index = 0;
			foreach(ItemData data in _itemData) {
				NavigationBarItem item = GetOrCreatorItem(index++);
				item.name = data.text;

				item.button.onClick.AddListener(data.onClick.Invoke);
				item.image.sprite = data.sprite;
				item.text.text = data.text;
			}

			if(_template.gameObject.activeSelf) {
				_template.gameObject.SetActive(false);
			}
		}

		private NavigationBarItem GetOrCreatorItem(int index)
		{
			while(_items.Count <= index) {
				NavigationBarItem item = Instantiate(template, _itemContainer, false);
				item.name = template.name;
				_items.Add(item);
			}

			return _items[index];
		}
	}
}
