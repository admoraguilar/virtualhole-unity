using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Midnight;

namespace Holoverse.Client.UI
{
	[RequireComponent(typeof(LoopScrollRect))]
	public class LoopScrollRectCellDataContainer : MonoBehaviour
	{
		public IReadOnlyList<object> data => _data;
		private List<object> _data = new List<object>();

		protected LoopScrollRect loopScrollRect => 
			this.GetComponent(ref _loopScrollRect, () => GetComponent<LoopScrollRect>());
		private LoopScrollRect _loopScrollRect = null;

		public void UpdateData(IEnumerable<object> values)
		{
			_data.Clear();
			_data.AddRange(values);

			if(loopScrollRect.totalCount != _data.Count) {
				bool wasZeroOrLess = loopScrollRect.totalCount <= 0;
				loopScrollRect.totalCount = _data.Count;

				if(wasZeroOrLess) { loopScrollRect.RefillCells(); }
				else {loopScrollRect.RefreshCells(); }
			} else {
				loopScrollRect.RefreshCells();
			}
		}
	}
}
