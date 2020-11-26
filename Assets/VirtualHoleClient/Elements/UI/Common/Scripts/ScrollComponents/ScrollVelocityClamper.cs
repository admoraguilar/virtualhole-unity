
namespace UnityEngine.UI
{
	public class ScrollVelocityClamper : MonoBehaviour
	{
		public float deadzone = 400f;

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
			Vector2 velocity = scrollRect.velocity;
			if(Mathf.Abs(velocity.x) <= deadzone) { velocity.x = 0f; }
			if(Mathf.Abs(velocity.y) <= deadzone) { velocity.y = 0f; }
			scrollRect.velocity = velocity;
		}
	}
}
