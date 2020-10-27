using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	public interface ILoopScrollCellReturnReceiver
	{
		void ScrollCellReturn();
	}

	[Serializable]
	public class LoopScrollPrefabSource
	{
		public GameObject prefab;
		public int poolSize = 5;

		private Dictionary<int, ILoopScrollCellReturnReceiver> _cellReturnReceiverLookup = new Dictionary<int, ILoopScrollCellReturnReceiver>();
		private bool _isInitialized = false;

		public virtual GameObject GetObject()
		{
			if(!_isInitialized) {
				_isInitialized = true;
				SG.ResourceManager.Instance.InitPool(prefab, poolSize);
			}
			return SG.ResourceManager.Instance.GetObjectFromPool(prefab);
		}

		public virtual void ReturnObject(Transform transform)
		{
			ILoopScrollCellReturnReceiver receiver = LoopScrollUtilities.GetComponentFromLookup(_cellReturnReceiverLookup, transform);
			if(receiver != null) { receiver.ScrollCellReturn(); }

			SG.ResourceManager.Instance.ReturnObjectToPool(transform.gameObject);
		}
	}
}
