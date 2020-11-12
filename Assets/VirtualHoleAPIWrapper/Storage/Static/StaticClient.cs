using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualHole.APIWrapper.Storage.Static
{
	public class StaticClient : APIClient
	{
		public override string path => "static";

		public StaticClient(string domain) : base(domain)
		{ }

		public async Task<T> GetObjectAsync<T>(string slug, CancellationToken cancellationToken = default)
		{
			return await GetAsync<T>(CreateUri(slug), cancellationToken);
		}

		public async Task<Sprite> GetImageAsync(string slug, CancellationToken cancellationToken = default)
		{
			return await HTTPUtilities.GetImageAsync(CreateUri(slug).AbsoluteUri, cancellationToken);
		}
	}
}
