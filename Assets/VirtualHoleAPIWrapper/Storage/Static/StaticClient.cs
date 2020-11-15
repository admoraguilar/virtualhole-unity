using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualHole.APIWrapper.Storage.Static
{
	public class StaticClient : APIClient
	{
		public override string path => "static";
		public override string version => string.Empty;

		public StaticClient(string domain) : base(domain)
		{ }

		public async Task<T> GetObjectAsync<T>(string slug, CancellationToken cancellationToken = default)
		{
			return await GetAsync<T>(CreateUri(slug), cancellationToken);
		}

		public async Task<Sprite> GetImageAsync(string slug, CancellationToken cancellationToken = default)
		{
			// TODO: Hack
			// This just removes the path if the slug has included the path
			// Find a better way to handle this
			string uri = CreateUri(slug.Replace($"{path}/", "")).AbsoluteUri;
			return await HTTPUtilities.GetImageAsync(uri, cancellationToken);
		}
	}
}
