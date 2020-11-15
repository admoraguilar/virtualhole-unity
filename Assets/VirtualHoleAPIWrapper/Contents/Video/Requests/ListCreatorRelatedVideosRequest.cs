using System.Collections.Generic;

namespace VirtualHole.APIWrapper.Contents.Videos
{
	using Creators;

	public class ListCreatorRelatedVideosRequest : ListVideosRequest
	{
		public bool isBroadcast = false;
		public bool isLive = true;
		public List<Creator> creators = new List<Creator>();
	}
}