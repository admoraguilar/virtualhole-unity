using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[RequireComponent(typeof(LoopScrollRect))]
	public class LoopScrollCellDataContainer : MonoBehaviour
	{
		private class UpdateAction
		{
			public List<object> data = new List<object>();
			public Action action = null;
		}

		public IReadOnlyList<object> data => _data;
		private List<object> _data = new List<object>();

		private UpdateAction _updateAction = new UpdateAction();

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
			// NOTES: Oct 31, 2020
			// * Seems like the main source of weird calculation on LoopScrollRect
			// actually stems from the Content having a 0 width or 0 height, for either
			// horizontal or vertical version.
			// Adding a LayoutElement that gives the Content at least 1 width or height
			// eliminated the problem.

			_updateAction.data.Clear();
			if(values != null) { _updateAction.data.AddRange(values); }
			_updateAction.action = UpdateAction;

			ProcessUpdateActions();

			void UpdateAction()
			{
				_data.Clear();
				_data.AddRange(_updateAction.data);

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

					//// Hack the scroll rect to have a very tiny bit of movement to
					//// force it to update its cells
					//loopScrollRect.verticalNormalizedPosition += Mathf.Epsilon;
					//loopScrollRect.horizontalNormalizedPosition += Mathf.Epsilon;
				}

				//if(!wasZeroOrLess) {
				//	// NOTES: Still works fine even if the cells are not refreshed?
				//	loopScrollRect.RefreshCells();
				//}
			}
		}

		private void ProcessUpdateActions()
		{
			// NOTES:
			// * The reason this was done is it seems like
			// LoopScrollRect calculations don't work well when
			// being done while an object is disabled
			// 
			// * Also there's a weird thing where resetting
			// Unity's layout and only showing the game view
			// fixes most of the weird things on the scroll rect
			// this might be due to the scene view acting up
			// or something but if this gets on the build
			// then it'll be worth investigating further
			//
			// * There's another weird thing where if you
			// don't do this when the scroll rect is active
			// it'll scroll down a bit for some instances
			if(_updateAction.action == null || !gameObject.activeInHierarchy) { return; }

			_updateAction.action();
			_updateAction.action = null;

			_updateAction.data.Clear();
		}

		private void OnEnable()
		{
			// NOTES:
			// * Another thing to remedy the issue is to only
			// update it when it's finally active and when it
			// is then only update it after a frame so that
			// UI calculations are pretty much done already
			ProcessUpdateActions();
		}
	}
}
