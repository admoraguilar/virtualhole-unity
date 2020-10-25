using UnityEngine;
using Midnight;

namespace Holoverse.Client.Utilities
{
	public class Rotator : MonoBehaviour
	{
		public Vector3 direction = Vector3.zero;
		public float speed = 5f;
		public Space space = Space.Self;

		public new Transform transform => this.GetComponent(ref _transform, () => base.transform);
		private Transform _transform = null;

		private void FixedUpdate()
		{
			transform.Rotate(direction * speed * Time.fixedDeltaTime, space);
		}
	}
}
