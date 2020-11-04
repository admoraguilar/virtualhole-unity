using System.Threading;
using System.Threading.Tasks;
using VirtualHole.Api.Storage;

namespace VirtualHole.Client.Data
{
	public class SupportListData : ICDNDataGet<SupportInfo[]>
	{
		private static SupportInfo[] _cachedSupportInfo = default; 

		public string rootPath => _client.endpoint;
		public string subPath => "dynamic";
		public string filePath => "support-list.json";

		VirtualHoleStorageClient ICDNDataGet<SupportInfo[]>.client => _client;
		private VirtualHoleStorageClient _client = null;

		public SupportListData(VirtualHoleStorageClient client)
		{
			_client = client;
		}

		public string BuildImageUrl(SupportInfo supportInfo)
		{
			return _client.BuildObjectUri(supportInfo.imageUrl).AbsoluteUri;
		}

		public async Task<SupportInfo[]> GetAsync(CancellationToken cancellationToken = default)
		{
			if(_cachedSupportInfo != null) { return _cachedSupportInfo; }
			return _cachedSupportInfo = await this.GetFromCDNAsync(cancellationToken);
		}
	}
}
