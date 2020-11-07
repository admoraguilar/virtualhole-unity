using System;

namespace VirtualHole.Client.Data
{
	public interface ICache<T> : ICache
	{
		T GetOrUpsert(string key, Func<T> dataFactory);
		bool TryGet(string key, out T value);
		void Upsert(string key, T data);
	}

	public interface ICache
	{
		object GetOrUpsert(string key, Func<object> dataFactory);
		bool Contains(string key);
		bool TryGet(string key, out object value);
		void Upsert(string key, object data);
		void Remove(string key);
	}
}
