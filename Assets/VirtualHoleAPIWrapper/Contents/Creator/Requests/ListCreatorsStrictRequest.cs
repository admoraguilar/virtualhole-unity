
namespace VirtualHole.APIWrapper.Contents.Creators
{
	public class ListCreatorsStrictRequest : ListCreatorsRequest
	{
		public bool isAll = false;

		public string universalName = string.Empty;
		public string universalId = string.Empty;
	}
}
