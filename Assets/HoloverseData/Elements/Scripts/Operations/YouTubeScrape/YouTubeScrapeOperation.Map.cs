using System.IO;
using System.Collections.Generic;

namespace Holoverse.Data.YouTube
{
	public partial class YouTubeScrapeOperation
	{
		public class ChannelFilter : Filter<VideoInfo>
		{
			public List<ChannelInfo> channels { get; private set; } = null;

			public ChannelFilter(ChannelInfo channel)
			{
				this.channels = new List<ChannelInfo>();
				this.channels.Add(channel);
			}

			public ChannelFilter(IEnumerable<ChannelInfo> channels)
			{
				this.channels = new List<ChannelInfo>(channels);
			}

			public override bool IsValid(VideoInfo video)
			{
				return channels.Exists((ChannelInfo channel) => video.channelId.Contains(channel.id));
			}
		}

		public class DescriptionFilter : Filter<VideoInfo>
		{
			public string description { get; private set; } = string.Empty;

			public DescriptionFilter(string description)
			{
				this.description = description;
			}

			public override bool IsValid(VideoInfo video)
			{
				return video.description.Contains(description);
			}
		}
	}

	public partial class YouTubeScrapeOperation
	{
		public class ChannelMap
		{
			public string saveDirectoryPath = string.Empty;

			public ChannelInfo channel { get; private set; } = null;

			public Container<VideoInfo> discover { get; private set; } = null;
			public Container<VideoInfo> community { get; private set; } = null;
			public Container<VideoInfo> anime { get; private set; } = null;
			public Container<BroadcastInfo> live { get; private set; } = null;
			public Container<BroadcastInfo> schedule { get; private set; } = null;

			public ChannelMap(ChannelInfo channel)
			{
				this.channel = channel;

				discover.savePath = Path.Combine(saveDirectoryPath, "discover.json");
				community.savePath = Path.Combine(saveDirectoryPath, "community.json");
				anime.savePath = Path.Combine(saveDirectoryPath, "anime.json");
				live.savePath = Path.Combine(saveDirectoryPath, "live.json");
				schedule.savePath = Path.Combine(saveDirectoryPath, "schedule.json");
			}
		}

		public class Map
		{
			public Container<VideoInfo> discover { get; private set; } = null;
			public Container<VideoInfo> community { get; private set; } = null;
			public Container<VideoInfo> anime { get; private set; } = null;
			public Container<BroadcastInfo> live { get; private set; } = null;
			public Container<BroadcastInfo> schedule { get; private set; } = null;

			public List<ChannelMap> channels { get; private set; } = null;
		}
	}
}