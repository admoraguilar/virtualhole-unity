using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollRectUGUIBridge : MonoBehaviour, 
		IScrollRect, IDragHandler, IBeginDragHandler, IEndDragHandler
	{
		public bool horizontal => scrollRect.horizontal;

		public bool vertical => scrollRect.vertical;

		public Vector2 velocity 
		{ 
			get => scrollRect.velocity;
			set => scrollRect.velocity = value; 
		}

		public float dragTime 
		{
			get => _dragTime;
			private set => _dragTime = value;
		}
		[SerializeField]
		private float _dragTime = 0f;

		private bool _isDragging = false;

		public ScrollRect scrollRect
		{
			get {
				if(_scrollRect == null) { _scrollRect = GetComponent<ScrollRect>(); }
				return _scrollRect;
			}
		}

		private ScrollRect _scrollRect = null;

		public void OnDrag(PointerEventData eventData)
		{
			_isDragging = true;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_isDragging = true;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_isDragging = false;
		}

		private void LateUpdate()
		{
			if(_isDragging) { dragTime += Time.deltaTime; } 
			else { dragTime = 0f; }
		}
	}
}
