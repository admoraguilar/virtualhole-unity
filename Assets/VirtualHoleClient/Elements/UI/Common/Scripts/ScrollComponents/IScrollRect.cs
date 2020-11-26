using UnityEngine;

namespace UnityEngine.UI
{
	public interface IScrollRect
	{
		bool horizontal { get; }
		bool vertical { get; }

		Vector2 velocity { get; set; }
		float dragTime { get; }
	}
}
