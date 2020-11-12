
namespace VirtualHole.APIWrapper 
{
	using Contents;
	using Storage;

	public class VirtualHoleAPIWrapperClient
	{
		public string contentsDomain { get; private set; } = string.Empty;
		public string storageDomain { get; private set; } = string.Empty;

		public ContentClient contents { get; private set; } = null;
		public StorageClient storage { get; private set; } = null;

		public VirtualHoleAPIWrapperClient(string contentDomain, string storageDomain)
		{
			this.contentsDomain = contentDomain;
			this.storageDomain = storageDomain;

			contents = new ContentClient(contentDomain);
			storage = new StorageClient(storageDomain);
		}
	}
}
