
namespace VirtualHole.Client.Data
{
	using Api.Storage;

	public class SupportListQuerySettings
	{
		public VirtualHoleStorageClient storageClient { get; set; } = null;

		public IDataCache<SupportInfo[]> supportInfoListCache { get; set; } = null;
		public IDataCache<Image> imagesCache { get; set; } = null;

		public SupportListQuerySettings()
		{
			storageClient = VirtualHoleStorageClientFactory.Get();

			supportInfoListCache = SimpleCache<SupportInfo[]>.Get();
			imagesCache = SimpleCache<Image>.Get();
		}
	}
}
