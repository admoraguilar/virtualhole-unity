using System.Threading;
using System.Threading.Tasks;
using Midnight;
using Midnight.Web;

namespace VirtualHole.Client.Data
{
	using Api.Storage;
	
	public class SupportListQuery
	{
		internal const string rootPath = "/dynamic/";
		internal const string supportListPath = "support-list.json";

		public SupportInfo[] _supportInfos { get; private set; } = null;

		private VirtualHoleStorageClient _client = null;
		private bool _isLoaded = false;

		public SupportListQuery(VirtualHoleStorageClient client)
		{
			_client = client;
		}

		public string BuildImageUrl(SupportInfo supportInfo)
		{
			return _client.BuildObjectUri(supportInfo.imageUrl).AbsoluteUri;
		}

		public async Task<SupportInfo[]> LoadAsync(CancellationToken cancellationToken = default)
		{
			if(_isLoaded) { return _supportInfos; }

			using(new StopwatchScope(nameof(SupportInfo), "Start getting support-list", "End getting support-list")) {
				string url = _client.BuildObjectUri($"{rootPath}{supportListPath}").AbsoluteUri;
				string response = await TextGetWebRequest.GetAsync(url, null, cancellationToken);

				SupportInfo[] result = null;
				JsonUtilities.Deserialize(ref result, response);
				_supportInfos = result;
			}

			_isLoaded = true;
			return _supportInfos;
		}
	}
}
