using System;
using System.Collections.Generic;

namespace VirtualHole.Client.Data
{
	public static class UserDataCache
	{
		private static Dictionary<Type, object> _userDataLookup = new Dictionary<Type, object>();

		public static bool TryGet<T>(out T result)
		{
			if(!_userDataLookup.TryGetValue(typeof(T), out object data)) {
				result = default;
				return false;
			}

			result = (T)data;
			return true;
		}

		public static void Upsert<T>(T data)
		{
			_userDataLookup[typeof(T)] = data;
		}

		public static void Remove<T>()
		{
			_userDataLookup.Remove(typeof(T));
		}

		public static void Clear()
		{
			_userDataLookup.Clear();
		}
	}
}
