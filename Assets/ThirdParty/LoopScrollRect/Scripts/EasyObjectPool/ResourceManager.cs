using UnityEngine;
using System.Collections.Generic;

namespace SG
{
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class ResourceManager : MonoBehaviour
	{
		public static ResourceManager Instance
		{
			get {
				if(_instance == null) {
					GameObject go = new GameObject("ResourceManager", typeof(ResourceManager));
					go.transform.localPosition = new Vector3(9999999, 9999999, 9999999);
					// Kanglai: if we have `GO.hideFlags |= HideFlags.DontSave;`, we will encounter Destroy problem when exit playing
					// However we should keep using this in Play mode only!
					_instance = go.GetComponent<ResourceManager>();

					if(Application.isPlaying) {
						DontDestroyOnLoad(_instance.gameObject);
					} else {
						Debug.LogWarning("[ResourceManager] You'd better ignore ResourceManager in Editor mode");
					}
				}

				return _instance;
			}
		}
		private static ResourceManager _instance = null;

		private Dictionary<string, Pool> poolDict = new Dictionary<string, Pool>();

		public void InitPool(GameObject prefab, int size, PoolInflationType type = PoolInflationType.DOUBLE)
		{
			string poolName = prefab.GetInstanceID().ToString();
			if(poolDict.ContainsKey(poolName)) {
				return;
			} else {
				if(prefab == null) {
					Debug.LogError("[ResourceManager] Invalide prefab name for pooling :" + poolName);
					return;
				}
				poolDict[poolName] = new Pool(poolName, prefab, gameObject, size, type);
			}
		}

		/// <summary>
		/// Returns an available object from the pool 
		/// OR null in case the pool does not have any object available & can grow size is false.
		/// </summary>
		/// <param name="poolName"></param>
		/// <returns></returns>
		public GameObject GetObjectFromPool(GameObject prefab, bool autoActive = true, int autoCreate = 0)
		{
			GameObject result = null;

			string poolName = prefab.GetInstanceID().ToString();
			if(!poolDict.ContainsKey(poolName) && autoCreate > 0) {
				InitPool(prefab, autoCreate, PoolInflationType.INCREMENT);
			}

			if(poolDict.ContainsKey(poolName)) {
				Pool pool = poolDict[poolName];
				result = pool.NextAvailableObject(autoActive);
				//scenario when no available object is found in pool
#if UNITY_EDITOR
				if(result == null) {
					Debug.LogWarning("[ResourceManager]:No object available in " + poolName);
				}
#endif
			}
#if UNITY_EDITOR
			else {
				Debug.LogError("[ResourceManager]:Invalid pool name specified: " + poolName);
			}
#endif
			return result;
		}

		/// <summary>
		/// Return obj to the pool
		/// </summary>
		/// <param name="go"></param>
		public void ReturnObjectToPool(GameObject go)
		{
			PoolObject po = go.GetComponent<PoolObject>();
			if(po == null) {
#if UNITY_EDITOR
				Debug.LogWarning("Specified object is not a pooled instance: " + go.name);
#endif
			} else {
				Pool pool = null;
				if(poolDict.TryGetValue(po.poolName, out pool)) {
					pool.ReturnObjectToPool(po);
				}
#if UNITY_EDITOR
				else {
					Debug.LogWarning("No pool available with name: " + po.poolName);
				}
#endif
			}
		}

		/// <summary>
		/// Return obj to the pool
		/// </summary>
		/// <param name="t"></param>
		public void ReturnTransformToPool(Transform t)
		{
			if(t == null) {
#if UNITY_EDITOR
				Debug.LogError("[ResourceManager] try to return a null transform to pool!");
#endif
				return;
			}
			ReturnObjectToPool(t.gameObject);
		}
	}
}