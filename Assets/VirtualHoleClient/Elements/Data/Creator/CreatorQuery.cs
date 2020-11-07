using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Web;

namespace VirtualHole.Client.Data
{
	using Api.DB;
	using Api.DB.Common;
	using Api.DB.Contents.Creators;
	
	public class CreatorDTO : DataQueryDTO<Creator>
	{
		public Sprite avatarSprite;

		public CreatorDTO(Creator creator) : base(creator) 
		{ }
	}

	public class CreatorQuerySettings : PaginatedQuerySettings<Creator, CreatorDTO>
	{
		public VirtualHoleDBClient dbClient { get; set; } = null;

		public CreatorQuerySettings() : base()
		{
			dbClient = VirtualHoleDBClientFactory.Get();
		}
	}

	public class CreatorQuery : PaginatedQuery<Creator, CreatorDTO, CreatorQuerySettings>
	{
		private FindCreatorsSettings _findCreatorsSettings = null;

		public CreatorQuery(FindCreatorsSettings findCreatorsSettings, CreatorQuerySettings querySettings = null) : base(querySettings)
		{
			_findCreatorsSettings = findCreatorsSettings;
		}

		protected override CreatorDTO FromRawToDTO(Creator raw)
		{
			return new CreatorDTO(raw);
		}

		protected override async Task ProcessDTOAsync(CreatorDTO dto, CancellationToken cancellationToken = default)
		{
			dto.avatarSprite = await ImageGetWebRequest.GetAsync(dto.raw.avatarUrl, null, cancellationToken);
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
				CreatorClient creatorClient = _querySettings.dbClient.contents.creators;

				using(FindResults<Creator> findResults = await creatorClient.FindCreatorsAsync(_findCreatorsSettings, cancellationToken)) {
					while(await findResults.MoveNextAsync()) {
						results.AddRange(findResults.current);
					}
				}

				isDone = true;
				return results;
			}
		}
	}
}
