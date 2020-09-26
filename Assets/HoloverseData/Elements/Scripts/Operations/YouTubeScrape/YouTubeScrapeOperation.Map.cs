using System.IO;
using System.Collections.Generic;

namespace Holoverse.Data.YouTube
{
	public partial class YouTubeScrapeOperation
	{
		public class ChannelMap
		{
			public string saveDirectoryPath = string.Empty;

			public Channel channel { get; private set; } = null;

			public Container<Video> discover { get; private set; } = null;
			public Container<Video> community { get; private set; } = null;
			public Container<Video> anime { get; private set; } = null;
			public Container<Broadcast> live { get; private set; } = null;
			public Container<Broadcast> schedule { get; private set; } = null;

			public ChannelMap(Channel channel)
			{
				this.channel = channel;

				discover.savePath = Path.Combine(saveDirectoryPath, "discover.json");
				discover.filters.Add(new ChannelIdFilter(channel));

				community.savePath = Path.Combine(saveDirectoryPath, "community.json");
				community.filters.Add(new ChannelIdFilter(channel) { isOpposite = true });
				community.filters.Add(new ChannelMatchFilter(channel));

				anime.savePath = Path.Combine(saveDirectoryPath, "anime.json");

				live.savePath = Path.Combine(saveDirectoryPath, "live.json");
				community.filters.Add(new ChannelIdFilter(channel));

				schedule.savePath = Path.Combine(saveDirectoryPath, "schedule.json");
				community.filters.Add(new ChannelIdFilter(channel));
			}
		}

		public class Map
		{

			public string saveDirectoryPath = string.Empty;

			public Container<Video> discover { get; private set; } = null;
			public Container<Video> community { get; private set; } = null;
			public Container<Video> anime { get; private set; } = null;
			public Container<Broadcast> live { get; private set; } = null;
			public Container<Broadcast> schedule { get; private set; } = null;

			public List<ChannelMap> channels { get; private set; } = null;

			public Map(YouTubeScraperSettings settings)
			{
				
			}
		}
	}
}