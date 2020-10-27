using System.Collections.Generic;

namespace UnityEngine.UI
{
	[RequireComponent(typeof(LoopScrollRect))]
	public class LoopScrollCellDataContainer : MonoBehaviour
	{
		public IReadOnlyList<object> data => _data;
		private List<object> _data = new List<object>();

		protected LoopScrollRect loopScrollRect
		{
			get {
				if(_loopScrollRect == null) {
					_loopScrollRect = GetComponent<LoopScrollRect>();
				}
				return _loopScrollRect;
			}
		}
		private LoopScrollRect _loopScrollRect = null;

		public void UpdateData(IEnumerable<object> values)
		{
			_data.Clear();
			_data.AddRange(values);

			if(loopScrollRect.totalCount != _data.Count) {
				bool wasZeroOrLess = loopScrollRect.totalCount <= 0;
				loopScrollRect.totalCount = _data.Count;

				if(wasZeroOrLess) { loopScrollRect.RefillCells(); }
				else { loopScrollRect.RefreshCells(); }
			} else {
				loopScrollRect.RefreshCells();
			}
		}
	}
}
