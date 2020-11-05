using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Midnight;
using Midnight.Concurrency;
using Midnight.Web;

namespace VirtualHole.Client.Data
{
	using Api.Storage.Data;

	public class SupportListQuery : ILocatableData
	{
		public string rootPath => _settings.storageClient.endpoint;
		public string subPath => "dynamic";
		public string filePath => "support-list.json";

		private SupportListQuerySettings _settings = null;

		public SupportListQuery(SupportListQuerySettings settings = null)
		{
			_settings = settings;
			if(_settings == null) { _settings = new SupportListQuerySettings(); }
		}

		public async Task<ImageData[]> GetImagesAsync(CancellationToken cancellationToken = default)
		{
			SupportInfo[] supportInfos = await GetAsync(cancellationToken);
			ImageData[] images = new ImageData[supportInfos.Length];

			await Concurrent.ForEachAsync(supportInfos, LoadDataAsync, cancellationToken);

			int index = 0;
			foreach(SupportInfo info in supportInfos) {
				_settings.imagesDataCache.TryGet(info.imageUrl, out ImageData image);
				images[index] = image;
				index++;
			}

			return images;

			async Task LoadDataAsync(SupportInfo info)
			{
				await _settings.imagesDataCache.GetOrUpsertAsync(info.imageUrl, GetImageAsync);

				async Task<ImageData> GetImageAsync() => new ImageData(
					info.imageUrl,
					await ImageGetWebRequest.GetAsync(
						_settings.storageClient.BuildObjectUri(info.imageUrl).AbsoluteUri, null,
						cancellationToken));
			}
		}

		public async Task<SupportInfo[]> GetAsync(CancellationToken cancellationToken = default)
		{
			return await _settings.supportInfoListCache.GetOrUpsertAsync(filePath, GetDataAsync);

			async Task<SupportInfo[]> GetDataAsync()
			{
				string response = string.Empty;
				using(new StopwatchScope(
					nameof(SupportInfo),
					$"Start getting {this.GetFullPath()}",
					$"End getting {this.GetFullPath()}")) {
					response = await _settings.storageClient.GetAsync(Path.Combine(subPath, filePath), cancellationToken);
				}
				return JsonUtilities.Deserialize<SupportInfo[]>(response);
			}
		}
	}
}
