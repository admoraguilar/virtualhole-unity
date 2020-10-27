
namespace UnityEngine.UI
{
	[RequireComponent(typeof(LoopScrollRect))]
	public class LoopScrollAntiMistap : MonoBehaviour
	{
		[SerializeField]
		private GameObject _antiMistapImage = null;

		[SerializeField]
		private float _antiMistapTime = .1f;
		private float _antiMistapTimer = 0f;

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

		private void FixedUpdate()
		{
			if(Mathf.Abs(loopScrollRect.velocity.x) > 0.1f || 
			   Mathf.Abs(loopScrollRect.velocity.y) > 0.1f) {
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
