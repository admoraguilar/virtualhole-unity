using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midnight.SOM
{
	[DefaultExecutionOrder(-2)]
	public class SceneObjectModel : MonoBehaviour
	{
		private static Dictionary<int, List<SceneObjectModel>> _sceneObjectModels = new Dictionary<int, List<SceneObjectModel>>();

		public static SceneObjectModel Get(MonoBehaviour mono)
		{
			return Get(mono.gameObject);
		}

		public static SceneObjectModel Get(GameObject go)
		{
			return Get(go.scene.handle);
		}

		public static SceneObjectModel Get(int sceneHandle)
		{
			SceneObjectModel result = null;

			List<SceneObjectModel> models = GetModelList(sceneHandle);
			if(models.Count > 0) { result = models[0]; }

			return result;
		}

		internal static SceneObjectModel Get(SceneObject sceneObject)
		{
			SceneObjectModel result = null;

			List<SceneObjectModel> models = GetModelList(sceneObject.gameObject.scene.handle);
			foreach(SceneObjectModel model in models) {
				if(sceneObject.transform.IsChildOf(model.transform)) {
					result = model;
					break;
				}
			}

			return result;
		}

		private static void Add(SceneObjectModel model)
		{
			List<SceneObjectModel> models = GetModelList(model.gameObject.scene.handle);
			models.Add(model);
		}

		private static void Remove(SceneObjectModel model)
		{
			List<SceneObjectModel> models = GetModelList(model.gameObject.scene.handle);
			models.Remove(model);
		}

		private static List<SceneObjectModel> GetModelList(int sceneHandle)
		{
			if(!_sceneObjectModels.TryGetValue(sceneHandle, out List<SceneObjectModel> models)) {
				_sceneObjectModels[sceneHandle] = models = new List<SceneObjectModel>();
			}
			return models;
		}

		private Dictionary<Type, List<Component>> _componentCacheLookup = new Dictionary<Type, List<Component>>();
		private List<SceneObject> _sceneObjects = new List<SceneObject>();

		public new Transform transform => this.GetComponent(ref _transform, () => base.transform);
		private Transform _transform = null;

		public T GetSceneObjectComponent<T>() where T : Component
		{
			return (T)GetSceneObjectComponent(typeof(T));
		}

		public Component GetSceneObjectComponent(Type type)
		{
			Component result = null;

			if(_componentCacheLookup.TryGetValue(type, out List<Component> cache)) {
				result = cache[0];
			}

			if(result == null) {
				foreach(SceneObject sceneObject in _sceneObjects) {
					if(sceneObject.TryGetComponent(type, out Component component)) {
						AddComponentsToCache(component);
						result = component;
					}
				}
			}

			if(result == null) {
				result = GetComponentInChildren(type, true);
			}

			return result;
		}

		public void Add(SceneObject sceneObject)
		{
			AddComponentsToCache(sceneObject.GetComponents<Component>());
		}

		public void Add(Component component)
		{
			AddComponentsToCache(component);
		} 

		public void Remove(SceneObject sceneObject)
		{
			RemoveComponentsFromCache(sceneObject.GetComponents<Component>());
		}

		public void Remove(Component component)
		{
			RemoveComponentsFromCache(component);
		}

		private void AddComponentsToCache(params Component[] components)
		{
			foreach(Component component in components) {
				Type[] types = component.GetType().GetAllTypeAndInterfaceBases(typeof(MonoBehaviour), true, false);
				foreach(Type type in types) { GetOrCreateComponentCache(type).Add(component); }
			}
		}

		private void RemoveComponentsFromCache(params Component[] components)
		{
			foreach(Component component in components) {
				Type[] types = component.GetType().GetAllTypeAndInterfaceBases(typeof(MonoBehaviour), true, false);
				foreach(Type type in types) { GetOrCreateComponentCache(type).Remove(component); }
			}
		}

		private List<Component> GetOrCreateComponentCache(Type type)
		{
			if(!_componentCacheLookup.TryGetValue(type, out List<Component> cache)) {
				_componentCacheLookup[type] = cache = new List<Component>();
			}
			return cache;
		}

		private void Awake()
		{
			Add(this);
		}

		private void OnDestroy()
		{
			Remove(this);
		}
	}
}
