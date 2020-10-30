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

			bool wasZeroOrLess = loopScrollRect.totalCount <= 0;

			if(loopScrollRect.totalCount != _data.Count) {
				loopScrollRect.totalCount = _data.Count;

				// NOTES: refilling cells causing some weird behaviours on
				// first fill? (or apparently it seems like it's Unity's fault,
				// I've removed the scene view and only had the game view
				// and the scroll is working fine.
				// loading scene view and game view again after that confirms
				// a fix, remember to reload the editor layout again next time
				if(wasZeroOrLess) { loopScrollRect.RefillCells(); }
				else {
					// Hack the scroll rect to have a very tiny bit of movement to
					// force it to update its cells
					loopScrollRect.verticalNormalizedPosition += 0.0001f;
				}
			}

			if(!wasZeroOrLess) {
				// NOTES: Still works fine even if the cells are not refreshed?
				loopScrollRect.RefreshCells();
			}
		}
	}
}
