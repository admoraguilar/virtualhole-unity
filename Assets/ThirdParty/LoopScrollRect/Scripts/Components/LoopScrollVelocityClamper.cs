
namespace UnityEngine.UI
{
	[RequireComponent(typeof(LoopScrollRect))]
	public class LoopScrollVelocityClamper : MonoBehaviour
	{
		public float deadzone = 300f;

		private LoopScrollRect loopScrollRect
		{
			get {
				if(_loopScrollRect == null) { _loopScrollRect = GetComponent<LoopScrollRect>(); }
				return _loopScrollRect;
			}
		}
		private LoopScrollRect _loopScrollRect = null;

		private void FixedUpdate()
		{
			Vector2 velocity = loopScrollRect.velocity;
			if(Mathf.Abs(velocity.x) <= deadzone) { velocity.x = 0f; }
			if(Mathf.Abs(velocity.y) <= deadzone) { velocity.y = 0f; }
			loopScrollRect.velocity = velocity;
		}
	}
}
