using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Holoverse.Client.UI
{
	[RequireComponent(typeof(LoopScrollRect))]
	public class LoopScrollRectFlickScroll : MonoBehaviour, IEndDragHandler
	{
		public float scrollMultiplierApex = 3f;
		public int flickCountToReachApex = 5;

		private Vector2 _lastVelocity = Vector2.zero;
		private Vector2 _flickCounter = Vector2.zero;

		private float _flickValidityTime = 1.5f;
		private float _flickTimer = 0f;

		public LoopScrollRect loopScrollRect 
		{
			get {
				if(_loopScrollRect == null) { _loopScrollRect = GetComponent<LoopScrollRect>(); }
				return _loopScrollRect;
			}
		}
		private LoopScrollRect _loopScrollRect = null;

		public void OnEndDrag(PointerEventData eventData)
		{
			float flickRegisterTime = .13f;

			if(loopScrollRect.dragTime <= flickRegisterTime) {
				_flickTimer = 0f;

				Vector2 flickDirection = new Vector2(
					loopScrollRect.horizontal ? -eventData.delta.x : 0f,
					loopScrollRect.vertical ? eventData.delta.y : 0f); ;

				if(flickDirection != Vector2.zero) {
					bool isHorizontalSameDirection = Mathf.Sign(_lastVelocity.x) == Mathf.Sign(flickDirection.x);
					bool isVerticalSameDirection = Mathf.Sign(_lastVelocity.y) == Mathf.Sign(flickDirection.y);

					_flickCounter.x = Mathf.Clamp(!isHorizontalSameDirection ? 1 : _flickCounter.x + 1, 0, flickCountToReachApex);
					_flickCounter.y = Mathf.Clamp(!isVerticalSameDirection ? 1 : _flickCounter.y + 1, 0, flickCountToReachApex);

					float xSine = Mathf.InverseLerp(0f, flickCountToReachApex, _flickCounter.x);
					float ySine = Mathf.InverseLerp(0f, flickCountToReachApex, _flickCounter.y);

					Vector2 finalVelocity = loopScrollRect.velocity;
					finalVelocity.x *= scrollMultiplierApex * EaseInSine(xSine);
					finalVelocity.y *= scrollMultiplierApex * EaseInSine(ySine);
					loopScrollRect.velocity = finalVelocity;
					_lastVelocity = finalVelocity;

					//Debug.Log($"Flick detected: {finalVelocity} - ({scrollMultiplierApex * EaseInSine(xSine)}, {scrollMultiplierApex * EaseInSine(ySine)})");
				}
			}

			float EaseInSine(float x) => 1f - Mathf.Cos(x * Mathf.PI / 2f);
		}

		private void FixedUpdate()
		{
			if(_flickTimer > _flickValidityTime) {_flickCounter = Vector2.zero; } 
			else { _flickTimer += Time.fixedDeltaTime; }
		}

		//public void Attempt_OnEndDrag(PointerEventData eventData)
		//{
		//	float flickRegisterTime = .13f;

		//	if(loopScrollRect.dragTime <= flickRegisterTime) {
		//		Vector2 flickDirection = GetFlickDirection();

		//		if(flickDirection != Vector2.zero) {
		//			bool isHorizontalSameDirection = Mathf.Sign(loopScrollRect.velocity.x) == Mathf.Sign(flickDirection.x);
		//			bool isVerticalSameDirection = Mathf.Sign(loopScrollRect.velocity.y) == Mathf.Sign(flickDirection.y);

		//			Vector2 finalVelocity = loopScrollRect.velocity;
		//			finalVelocity.x =
		//				!isHorizontalSameDirection ? CalculateDirectionalFlickVelocity(flickDirection, true, scrollMultiplier)
		//				: finalVelocity.x + CalculateDirectionalFlickVelocity(flickDirection, true, scrollMultiplier);
		//			finalVelocity.y =
		//				!isVerticalSameDirection ? CalculateDirectionalFlickVelocity(flickDirection, false, scrollMultiplier)
		//				: finalVelocity.y + CalculateDirectionalFlickVelocity(flickDirection, false, scrollMultiplier);

		//			loopScrollRect.velocity = finalVelocity;
		//			Debug.Log($"Flick velocity: {loopScrollRect.velocity} - ({CalculateDirectionalFlickVelocity(flickDirection, true, scrollMultiplier)}, {CalculateDirectionalFlickVelocity(flickDirection, false, scrollMultiplier)})");
		//		}

		//		Vector2 GetFlickDirection() => new Vector2(
		//			loopScrollRect.horizontal ? -eventData.delta.x : 0f,
		//			loopScrollRect.vertical ? eventData.delta.y : 0f);

		//		float CalculateDirectionalFlickVelocity(Vector2 direction, bool isHorizontal, float multiplier = 1f)
		//		{
		//			float flickDirectionMinCoefficient = .1f;
		//			float flickDirectionMaxCoefficient = .20f;
		//			float flickDirectionScreenDeltaRatioMin = .5f;
		//			float flickDirectionScreenDeltaRatioMax = 15f;

		//			float flickDirectionAbs = Mathf.Abs(isHorizontal ? direction.x : direction.y);
		//			float flickScreenDeltaRatio = isHorizontal ?
		//				CalculatePercentage(flickDirectionAbs, Screen.width) :
		//				CalculatePercentage(flickDirectionAbs, Screen.height);

		//			float lerp = Mathf.InverseLerp(flickDirectionScreenDeltaRatioMin, flickDirectionScreenDeltaRatioMax, flickScreenDeltaRatio);
		//			float coefficient = Mathf.Lerp(flickDirectionMinCoefficient, flickDirectionMaxCoefficient, lerp);

		//			// 0.4%, 5%, 12%
		//			//Debug.Log($"FlickDirection: {flickDirection} | FlickRatio: {flickScreenDeltaRatio} | Lerp: {lerp} | Coefficient: {coefficient}");
		//			return (isHorizontal ? direction.x : direction.y) * coefficient * multiplier;
		//		}

		//		float CalculatePercentage(float delta, float max) => delta / max * 100f;
		//	}
		//}
	}
}
