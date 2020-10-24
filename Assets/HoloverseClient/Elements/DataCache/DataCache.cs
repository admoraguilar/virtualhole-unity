using System.Collections.Generic;

namespace Holoverse.Client.Caching
{
	public static partial class DataCache
	{
		private static Dictionary<string, Dictionary<string, object>> _dataCache = new Dictionary<string, Dictionary<string, object>>();

		public static T Get<T>(string group, string key)
		{
			return (T)GetOrCreateGroup(group)[key];
		}

		public static bool TryGet<T>(string group, string key, out T value)
		{
			if(GetOrCreateGroup(group).TryGetValue(key, out object result)) {
				value = (T)result;
				return true;
			}

			value = default;
			return false;
		}

		public static void Add<T>(string group, string key, T value)
		{
			GetOrCreateGroup(group)[key] = value;
		}

		public static void Remove(string group, string key)
		{
			GetOrCreateGroup(group).Remove(key);
		}

		private static Dictionary<string, object> GetOrCreateGroup(string group)
		{
			if(!_dataCache.TryGetValue(group, out Dictionary<string, object> cache)) {
				_dataCache[group] = cache = new Dictionary<string, object>();
			}
			return cache;
		}
	}
}
