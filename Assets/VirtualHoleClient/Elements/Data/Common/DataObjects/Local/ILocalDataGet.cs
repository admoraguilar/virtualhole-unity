using System.Threading.Tasks;
using Midnight;

namespace VirtualHole.Client.Data
{
	public interface ILocalDataGet<T> : IDataSource { }

	public static class LocalDataGetDefaults
	{
		public static async Task<T> GetFromLocalAsync<T>(this ILocalDataGet<T> localGet)
		{
			await Task.CompletedTask;
			return JsonUtilities.LoadFromDisk<T>(new JsonUtilities.LoadFromDiskParameters {
				filePath = localGet.GetFullPath()
			});
		}
	}
}
