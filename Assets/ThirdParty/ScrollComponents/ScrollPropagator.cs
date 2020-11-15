using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	/// Source: https://forum.unity.com/threads/nested-scrollrect.268551/page-2#post-4214161
	public class ScrollPropagator :
		MonoBehaviour, IInitializePotentialDragHandler,
		IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		private bool _shouldRouteToParent = false;

		public IScrollRect scrollRect {
			get {
				if(_scrollRect == null) { _scrollRect = GetComponent<IScrollRect>(); }
				return _scrollRect;
			}
		}
		private IScrollRect _scrollRect = null;

		public new Transform transform
		{
			get {
				if(_transform == null) { _transform = base.transform; }
				return _transform;
			}
		}
		private Transform _transform = null;

		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if(!scrollRect.horizontal && Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y)) {
				_shouldRouteToParent = true;
			} else if(!scrollRect.vertical && Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y)) {
				_shouldRouteToParent = true;
			} else {
				_shouldRouteToParent = false;
			}

			if(_shouldRouteToParent) {
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if(_shouldRouteToParent) {
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if(_shouldRouteToParent) {
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
			}
			_shouldRouteToParent = false;
		}
	}
}
