using System;

namespace VirtualHole.APIWrapper.Contents.Videos
{
	[Serializable]
	public class Broadcast : Video
	{
		public bool isLive = false;
		public long viewerCount = 0;
		public DateTimeOffset schedule = DateTimeOffset.MinValue;
	}
}
