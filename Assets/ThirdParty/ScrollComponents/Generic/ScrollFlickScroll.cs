using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	public class ScrollFlickScroll : MonoBehaviour, IEndDragHandler
	{
		public float scrollMultiplierApex = 2.5f;
		public int flickCountToReachApex = 5;

		private Vector2 _lastVelocity = Vector2.zero;
		private Vector2 _flickCounter = Vector2.zero;

		private float _flickValidityTime = 1.5f;
		private float _flickTimer = 0f;

		public IScrollRect scrollRect 
		{
			get {
				if(_scrollRect == null) { _scrollRect = GetComponent<IScrollRect>(); }
				return _scrollRect;
			}
		}
		private IScrollRect _scrollRect = null;

		public void OnEndDrag(PointerEventData eventData)
		{
			float flickRegisterTime = .13f;

			if(scrollRect.dragTime <= flickRegisterTime) {
				_flickTimer = 0f;

				Vector2 flickDirection = new Vector2(
					scrollRect.horizontal ? -eventData.delta.x : 0f,
					scrollRect.vertical ? eventData.delta.y : 0f); ;

				if(flickDirection != Vector2.zero) {
					bool isHorizontalSameDirection = Mathf.Sign(_lastVelocity.x) == Mathf.Sign(flickDirection.x);
					bool isVerticalSameDirection = Mathf.Sign(_lastVelocity.y) == Mathf.Sign(flickDirection.y);

					_flickCounter.x = Mathf.Clamp(!isHorizontalSameDirection ? 1 : _flickCounter.x + 1, 0, flickCountToReachApex);
					_flickCounter.y = Mathf.Clamp(!isVerticalSameDirection ? 1 : _flickCounter.y + 1, 0, flickCountToReachApex);

					float xSine = Mathf.InverseLerp(0f, flickCountToReachApex, _flickCounter.x);
					float ySine = Mathf.InverseLerp(0f, flickCountToReachApex, _flickCounter.y);

					Vector2 finalVelocity = scrollRect.velocity;
					finalVelocity.x *= scrollMultiplierApex * EaseOutCirc(xSine);
					finalVelocity.y *= scrollMultiplierApex * EaseOutCirc(ySine);
					scrollRect.velocity = finalVelocity;
					_lastVelocity = finalVelocity;
				}
			}

			// https://easings.net/#easeOutCirc
			//float EaseInSine(float x) => 1f - Mathf.Cos(x * Mathf.PI / 2f);
			float EaseOutCirc(float x) => Mathf.Sqrt(1f - Mathf.Pow(x - 1, 2));
		}

		private void FixedUpdate()
		{
			if(_flickTimer > _flickValidityTime) {_flickCounter = Vector2.zero; } 
			else { _flickTimer += Time.fixedDeltaTime; }
		}
	}
}
