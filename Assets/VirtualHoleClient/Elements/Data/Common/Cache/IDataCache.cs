using System;
using System.Threading.Tasks;

namespace VirtualHole.Client.Data
{
	public interface IDataCache<T> : IDataCache
	{
		Task<T> GetOrUpsertAsync(string key, Func<Task<T>> dataFactory);
		T GetOrUpsert(string key, Func<T> dataFactory);
		bool TryGet(string key, out T value);
		void Upsert(string key, T data);
	}

	public interface IDataCache
	{
		Task<object> GetOrUpsert(string key, Func<Task<object>> dataFactory);
		object GetOrUpsert(string key, Func<object> dataFactory);
		bool TryGet(string key, out object value);
		void Upsert(string key, object data);
		void Remove(string key);
	}
}
