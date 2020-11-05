using Midnight;
using Midnight.Concurrency;
using Midnight.Web;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualHole.Client.Data
{
	public class Image
	{
		public string url = string.Empty;
		public Sprite sprite = null;

		public Image(string url, Sprite sprite)
		{
			this.url = url;
			this.sprite = sprite;
		}
	}

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

		public async Task<Image[]> GetImagesAsync(CancellationToken cancellationToken = default)
		{
			SupportInfo[] supportInfos = await GetAsync(cancellationToken);
			Image[] images = new Image[supportInfos.Length];

			await Concurrent.ForEachAsync(supportInfos, LoadDataAsync, cancellationToken);

			int index = 0;
			foreach(SupportInfo info in supportInfos) {
				_settings.imagesCache.TryGet(info.imageUrl, out Image image);
				images[index] = image;
				index++;
			}

			return images;

			async Task LoadDataAsync(SupportInfo info)
			{
				await _settings.imagesCache.GetOrUpsertAsync(info.imageUrl, GetImageAsync);

				async Task<Image> GetImageAsync() => new Image(
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
				string response = await _settings.storageClient.GetAsync(Path.Combine(subPath, filePath), cancellationToken);
				return JsonUtilities.Deserialize<SupportInfo[]>(response);
			}
		}
	}
}
