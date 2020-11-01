using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Midnight;

namespace VirtualHole.Client.Data
{
	using Api.DB;
	using Api.DB.Common;
	using Api.DB.Contents.Creators;

	public class CreatorQuery
	{
		public IReadOnlyDictionary<string, Creator> creatorLookup => _creatorLookup;
		private Dictionary<string, Creator> _creatorLookup = new Dictionary<string, Creator>();

		private VirtualHoleDBClient _client = null;
		private FindCreatorsSettings _findCreatorsSettings = null;
		private bool _isLoaded = false;

		public CreatorQuery(VirtualHoleDBClient client, FindCreatorsSettings findCreatorsSettings)
		{
			_client = client;
			_findCreatorsSettings = findCreatorsSettings;
		}

		public async Task<IEnumerable<Creator>> LoadAsync(CancellationToken cancellationToken = default)
		{
			if(_isLoaded) { return _creatorLookup.Values; }

			using(new StopwatchScope(nameof(CreatorQuery), "Start getting creators...", "End getting creators.")) {
				CreatorClient creatorClient = _client.contents.creators;
				using(FindResults<Creator> results = await creatorClient.FindCreatorsAsync(_findCreatorsSettings, cancellationToken)) {
					while(await results.MoveNextAsync()) {
						AddCreatorsToLookup(results.current);
					}
				}
			}

			_isLoaded = true;
			return _creatorLookup.Values;
		}

		private void AddCreatorsToLookup(IEnumerable<Creator> creators)
		{
			foreach(Creator result in creators) {
				_creatorLookup[result.universalId] = result;
			}

			CreatorCache.Add(creators);
		}
	}
}
