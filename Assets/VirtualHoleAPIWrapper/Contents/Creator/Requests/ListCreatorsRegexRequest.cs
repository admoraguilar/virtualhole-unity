using System.Collections.Generic;

namespace VirtualHole.APIWrapper.Contents.Creators
{
	public class ListCreatorsRegexRequest : ListCreatorsRequest
	{
		public List<string> searchQueries = new List<string>();

		public bool isCheckUniversalName = true;
		public bool isCheckUniversalId = true;

		public bool isCheckSocialName = true;
		public bool isCheckSocialCustomKeywords = true;

		public bool isCheckCustomKeywords = true;
	}
}
