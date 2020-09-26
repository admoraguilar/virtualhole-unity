using System;

namespace Holoverse.Data.YouTube
{
	[Serializable]
	public class VideoInfo
	{
		public string url = string.Empty;
		public string id = string.Empty;
		public string title = string.Empty;
		public string description = string.Empty;
		//public TimeSpan duration = TimeSpan.Zero;
		public string duration = string.Empty;
		public long viewCount = 0;
		public string mediumResThumbnailUrl = string.Empty;
		public string channel = string.Empty;
		public string channelId = string.Empty;
		//public DateTimeOffset uploadDate = DateTimeOffset.MinValue;
		public string uploadDate = string.Empty;
	}
}
