using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;

namespace VirtualHole.Client.Data
{
	using APIWrapper.Contents.Creators;
	using VirtualHole.APIWrapper;

	public class CreatorDTO : DataQueryDTO<Creator>
	{
		public Sprite avatarSprite;

		public CreatorDTO(Creator creator) : base(creator) 
		{ }
	}

	public class CreatorQuerySettings : PaginatedQuerySettings<Creator, CreatorDTO>
	{
		public VirtualHoleAPIWrapperClient apiWrapperClient { get; set; } = null;

		public CreatorQuerySettings() : base()
		{
			apiWrapperClient = VirtualHoleAPIWrapperClientFactory.Get();
		}
	}

	public partial class CreatorQuery : PaginatedQuery<Creator, CreatorDTO, CreatorQuerySettings>
	{
		private ListCreatorsRequest _request = null;

		public CreatorQuery(ListCreatorsRequest request, CreatorQuerySettings querySettings = null) : base(querySettings)
		{
			_request = request;
		}

		protected override CreatorDTO FromRawToDTO(Creator raw)
		{
			return new CreatorDTO(raw);
		}

		protected override async Task ProcessDTOAsync(CreatorDTO dto, CancellationToken cancellationToken = default)
		{
			dto.avatarSprite = await HTTPUtilities.GetImageAsync(dto.raw.avatarUrl, cancellationToken);
		}

		protected override string GetCacheKey(Creator raw)
		{
			return raw.universalId;
		}

		protected override async Task<IEnumerable<Creator>> GetRawAsync_Impl(CancellationToken cancellationToken = default)
		{
			using(new StopwatchScope(
				nameof(CreatorQuery),
				$"Start getting {nameof(Creator)}s...",
				$"End getting {nameof(Creator)}s.")) {

				List<Creator> results = new List<Creator>();
				CreatorClient creatorClient = _querySettings.apiWrapperClient.contents.creators;
				results.AddRange(await creatorClient.ListCreatorsAsync(_request, cancellationToken));

				isDone = true;
				return results;
			}
		}
	}

	public partial class CreatorQuery
	{
		public static class Affiliation
		{
			public static string hololiveProduction => "hololiveProduction";
			public static string community => "Community";
		}
	}
}
