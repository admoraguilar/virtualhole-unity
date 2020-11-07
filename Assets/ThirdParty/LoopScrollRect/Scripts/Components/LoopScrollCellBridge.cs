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
			public GameObject instance = null;
			public LoopScrollCellDataProcessor processor = null;

			public RectTransform rectTransform = null;
			public LayoutElement layoutElement = null;
			
			public CanvasGroup canvasGroup = null;
			private float _orgCanvasGroupAlphaValue = 0f;
			private bool _orgCanvasGroupInteractable = false;
			private bool _orgCanvaGroupBlocksRaycastValue = false;

			public void SetActive(bool value)
			{
				if(canvasGroup != null) {
					if(value) {
						canvasGroup.alpha = _orgCanvasGroupAlphaValue;
						canvasGroup.interactable = _orgCanvasGroupInteractable;
						canvasGroup.blocksRaycasts = _orgCanvaGroupBlocksRaycastValue;
					} else {
						_orgCanvasGroupAlphaValue = canvasGroup.alpha;
						_orgCanvasGroupInteractable = canvasGroup.interactable;
						_orgCanvaGroupBlocksRaycastValue = canvasGroup.blocksRaycasts;

						canvasGroup.alpha = 0f;
						canvasGroup.interactable = false;
						canvasGroup.blocksRaycasts = false;
					}
				} else {
					instance.SetActive(value);
				}
			}
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
			foreach(LoopScrollCell cellItem in _cellLookup.Values) {
				cellItem.SetActive(false);
			}

			if(_data == null) { return; }

			Type dataType = _data.GetType();
			if(!_cellLookup.TryGetValue(dataType, out LoopScrollCell cell)) {
				cell = new LoopScrollCell();

				foreach(LoopScrollCellDataProcessor dataProcessor in dataProcessors) {
					if(dataProcessor.dataType != dataType) { continue; }

					cell.processor = dataProcessor;

					cell.instance = Instantiate(cell.processor.prefab, rectTransform, false);
					cell.instance.name = cell.processor.prefab.name;

					cell.rectTransform = cell.instance.GetComponent<RectTransform>();

					if(!cell.instance.TryGetComponent(out cell.layoutElement)) {
						cell.layoutElement.minWidth = 1f;
						cell.layoutElement.minHeight = 1f;
						cell.layoutElement.preferredWidth = cell.rectTransform.sizeDelta.x;
						cell.layoutElement.preferredHeight = cell.rectTransform.sizeDelta.y;
					}

					cell.canvasGroup = cell.instance.GetComponent<CanvasGroup>();

					_cellLookup[dataType] = cell;
				}

				if(cell.processor == null) {
					Debug.LogWarning($"[{nameof(LoopScrollCellBridge)}] No compatible data processor for [{dataType.GetType().Name}]", this);
				}
			}

			if(cell != null && cell.processor != null && 
			   cell.instance != null) {
				cell.processor.ProcessData(cell.instance, _data);
				cell.SetActive(true);

				layoutElement.preferredWidth = cell.layoutElement.preferredWidth;
				layoutElement.preferredHeight = cell.layoutElement.preferredHeight;
			}
		}

		private void Start()
		{
			ScrollCellIndex(_index);
		}
	}
}
