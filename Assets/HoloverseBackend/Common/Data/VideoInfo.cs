using System;

namespace Holoverse.Backend.YouTube
{
	[Serializable]
	public class VideoInfo
	{
		public string url;
		public string id;
		public string title;
		public string duration;
		public string viewCount;
		public string mediumResThumbnailUrl;
		public string channel;
		public string channelId;
		public string uploadDate;
	}
}
