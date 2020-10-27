using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(LayoutElement))]
	public class LoopScrollRectCellBridge : MonoBehaviour, ILoopScrollIndexReceiver
	{
		public RectTransform[] cellPrefabs = null;

		private Dictionary<Type, ILoopScrollCell> _cellLookup = new Dictionary<Type, ILoopScrollCell>();
		private object _data = null;
		private int _index = 0;

		protected LoopScrollRectCellDataContainer dataContainer
		{
			get {
				if(_dataContainer == null) {
					LoopScrollRectCellDataContainer[] cs = GetComponentsInParent<LoopScrollRectCellDataContainer>(true);
					_dataContainer = cs != null && cs.Length > 0 ? cs[0] : null;
				}
				return _dataContainer;
			}
		}
		[SerializeField]
		private LoopScrollRectCellDataContainer _dataContainer = null;

		protected LayoutElement layoutElement
		{
			get {
				if(_layoutElement == null) {
					_layoutElement = GetComponent<LayoutElement>();
				}
				return _layoutElement;
			}
		}
		private LayoutElement _layoutElement = null;

		protected RectTransform rectTransform
		{
			get {
				if(_rectTransform == null) {
					_rectTransform = GetComponent<RectTransform>();
				}
				return _rectTransform;
			}
		}
		private RectTransform _rectTransform = null;

		public void ScrollCellIndex(int index)
		{
			_index = index;

			if(_index >= 0 && dataContainer.data.Count > _index) {
				_data = dataContainer.data[_index];
				Refresh();
			}
		}

		private void Refresh()
		{
			foreach(ILoopScrollCell cellItem in _cellLookup.Values) {
				cellItem.rectTrasform.gameObject.SetActive(false);
			}

			if(_data == null) { 
				return; 
			}

			Type dataType = _data.GetType();
			if(!_cellLookup.TryGetValue(dataType, out ILoopScrollCell cell)) {
				foreach(RectTransform cellPrefab in cellPrefabs) {
					if(cellPrefab.TryGetComponent(typeof(ILoopScrollCell), out Component c)) {
						cell = (ILoopScrollCell)c;
						if(cell.cellDataType == dataType) {
							Component newCell = Instantiate(c, rectTransform, false);
							newCell.name = c.name;

							cell = (ILoopScrollCell)newCell;
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
				cell.rectTrasform.gameObject.SetActive(true);

				layoutElement.preferredWidth = cell.layoutElement.preferredWidth;
				layoutElement.preferredHeight = cell.layoutElement.preferredHeight;
			}
		}

		private void Start()
		{
			_data = dataContainer.data[_index];
			Refresh();
		}
	}
}
