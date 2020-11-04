using System.Collections.Generic;
using Midnight;

namespace VirtualHole.Client.Data
{
	public class SimpleCache<TType, TSimpleCache> : IDataCache<TType> 
		where TType : class
		where TSimpleCache : class, new()
	{
		private static TSimpleCache _cache = null;

		public static TSimpleCache Get()
		{
			if(_cache != null) { return _cache; }
			return _cache = new TSimpleCache();
		}

		private Dictionary<string, TType> _lookup = new Dictionary<string, TType>();

		public bool TryGetData(string key, out TType data)
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

		bool IDataCache.TryGetData(string key, out object data)
		{
			bool result = TryGetData(key, out TType cachedData);
			data = cachedData;
			return result;
		}

		void IDataCache.Upsert(string key, object data)
		{
			TType castData = data as TType;
			if(castData == null) { MLog.LogError(nameof(TSimpleCache), $"Data is not of type {nameof(TType)}"); }
			Upsert(key, castData);
		}
	}
}
