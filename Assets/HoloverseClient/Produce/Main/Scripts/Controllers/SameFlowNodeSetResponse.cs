using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Midnight.SOM;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Client.SOM;

	public class SameFlowNodeSetResponse : MonoBehaviour
	{
		private SceneObjectModel _som = null;
		private FlowTree _flowTree = null;

		private IReadOnlyList<LoopScrollRect> _loopScrollRects = null;
		private IReadOnlyList<ScrollRect> _scrollRects = null;

		private void OnAttemptSetSameNodeAsCurrent(Node node)
		{
			foreach(LoopScrollRect loopScrollRect in _loopScrollRects) {
				if(!node.transform.IsChildOf(loopScrollRect.transform)) { continue; }
				loopScrollRect.ScrollToCell(0, 2000f);
			}

			foreach(ScrollRect scrollRect in _scrollRects) {
				if(!node.transform.IsChildOf(scrollRect.transform)) { continue; }
				Vector3 contentPosition = scrollRect.content.anchoredPosition;
				contentPosition.y = 0f;
				scrollRect.content.anchoredPosition = contentPosition;
			}
		}

		private void Awake()
		{
			_som = SceneObjectModel.Get(this);
			_flowTree = _som.GetCachedComponent<MainFlowMap>().flowTree;

			_loopScrollRects = _som.GetCachedComponents<LoopScrollRect>();
			_scrollRects = _som.GetCachedComponents<ScrollRect>();
		}

		private void OnEnable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent += OnAttemptSetSameNodeAsCurrent;
		}

		private void OnDisable()
		{
			_flowTree.OnAttemptSetSameNodeAsCurrent -= OnAttemptSetSameNodeAsCurrent;
		}
	}
}
