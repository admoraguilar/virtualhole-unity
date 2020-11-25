using System.Collections.Generic;
using VirtualHole.APIWrapper.Contents.Creators;

namespace VirtualHole.APIWrapper.Contents.Videos
{
	public class ListCreatorRelatedVideosRequest : ListVideosRequest
	{
		public bool isBroadcast = false;
		public bool isLive = true;
		public List<Creator> creators = new List<Creator>();
	}
}