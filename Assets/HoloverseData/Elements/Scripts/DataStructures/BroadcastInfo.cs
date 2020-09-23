using System;

namespace Holoverse.Data.YouTube
{
	public class BroadcastInfo : VideoInfo
	{
		public bool IsLive = false;
		public long viewerCount = 0;
		//public DateTimeOffset schedule = DateTimeOffset.MinValue;
		public string schedule = string.Empty;
	}
}
