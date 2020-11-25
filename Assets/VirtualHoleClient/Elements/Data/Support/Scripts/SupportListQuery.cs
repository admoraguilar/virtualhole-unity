using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Logs;
using VirtualHole.APIWrapper;
using VirtualHole.APIWrapper.Storage;
using VirtualHole.APIWrapper.Storage.Dynamic;

namespace VirtualHole.Client.Data
{
	public class SupportInfoDTO : DataQueryDTO<SupportInfo>
	{
		public Sprite imageSprite;

		public SupportInfoDTO(SupportInfo supportInfo) : base(supportInfo)
		{ }
	}

	public class SupportListQuerySettings : PaginatedQuerySettings<SupportInfo, SupportInfoDTO>
	{
		public VirtualHoleAPIWrapperClient apiWrapperClient { get; set; } = null;

		public SupportListQuerySettings() : base()
		{
			apiWrapperClient = VirtualHoleAPIWrapperClientFactory.Get();
		}
	}

	public class SupportListQuery : PaginatedQuery<SupportInfo, SupportInfoDTO, SupportListQuerySettings>
	{
		public SupportListQuery(SupportListQuerySettings querySettings = null) : base(querySettings) 
		{ }

		protected override SupportInfoDTO FromRawToDTO(SupportInfo raw)
		{
			return new SupportInfoDTO(raw);
		}

		protected async override Task ProcessDTOAsync(SupportInfoDTO dto, CancellationToken cancellationToken = default)
		{
			StorageClient storageClient = _querySettings.apiWrapperClient.storage;
			dto.imageSprite = await storageClient.@static.GetImageAsync(dto.raw.imagePath, cancellationToken);
		}

		protected override string GetCacheKey(SupportInfo raw)
		{
			return raw.url;
		}

		protected async override Task<IEnumerable<SupportInfo>> GetRawAsync_Impl(CancellationToken cancellationToken = default)
		{
			using(new StopwatchScope(
					nameof(SupportInfo),
					$"Start getting support-list",
					$"End getting support-list")) {
				StorageClient storageClient = _querySettings.apiWrapperClient.storage;
				List<SupportInfo> result = await storageClient.dynamic.ListSupportInfoAsync(cancellationToken);
				isDone = true;
				return result;
			}
		}
	}
}
