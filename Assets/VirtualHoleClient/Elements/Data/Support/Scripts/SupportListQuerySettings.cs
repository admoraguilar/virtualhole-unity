
namespace VirtualHole.Client.Data
{
	using Api.Storage;
	using Api.Storage.Data;

	public class SupportListQuerySettings
	{
		public VirtualHoleStorageClient storageClient { get; set; } = null;

		public IDataCache<SupportInfo[]> supportInfoListCache { get; set; } = null;
		public IDataCache<ImageData> imagesDataCache { get; set; } = null;

		public SupportListQuerySettings()
		{
			storageClient = VirtualHoleStorageClientFactory.Get();

			supportInfoListCache = SimpleCache<SupportInfo[]>.Get();
			imagesDataCache = SimpleCache<ImageData>.Get();
		}
	}
}
