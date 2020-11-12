using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VirtualHole.APIWrapper.Storage.Dynamic
{
	public class DynamicClient : APIClient
	{
		public override string path => "dynamic";

		public DynamicClient(string domain) : base(domain)
		{ }

		public async Task<List<SupportInfo>> ListSupportInfoAsync(CancellationToken cancellationToken = default)
		{
			return await GetAsync<List<SupportInfo>>(CreateUri("support-list.json"), cancellationToken);
		}
	}
}
