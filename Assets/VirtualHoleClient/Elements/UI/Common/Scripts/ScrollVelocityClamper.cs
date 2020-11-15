using UnityEngine;
using UnityEngine.UI;
using Midnight;

namespace VirtualHole.Client.UI
{
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollVelocityClamper : MonoBehaviour
	{
		public float deadzone = .2f;

		private ScrollRect scrollRect => this.GetComponent(ref _scrollRect, () => GetComponent<ScrollRect>());
		private ScrollRect _scrollRect = null;

		private void FixedUpdate()
		{
			Vector2 velocity = scrollRect.velocity;
			if(Mathf.Abs(velocity.x) <= deadzone) { velocity.x = 0f; }
			if(Mathf.Abs(velocity.y) <= deadzone) { velocity.y = 0f; }
			scrollRect.velocity = velocity;
		}
	}
}
