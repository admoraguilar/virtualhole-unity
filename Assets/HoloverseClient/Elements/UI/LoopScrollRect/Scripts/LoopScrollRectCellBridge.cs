using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Midnight;


namespace Holoverse.Client.UI
{
	[RequireComponent(typeof(LayoutElement))]
	public class LoopScrollRectCellBridge : MonoBehaviour, ILoopScrollIndexReceiver
	{
		public RectTransform[] cellPrefabs = null;

		private Dictionary<Type, ILoopScrollCell> _cellLookup = new Dictionary<Type, ILoopScrollCell>();
		private object _data = null;
		private int _index = 0;

		protected LoopScrollRectCellDataContainer dataContainer =>
			this.GetComponent(ref _dataContainer, () => GetComponentInParent<LoopScrollRectCellDataContainer>());
		private LoopScrollRectCellDataContainer _dataContainer = null;

		protected LayoutElement layoutElement => 
			this.GetComponent(ref _layoutElement, () => GetComponent<LayoutElement>());
		private LayoutElement _layoutElement = null;

		protected RectTransform rectTransform => 
			this.GetComponent(ref _rectTransform, () => GetComponent<RectTransform>());
		private RectTransform _rectTransform = null;

		public void ScrollCellIndex(int index)
		{
			_index = index;
			_data = dataContainer.data[index];
			Refresh();
		}

		private void Refresh()
		{
			foreach(ILoopScrollCell cellItem in _cellLookup.Values) {
				cellItem.rectTrasform.gameObject.SetActive(false);
			}

			if(_data == null) {
				layoutElement.preferredHeight = 0f;
				layoutElement.preferredWidth = 0f;
				rectTransform.sizeDelta = Vector2.zero;
				return;
			}

			Type dataType = _data.GetType();
			if(!_cellLookup.TryGetValue(dataType, out ILoopScrollCell cell)) {
				foreach(RectTransform cellPrefab in cellPrefabs) {
					if(cellPrefab.TryGetComponent(typeof(ILoopScrollCell), out Component c)) {
						cell = (ILoopScrollCell)c;
						if(cell.cellDataType == dataType) {
							_cellLookup[dataType] = cell;
							break;
						} else {
							cell = null;
						}
					}
				}
			}

			if(cell != null) {
				cell.UpdateData(_data);
			}
		}

		private void OnDataUpdated(IReadOnlyList<object> values)
		{
			_data = values[_index];
			Refresh();
		}

		private void Start()
		{
			OnDataUpdated(dataContainer.data);
		}

		private void OnEnable()
		{
			dataContainer.OnDataUpdated += OnDataUpdated;
		}

		private void OnDisable()
		{
			dataContainer.OnDataUpdated -= OnDataUpdated;
		}
	}
}
