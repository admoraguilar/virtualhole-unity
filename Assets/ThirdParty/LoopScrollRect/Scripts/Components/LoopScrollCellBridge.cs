﻿using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(LayoutElement))]
	public class LoopScrollCellBridge : MonoBehaviour, ILoopScrollIndexReceiver
	{
		private class LoopScrollCell
		{
			public GameObject gameObject = null;
			public LoopScrollCellDataProcessor processor = null;

			public RectTransform rectTransform = null;
			public LayoutElement layoutElement = null;
		}

		public LoopScrollCellDataProcessor[] dataProcessors = null;

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
				if(_layoutElement == null) { _layoutElement = GetComponent<LayoutElement>(); }
				return _layoutElement;
			}
		}
		private LayoutElement _layoutElement = null;

		protected RectTransform rectTransform
		{
			get {
				if(_rectTransform == null) { _rectTransform = GetComponent<RectTransform>(); }
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
			if(_data == null) {
				SetActiveCells(false);
				return;
			}

			Type dataType = _data.GetType();
			if(!_cellLookup.TryGetValue(dataType, out LoopScrollCell cell)) {
				cell = new LoopScrollCell();

				foreach(LoopScrollCellDataProcessor dataProcessor in dataProcessors) {
					if(dataProcessor.dataType != dataType) { continue; }

					cell.processor = dataProcessor;

					cell.gameObject = Instantiate(cell.processor.prefab, rectTransform, false);
					cell.gameObject.name = cell.processor.prefab.name;

					cell.rectTransform = cell.gameObject.GetComponent<RectTransform>();

					if(!cell.gameObject.TryGetComponent(out cell.layoutElement)) {
						cell.layoutElement.minWidth = 1f;
						cell.layoutElement.minHeight = 1f;
						cell.layoutElement.preferredWidth = cell.rectTransform.sizeDelta.x;
						cell.layoutElement.preferredHeight = cell.rectTransform.sizeDelta.y;
					}

					_cellLookup[dataType] = cell;
				}

				if(cell.processor == null) {
					Debug.LogWarning($"[{nameof(LoopScrollCellBridge)}] No compatible data processor for [{dataType.GetType().Name}]", this);
				}
			}

			if(cell != null && cell.processor != null && 
			   cell.gameObject != null) {
				cell.processor.ProcessData(cell.gameObject, _data);
				cell.gameObject.SetActive(true);
				SetActiveCells(false, cell);

				layoutElement.preferredWidth = cell.layoutElement.preferredWidth;
				layoutElement.preferredHeight = cell.layoutElement.preferredHeight;
			} else {
				SetActiveCells(false);
			}
		}

		private void SetActiveCells(bool value, LoopScrollCell cell = null)
		{
			foreach(LoopScrollCell cellItem in _cellLookup.Values) {
				if(cellItem == cell && cell != null) { continue; }
				cellItem.gameObject.SetActive(value);
			}
		}

		private void Start()
		{
			ScrollCellIndex(_index);
		}
	}
}
