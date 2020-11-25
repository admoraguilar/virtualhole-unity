using VirtualHole.APIWrapper.Storage.Static;
using VirtualHole.APIWrapper.Storage.Dynamic;

namespace VirtualHole.APIWrapper.Storage
{
	public class StorageClient
	{
		public DynamicClient dynamic { get; set; } = null;
		public StaticClient @static { get; set; } = null;

		public StorageClient(string domain)
		{
			dynamic = new DynamicClient(domain);
			@static = new StaticClient(domain);
		}
	}
}
