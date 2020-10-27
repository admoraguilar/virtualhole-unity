using System.Collections.Generic;

namespace UnityEngine.UI
{
	public static class LoopScrollUtilities
	{
		public static T GetComponentFromLookup<T>(Dictionary<int, T> lookup, Transform transform)
		{
			int instanceId = transform.GetInstanceID();
			if(!lookup.TryGetValue(instanceId, out T value)) {
				if(transform.TryGetComponent(out value)) {
					lookup[instanceId] = value;
				}
			}
			return value;
		}
	}
}
