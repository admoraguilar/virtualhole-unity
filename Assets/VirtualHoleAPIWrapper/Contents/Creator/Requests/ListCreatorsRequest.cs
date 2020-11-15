using System.Collections.Generic;

namespace VirtualHole.APIWrapper.Contents.Creators
{
	public class ListCreatorsRequest : PagedRequest
	{
		public bool isHidden = false;

		public bool isCheckForAffiliations = false;
		public List<string> affiliations = new List<string>();

		public bool isGroup = false;

		public bool isCheckForDepth = false;
		public int depth = 0;
	}
}
