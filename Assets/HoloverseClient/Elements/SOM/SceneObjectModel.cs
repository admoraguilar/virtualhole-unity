using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midnight.SOM
{
	[DefaultExecutionOrder(-1)]
	public class SceneObjectModel : MonoBehaviour
	{
		private Dictionary<Type, Component> _cache = new Dictionary<Type, Component>();
		private List<SceneObject> _sceneObjects = new List<SceneObject>();

		public T GetSceneObjectComponent<T>() where T : Component
		{
			return (T)GetSceneObjectComponent(typeof(T));
		}

		public Component GetSceneObjectComponent(Type type)
		{
			Component result = null;

			if(_cache.TryGetValue(type, out result)) {
				return result;
			}

			foreach(SceneObject sceneObject in _sceneObjects) {
				if(sceneObject.TryGetComponent(type, out result)) {
					_cache[type] = result;
					break;
				}
			}

			return result;
		}

		private void RefreshSceneObjects()
		{
			_cache.Clear();

			_sceneObjects.Clear();
			GetComponentsInChildren(true, _sceneObjects);
		}

		private void Awake()
		{
			RefreshSceneObjects();
		}

		private void OnTransformParentChanged()
		{
			RefreshSceneObjects();
		}

		private void OnTransformChildrenChanged()
		{
			RefreshSceneObjects();
		}
	}
}
