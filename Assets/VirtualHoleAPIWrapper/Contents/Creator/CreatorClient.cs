using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VirtualHole.APIWrapper.Contents.Creators
{
	public class CreatorClient : APIClient
	{
		public override string path => "Creators";

		public CreatorClient(string domain) : base(domain)
		{ }

		public async Task<List<Creator>> ListCreatorsAsync<T>(
			T request, CancellationToken cancellationToken = default)
			where T : ListCreatorsRequest
		{
			if(request is ListCreatorsRegexRequest regexRequest) { return await ListCreatorRegexAsync(regexRequest, cancellationToken); }
			else if(request is ListCreatorsStrictRequest strictRequest) { return await ListCreatorsStrictAsync(strictRequest, cancellationToken); }
			else { return await ListCreatorsSimpleAsync(request, cancellationToken); }
		}

		public async Task<List<Creator>> ListCreatorRegexAsync(
			ListCreatorsRegexRequest request, CancellationToken cancellationToken = default)
		{
			return await PostAsync<List<Creator>>(CreateUri("ListCreatorsRegex"), request, cancellationToken);
		}

		public async Task<List<Creator>> ListCreatorsStrictAsync(
			ListCreatorsStrictRequest request, CancellationToken cancellationToken = default)
		{
			return await PostAsync<List<Creator>>(CreateUri("ListCreatorsStrict"), request, cancellationToken);
		}

		public async Task<List<Creator>> ListCreatorsSimpleAsync(
			ListCreatorsRequest request, CancellationToken cancellationToken = default)
		{
			return await PostAsync<List<Creator>>(CreateUri("ListCreators"), request, cancellationToken);
		}
	}
}
