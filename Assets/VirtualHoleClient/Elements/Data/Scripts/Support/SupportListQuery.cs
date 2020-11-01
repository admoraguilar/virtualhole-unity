using System.Threading;
using System.Threading.Tasks;
using Midnight;
using Midnight.Web;

namespace VirtualHole.Client.Data
{
	using Api.Storage;
	
	public class SupportListQuery
	{
		internal const string rootPath = "/client/page/support/";
		internal const string supportListPath = "support-list.json";

		private SupportInfo[] _supportInfo = null;

		private VirtualHoleStorageClient _client = null;
		private bool _isLoaded = false;

		public SupportListQuery(VirtualHoleStorageClientObject client)
		{
			_client = client.client;
		}

		public async Task<SupportInfo[]> LoadAsync(CancellationToken cancellationToken = default)
		{
			if(_isLoaded) { return _supportInfo; }

			using(new StopwatchScope(nameof(SupportInfo), "Start getting support-list", "End getting support-list")) {
				string url = _client.BuildObjectUri($"{rootPath}{supportListPath}").AbsoluteUri;
				string response = await TextGetWebRequest.GetAsync(url, null, cancellationToken);
				JsonUtilities.Deserialize(ref _supportInfo, response);
			}

			_isLoaded = true;
			return _supportInfo;
		}
	}
}
