using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Midnight;

namespace VirtualHole.Client.UI
{
	/// Source: https://forum.unity.com/threads/nested-scrollrect.268551/page-2#post-4214161
	[RequireComponent(typeof(ScrollRect))]
	public class VanillaScrollPropagator :
		MonoBehaviour, IInitializePotentialDragHandler,
		IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		private bool _shouldRouteToParent = false;

		public ScrollRect scrollRect => this.GetComponent(ref _scrollRect, () => GetComponent<ScrollRect>());
		private ScrollRect _scrollRect = null;

		public new Transform transform => this.GetComponent(ref _transform, () => base.transform);
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
