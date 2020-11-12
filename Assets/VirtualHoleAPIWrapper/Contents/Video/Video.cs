using System;

namespace VirtualHole.APIWrapper.Contents.Videos
{
	[Serializable]
	public class Video : Content
	{
		public string thumbnailUrl = string.Empty;
		public string description = string.Empty;
		public TimeSpan duration = TimeSpan.Zero;
		public long viewCount = 0;
	}
}
