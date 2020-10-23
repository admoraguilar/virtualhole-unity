using System;
using System.Collections.Generic;

namespace Holoverse.Client.Caching
{
	public class KeyValueCache<TKey, TValue>
	{
		private Func<TValue, TKey> _keyFactory = null;
		private Dictionary<TKey, TValue> _lookup = new Dictionary<TKey, TValue>();

		public KeyValueCache(Func<TValue, TKey> keyFactory)
		{
			_keyFactory = keyFactory;
		}

		public TValue Get(TKey key)
		{
			if(_lookup.TryGetValue(key, out TValue result)) {
				return result;
			}
			return default;
		}

		public void AddRange(IEnumerable<TValue> values)
		{
			foreach(TValue value in values) { Add(value); }
		}

		public void Add(TValue value)
		{
			_lookup[_keyFactory(value)] = value;
		}

		public void Remove(TValue value)
		{
			_lookup.Remove(_keyFactory(value));
		}

		public void Clear()
		{
			_lookup.Clear();
		}
	}
}