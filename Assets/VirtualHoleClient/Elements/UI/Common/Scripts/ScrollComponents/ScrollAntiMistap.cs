
namespace UnityEngine.UI
{
	public class ScrollAntiMistap : MonoBehaviour
	{
		[SerializeField]
		private GameObject _antiMistapImage = null;

		[SerializeField]
		private float _antiMistapTime = .1f;
		private float _antiMistapTimer = 0f;

		protected IScrollRect scrollRect
		{
			get {
				if(_scrollRect == null) { _scrollRect = GetComponent<IScrollRect>(); }
				return _scrollRect;
			}
		}
		private IScrollRect _scrollRect = null;

		private void FixedUpdate()
		{
			if(Mathf.Abs(scrollRect.velocity.x) > 0.1f || 
			   Mathf.Abs(scrollRect.velocity.y) > 0.1f) {
				_antiMistapImage?.SetActive(true);
				_antiMistapTimer = 0f;
			} else {
				if(_antiMistapTimer > _antiMistapTime) {
					_antiMistapImage?.SetActive(false);
				} else {
					_antiMistapTimer += Time.fixedDeltaTime;
				}
			}
		}
	}
}
