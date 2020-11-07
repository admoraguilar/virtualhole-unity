using System;
using System.Collections.Generic;
using UnityEngine;
using Midnight;

namespace VirtualHole.Client.Data
{
	public class SimpleCache<TType> : ICache<TType> 
		where TType : class
	{
		private static SimpleCache<TType> _cache = null;

		public static SimpleCache<TType> Get()
		{
			if(_cache != null) { return _cache; }
			return _cache = new SimpleCache<TType>();
		}

		private Dictionary<string, TType> _lookup = new Dictionary<string, TType>();

		public TType GetOrUpsert(string key, Func<TType> dataFactory)
		{
			if(!TryGet(key, out TType data)) {
				data = dataFactory();
				Upsert(key, data);
			}

			return data;
		}

		public bool Contains(string key)
		{
			return _lookup.ContainsKey(key);
		}

		public bool TryGet(string key, out TType data)
		{
			return _lookup.TryGetValue(key, out data);
		}

		public void Upsert(string key, TType data)
		{
			_lookup[key] = data;
		}

		public void Remove(string key)
		{
			_lookup.Remove(key);
		}

		object IDataCache.GetOrUpsert(string key, Func<object> dataFactory)
		{
			return GetOrUpsert(key, () => (TType)dataFactory());
		}

		bool IDataCache.TryGet(string key, out object data)
		{
			bool result = TryGet(key, out TType cachedData);
			data = cachedData;
			return result;
		}

		void IDataCache.Upsert(string key, object data)
		{
			TType castData = data as TType;
			if(castData == null) { MLog.LogError(nameof(SimpleCache<Sprite>), $"Data is not of type {nameof(TType)}"); }
			Upsert(key, castData);
		}
	}
}
