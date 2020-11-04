using System.Threading.Tasks;
using Midnight;

namespace VirtualHole.Client.Data
{
	public interface ILocalDataUpsert<T> : IDataSource { }

	public static class LocalDataUpsertDefaults
	{
		public static async Task UpsertToLocalAsync<T>(this ILocalDataGet<T> localUpsert, object data)
		{
			await Task.CompletedTask;
			JsonUtilities.SaveToDisk(data, new JsonUtilities.SaveToDiskParameters {
				filePath = localUpsert.GetFullPath()
			});
		}
	}
}
