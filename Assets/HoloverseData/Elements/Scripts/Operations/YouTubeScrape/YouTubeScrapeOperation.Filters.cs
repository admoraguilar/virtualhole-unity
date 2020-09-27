using System.Collections.Generic;

namespace Holoverse.Data.YouTube
{
	public partial class YouTubeScrapeOperation
	{
		public class LiveBroadcastFilter<T> : Filter<Broadcast>
		{
			protected override bool IsValidImpl(Broadcast broadcast)
			{
				return broadcast.IsLive;
			}
		}

		public class ChannelIdFilter<T> : ChannelFilter<T>
			where T : Video
		{
			public ChannelIdFilter(Channel channel) : base(channel) { }

			public ChannelIdFilter(IEnumerable<Channel> channels) : base(channels) { }

			protected override bool IsValidImpl(T video)
			{
				return channels.Exists((Channel channel) => video.channelId.Contains(channel.id));
			}
		}

		public class ChannelMatchFilter<T> : ChannelFilter<T>
			where T : Video
		{
			public ChannelMatchFilter(Channel channel) : base(channel) { }

			public ChannelMatchFilter(IEnumerable<Channel> channels) : base(channels) { }

			protected override bool IsValidImpl(T video)
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

		public abstract class ChannelFilter<T> : Filter<T>
			where T : Video
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