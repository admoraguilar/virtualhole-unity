using System.Collections.Generic;
using UnityEngine;
using FancyScrollView;

namespace Holoverse.Client.UI 
{
	public class VideoScrollView : FancyScrollRect<VideoScrollViewCellData, VideoScrollRectContext>
	{
		protected override float CellSize {
			get {
				RectTransform cellPrefabRT = CellPrefab.GetComponent<RectTransform>();
				return cellPrefabRT.rect.height;
			}
		}
		
		protected override GameObject CellPrefab => _cellPrefab;
		[SerializeField]
		private GameObject _cellPrefab = null;

		public void UpdateData(IList<VideoScrollViewCellData> items)
		{
			UpdateContents(items);
		}
	}
}