using System.Collections.Generic;

namespace Holoverse.Data.YouTube
{
	public partial class YouTubeScrapeOperation
	{
		public class ChannelIdFilter : ChannelFilter
		{
			public ChannelIdFilter(Channel channel) : base(channel) { }

			protected override bool IsValidImpl(Video video)
			{
				return channels.Exists((Channel channel) => video.channelId.Contains(channel.id));
			}
		}

		public class ChannelMatchFilter : ChannelFilter
		{
			public ChannelMatchFilter(Channel channel) : base(channel) { }

			protected override bool IsValidImpl(Video video)
			{
				return channels.Exists(
					(Channel channel) => {
						return video.channelId.Contains(channel.id) || 
							   video.title.Contains(channel.id) ||
							   video.description.Contains(channel.id) ||
							   video.title.Contains(channel.name) ||
							   video.description.Contains(channel.name) ||
							   video.description.Contains(channel.url);
					}
				);
			}
		}

		public abstract class ChannelFilter : Filter<Video>
		{
			public List<Channel> channels { get; protected set; } = null;

			public ChannelFilter(Channel channel)
			{
				channels = new List<Channel>();
				channels.Add(channel);
			}

			public ChannelFilter(IEnumerable<Channel> channels)
			{
				this.channels = new List<Channel>(channels);
			}
		}
	}
}