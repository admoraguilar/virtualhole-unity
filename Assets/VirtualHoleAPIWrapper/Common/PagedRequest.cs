
namespace VirtualHole.APIWrapper
{
	public abstract class PagedRequest
	{
		public int batchSize = 20;
		public int resultsLimit = 500;
		public int skip = 0;
	}
}