using System.IO;
using System.Collections.Generic;
using System.Linq;
using Midnight;

namespace Holoverse.Data.YouTube
{
	public partial class YouTubeScrapeOperation
	{
		public class AggregateMap : BaseMap
		{
			public List<ChannelMap> channels { get; private set; } = new List<ChannelMap>();

			public AggregateMap(string saveDirectoryPath, YouTubeScraperSettings settings) : base(saveDirectoryPath)
			{
				channels.AddRange(
					settings.idols.SelectMany((YouTubeScraperSettings.ChannelGroup cg) => 
						cg.channels.Select((Channel ch) => {
							return new ChannelMap(Path.Combine(this.saveDirectoryPath, ch.id), ch);
						})
					)
				);

				channels.AddRange(
					settings.community.SelectMany((YouTubeScraperSettings.ChannelGroup cg) =>
						cg.channels.Select((Channel ch) => {
							return new ChannelMap(Path.Combine(this.saveDirectoryPath, ch.id), ch);
						})
					)
				);

				discover.filters.Add(new ChannelIdFilter<Video>(
					settings.idols.SelectMany((YouTubeScraperSettings.ChannelGroup cg) => cg.channels)
				));

				community.filters.Add(new ChannelIdFilter<Video>(
					settings.community.SelectMany((YouTubeScraperSettings.ChannelGroup cg) => cg.channels)
				));

				live.filters.Add(new ChannelIdFilter<Broadcast>(
					settings.idols.SelectMany((YouTubeScraperSettings.ChannelGroup cg) => cg.channels)
				));

				schedule.filters.Add(new ChannelIdFilter<Broadcast>(
					settings.idols.SelectMany((YouTubeScraperSettings.ChannelGroup cg) => cg.channels)
				));
			}

			public override void Add(Video video)
			{
				base.Add(video);
				channels.ForEach((ChannelMap map) => map.Add(video));
			}

			public override void Add(Broadcast broadcast)
			{
				base.Add(broadcast);
				channels.ForEach((ChannelMap map) => map.Add(broadcast));
			}

			public override void Save()
			{
				base.Save();
				channels.ForEach((ChannelMap map) => map.Save());
			}
		}

		public class ChannelMap : BaseMap
		{
			public readonly Channel channel = null;

			public ChannelMap(string saveDirectoryPath, Channel channel) : base(saveDirectoryPath)
			{
				this.channel = channel;

				discover.filters.Add(new ChannelIdFilter<Video>(channel));

				community.filters.Add(new ChannelIdFilter<Video>(channel) { isOpposite = true });
				community.filters.Add(new ChannelMatchFilter<Video>(channel));

				anime.filters.Add(new ChannelMatchFilter<Video>(channel));

				live.filters.Add(new ChannelIdFilter<Broadcast>(channel));
				
				schedule.filters.Add(new ChannelIdFilter<Broadcast>(channel));
			}
		}

		public abstract class BaseMap : Map
		{
			public Container<Video> discover { get; private set; } = new Container<Video>();
			public Container<Video> community { get; private set; } = new Container<Video>();
			public Container<Video> anime { get; private set; } = new Container<Video>();
			public Container<Broadcast> live { get; private set; } = new Container<Broadcast>();
			public Container<Broadcast> schedule { get; private set; } = new Container<Broadcast>();

			public BaseMap(string saveDirectoryPath) : base(saveDirectoryPath)
			{
				discover.savePath = Path.Combine(saveDirectoryPath, "discover.json");
				community.savePath = Path.Combine(saveDirectoryPath, "community.json");
				anime.savePath = Path.Combine(saveDirectoryPath, "anime.json");
				live.savePath = Path.Combine(saveDirectoryPath, "live.json");
				schedule.savePath = Path.Combine(saveDirectoryPath, "schedule.json");
			}

			public virtual void Add(Video video)
			{
				discover.Add(video);
				community.Add(video);
				anime.Add(video);
			}

			public virtual void Add(Broadcast broadcast)
			{
				live.Add(broadcast);
				schedule.Add(broadcast);
			}

			public virtual void Save()
			{
				discover.Save();
				community.Save();
				anime.Save();
				live.Save();
				schedule.Save();
			}
		}

		public abstract class Map
		{
			public readonly string saveDirectoryPath = string.Empty;

			public Map(string saveDirectoryPath)
			{
				this.saveDirectoryPath = saveDirectoryPath;
			}
		}
	}
}