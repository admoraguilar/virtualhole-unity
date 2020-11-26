
namespace UnityEngine.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ScrollAntiMistap : MonoBehaviour
	{
		[SerializeField]
		private float _antiMistapTime = .1f;
		private float _antiMistapTimer = 0f;

		[SerializeField]
		private float _requiredVelocity = 100f;

		public CanvasGroup canvasGroup
		{
			get {
				if(_canvasGroup == null) { _canvasGroup = GetComponent<CanvasGroup>(); }
				return _canvasGroup;
			}
		}
		private CanvasGroup _canvasGroup = null;

		public IScrollRect scrollRect
		{
			get {
				if(_scrollRect == null) { _scrollRect = GetComponent<IScrollRect>(); }
				return _scrollRect;
			}
		}
		private IScrollRect _scrollRect = null;

		private void FixedUpdate()
		{
			if(Mathf.Abs(scrollRect.velocity.x) > _requiredVelocity || 
			   Mathf.Abs(scrollRect.velocity.y) > _requiredVelocity) {
				canvasGroup.interactable = false;
				_antiMistapTimer = 0f;
			} else {
				if(_antiMistapTimer > _antiMistapTime) {
					canvasGroup.interactable = true;
				} else {
					_antiMistapTimer += Time.fixedDeltaTime;
				}
			}
		}
	}
}
