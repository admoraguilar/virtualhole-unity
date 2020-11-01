using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualHole.Client.Data
{
	public class SupportListQuery
	{
		internal const string rootPath = "/client/page/support/";
		internal const string supportListPath = "support-list.json";

		private CDNClientObject _client = null;
		private bool _isLoaded = false;

		public SupportListQuery(CDNClientObject client)
		{
			
		}

		//public async Task<SupportInfo[]> LoadAsync(CancellationToken cancellationToken = default)
		//{

		//}
	}
}
