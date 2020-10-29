using Midnight;
using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(LayoutElement))]
	public class LoopScrollCellBridge : MonoBehaviour, ILoopScrollIndexReceiver
	{
		private class LoopScrollCell
		{
			public ILoopScrollCell cell = null;
			public RectTransform rectTransform = null;
			public LayoutElement layoutElement = null;
		}

		public RectTransform[] cellPrefabs = null;

		private Dictionary<Type, LoopScrollCell> _cellLookup = new Dictionary<Type, LoopScrollCell>();
		private object _data = null;
		private int _index = 0;

		protected LoopScrollCellDataContainer dataContainer
		{
			get {
				if(_dataContainer == null) {
					LoopScrollCellDataContainer[] cs = GetComponentsInParent<LoopScrollCellDataContainer>(true);
					_dataContainer = cs != null && cs.Length > 0 ? cs[0] : null;
				}
				return _dataContainer;
			}
		}
		[SerializeField]
		private LoopScrollCellDataContainer _dataContainer = null;

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
			foreach(LoopScrollCell cellItem in _cellLookup.Values) {
				cellItem.rectTransform.gameObject.SetActive(false);
			}

			if(_data == null) { 
				return; 
			}

			Type dataType = _data.GetType();
			if(!_cellLookup.TryGetValue(dataType, out LoopScrollCell cell)) {
				cell = new LoopScrollCell();

				foreach(RectTransform cellPrefab in cellPrefabs) {
					if(cellPrefab.TryGetComponent(typeof(ILoopScrollCell), out Component c)) {
						cell.cell = (ILoopScrollCell)c;

						if(cell.cell.cellDataType == dataType) {
							Component newCell = Instantiate(c, rectTransform, false);
							newCell.name = c.name;

							if(!newCell.TryGetComponent(out RectTransform rt)) { 
								rt = newCell.AddOrGetComponent<RectTransform>(); 
							}
							
							if(!newCell.TryGetComponent(out LayoutElement le)) { 
								le = newCell.AddOrGetComponent<LayoutElement>();
								le.preferredWidth = rt.sizeDelta.x;
								le.preferredHeight = rt.sizeDelta.y;
							}

							cell.cell = (ILoopScrollCell)newCell;
							cell.rectTransform = rt;
							cell.layoutElement = le;

							_cellLookup[dataType] = cell;
							break;
						} else {
							cell = null;
						}
					}
				}
			}

			if(cell != null) {
				cell.cell.UpdateData(_data);
				cell.rectTransform.gameObject.SetActive(true);

				layoutElement.preferredWidth = cell.layoutElement.preferredWidth;
				layoutElement.preferredHeight = cell.layoutElement.preferredHeight;
			}
		}

		private void Start()
		{
			if(_index >= 0 && dataContainer.data.Count > _index) {
				_data = dataContainer.data[_index];
				Refresh();
			}
		}
	}
}
