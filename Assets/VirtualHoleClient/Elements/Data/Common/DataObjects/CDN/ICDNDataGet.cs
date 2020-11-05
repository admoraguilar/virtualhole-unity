using System.Threading;
using System.Threading.Tasks;
using Midnight;

namespace VirtualHole.Client.Data
{
	using Api.Storage;

	public interface ICDNDataGet<T> : ILocatableData 
	{ 
		VirtualHoleStorageClient client { get; }
	}

	public static class CDNDataGetDefaults
	{
		public static async Task<T> GetFromCDNAsync<T>(this ICDNDataGet<T> localCDN, CancellationToken cancellationToken = default)
		{
			T result = default;

			using(new StopwatchScope(
				nameof(SupportInfo), 
				$"Start getting {localCDN.filePath}", 
				$"End getting {localCDN.filePath}")) {
				string response = await localCDN.client.GetAsync(localCDN.GetFullPath(), cancellationToken);
				result = JsonUtilities.Deserialize<T>(response);
			}

			return result;
		}
	}
}
