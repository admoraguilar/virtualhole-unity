using System.Linq;
using System.Collections.Generic;

namespace Holoverse.Data.YouTube
{
	public partial class YouTubeScrapeOperation
	{
		public static class Filters
		{
			public static bool ContainsTextInTitle<T>(T item, string text)
				where T : Video
			{
				return item.title.Contains(text);
			}

			public static bool IsLive(Broadcast broadcast)
			{
				return broadcast.IsLive;
			}

			public static bool IsChannelIdMatch<T>(T item, Channel channel)
				where T : Video
			{
				return IsChannelIdMatch(item, new Channel[] { channel });
			}

			public static bool IsChannelIdMatch<T>(T item, IEnumerable<Channel> channels)
				where T : Video
			{
				return channels.Any((Channel channel) => item.channelId.Contains(channel.id));
			}

			public static bool IsChannelMatch<T>(T item, Channel channel)
				where T : Video
			{
				return IsChannelMatch(item, new Channel[] { channel });
			}

			public static bool IsChannelMatch<T>(T item, IEnumerable<Channel> channels)
				where T : Video
			{
				return channels.Any(
					(Channel channel) => {
						return item.channelId.Contains(channel.id) ||
							   item.title.Contains(channel.id) ||
							   item.description.Contains(channel.id) ||
							   item.channelId.Contains(channel.name) ||
							   item.title.Contains(channel.name) ||
							   item.description.Contains(channel.name) ||
							   item.description.Contains(channel.url) ||
							   channel.customKeywords.Any((string keyword) => {
								   return item.channelId.Contains(keyword) ||
										  item.title.Contains(keyword) ||
										  item.description.Contains(keyword);
							   });
					}
				);
			}
		}
	}
}