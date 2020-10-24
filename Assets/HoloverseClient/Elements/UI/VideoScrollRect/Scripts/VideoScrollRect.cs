using System;
using System.Collections.Generic;
using UnityEngine;
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

		[SerializeField]
		private GameObject _antiMistapImage = null;

		[SerializeField]
		private float _antiMistapTime = .1f;
		private float _antiMistapTimer = 0f;

		public void UpdateData(IList<VideoScrollRectCellData> items)
		{
			UpdateContents(items);
		}

		public void ScrollTo(float position, float duration, Action onComplete = null)
		{
			Scroller.ScrollTo(position, duration, onComplete);
		}

		private void FixedUpdate()
		{
			if(Mathf.Abs(scrollerPosition - _prevScrollerPosition) > 0.1f) {
				OnScrollerPositionChanged?.Invoke(_prevScrollerPosition = scrollerPosition);
				_antiMistapImage?.SetActive(true);
				_antiMistapTimer = 0f;
			} else {
				if(_antiMistapTimer > _antiMistapTime) {
					_antiMistapImage?.SetActive(false);
				} else {
					_antiMistapTimer += Time.fixedDeltaTime;
				}
			}
		}
	}
}