﻿using System;
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
		private GameObject _scrollCover = null;

		public void UpdateData(IList<VideoScrollRectCellData> items)
		{
			UpdateContents(items);
		}

		public void ScrollToTop()
		{
			Scroller.ScrollTo(0, 1f);
		}

		private void Update()
		{
			if(Mathf.Abs(scrollerPosition - _prevScrollerPosition) > 0f) {
				OnScrollerPositionChanged?.Invoke(_prevScrollerPosition = scrollerPosition);
				_scrollCover?.SetActive(true);
			} else {
				_scrollCover?.SetActive(false);
			}
		}
	}
}