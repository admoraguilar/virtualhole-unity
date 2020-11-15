
namespace VirtualHole.APIWrapper.Contents.Videos
{
	public abstract class ListVideosRequest : PagedRequest
	{
		public SortMode sortMode = SortMode.None;
		public bool isSortAscending = true;
	}
}
