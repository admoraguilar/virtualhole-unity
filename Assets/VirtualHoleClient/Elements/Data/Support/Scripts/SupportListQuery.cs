using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Web;

namespace VirtualHole.Client.Data
{
	using Api.Storage;
	using Api.Storage.Data;

	public class SupportInfoDTO : DataQueryDTO<SupportInfo>
	{
		public Sprite imageSprite;

		public SupportInfoDTO(SupportInfo supportInfo) : base(supportInfo)
		{ }
	}

	public class SupportListQuerySettings : PaginatedQuerySettings<SupportInfo, SupportInfoDTO>
	{
		public VirtualHoleStorageClient storageClient { get; set; } = null;

		public SupportListQuerySettings() : base()
		{
			storageClient = VirtualHoleStorageClientFactory.Get();
		}
	}

	public class SupportListQuery : PaginatedQuery<SupportInfo, SupportInfoDTO, SupportListQuerySettings>, ILocatableData
	{
		public string rootPath => _querySettings.storageClient.endpoint;
		public string subPath => "dynamic";
		public string filePath => "support-list.json";

		public SupportListQuery(SupportListQuerySettings querySettings = null) : base(querySettings) 
		{ }

		protected override SupportInfoDTO FromRawToDTO(SupportInfo raw)
		{
			return new SupportInfoDTO(raw);
		}

		protected async override Task ProcessDTOAsync(SupportInfoDTO dto, CancellationToken cancellationToken = default)
		{
			//dto.imageSprite = await ImageGetWebRequest.GetAsync(
			//	_querySettings.storageClient.BuildObjectUri(dto.raw.imagePath).AbsoluteUri, null,
			//	cancellationToken);
			dto.imageSprite = await QueryUtilities.GetImageAsync(
				_querySettings.storageClient.BuildObjectUri(dto.raw.imagePath).AbsoluteUri,
				cancellationToken);
		}

		protected override string GetCacheKey(SupportInfo raw)
		{
			return raw.url;
		}

		protected async override Task<IEnumerable<SupportInfo>> GetRawAsync_Impl(CancellationToken cancellationToken = default)
		{
			using(new StopwatchScope(
					nameof(SupportInfo),
					$"Start getting {this.GetFullPath()}",
					$"End getting {this.GetFullPath()}")) {
				string response = await _querySettings.storageClient.GetAsync(Path.Combine(subPath, filePath), cancellationToken);
				SupportInfo[] result = JsonUtilities.Deserialize<SupportInfo[]>(response);

				isDone = true;
				return result;
			}
		}
	}
}
