using System;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using FancyScrollView;

namespace Holoverse.Client.UI 
{
	public class VideoScrollRect : FancyScrollRect<VideoScrollRectCellData, VideoScrollRectContext>
	{
		public event Action<float> OnScrollerPositionChanged = delegate { };

		public float itemCount => ItemsSource.Count;

		public float scrollerPosition => Scroller.Position;
		private float _prevScrollerPosition = 0f;

		protected override float CellSize {
			get {
				RectTransform cellPrefabRT = CellPrefab.GetComponent<RectTransform>();
				return cellPrefabRT.rect.height;
			}
		}
		
		protected override GameObject CellPrefab => _cellPrefab;
		[SerializeField]
		private GameObject _cellPrefab = null;

		public CanvasGroup canvasGroup => this.GetComponent(ref _canvasGroup, () => GetComponent<CanvasGroup>());
		private CanvasGroup _canvasGroup = null;

		[SerializeField]
		private float _antiMistapTime = .1f;
		private float _antiMistapTimer = 0f;

		public void UpdateData(IList<VideoScrollRectCellData> items)
		{
			UpdateContents(items);
		}

		public void ScrollToTop()
		{
			Scroller.ScrollTo(0, 1f);
		}

		private void FixedUpdate()
		{
			if(Mathf.Abs(scrollerPosition - _prevScrollerPosition) > 0f) {
				OnScrollerPositionChanged?.Invoke(_prevScrollerPosition = scrollerPosition);
				if(canvasGroup != null) { canvasGroup.interactable = false; }
				_antiMistapTimer = 0f;
			} else {
				if(_antiMistapTimer > _antiMistapTime) {
					if(canvasGroup != null) { canvasGroup.interactable = true; }
				} else {
					_antiMistapTimer += Time.fixedDeltaTime;
				}
			}
		}
	}
}