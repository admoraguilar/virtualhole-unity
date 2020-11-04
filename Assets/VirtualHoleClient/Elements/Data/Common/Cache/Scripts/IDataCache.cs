
namespace VirtualHole.Client.Data
{
	public interface IDataCache<T> : IDataCache
	{
		bool TryGetData(string key, out T value);
		void Upsert(string key, T data);
	}

	public interface IDataCache
	{
		bool TryGetData(string key, out object value);
		void Upsert(string key, object data);
		void Remove(string key);
	}
}
