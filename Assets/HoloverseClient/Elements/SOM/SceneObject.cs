using UnityEngine;

namespace Midnight.SOM
{
	[DefaultExecutionOrder(-1)]
	public class SceneObject : MonoBehaviour 
	{
		private SceneObjectModel _model = null;

		public new Transform transform => this.GetComponent(ref _transform, () => base.transform);
		private Transform _transform = null;

		private void Awake()
		{
			_model = SceneObjectModel.Get(this);
			_model.Add(this);
		}

		private void OnDestroy()
		{
			_model.Remove(this);
		}
	}
}
