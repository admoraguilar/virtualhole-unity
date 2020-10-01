﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Holoverse.Data.YouTube
{
	public partial class YouTubeScrapeOperation
	{
		public class AggregateMap : BaseMap
		{
			public List<ChannelMap> channels { get; private set; } = new List<ChannelMap>();

			public AggregateMap(string saveDirectoryPath, Settings settings) : base(saveDirectoryPath)
			{
				channels.AddRange(
					settings.idols.SelectMany((ChannelGroup cg) => 
						cg.channels.Select((Channel ch) => {
							return new ChannelMap(Path.Combine(this.saveDirectoryPath, ch.id), ch);
						})
					)
				);
				channels.AddRange(
					settings.community.SelectMany((ChannelGroup cg) =>
						cg.channels.Select((Channel ch) => {
							return new ChannelMap(Path.Combine(this.saveDirectoryPath, ch.id), ch);
						})
					)
				);

				discover.filters.Add((Video video) => 
					Filters.IsChannelIdMatch(
						video,
						settings.idols.SelectMany((ChannelGroup cg) => cg.channels)
					)
				);

				community.filters.Add((Video video) =>
					Filters.IsChannelIdMatch(
						video,
						settings.community.SelectMany((ChannelGroup cg) => cg.channels)
					)
				);
				community.filters.Add((Video video) =>
					Filters.IsChannelMatch(
						video,
						settings.idols.SelectMany((ChannelGroup cg) => cg.channels)
					)
				);

				anime.filters.Add((Video video) =>
					Filters.IsChannelIdMatch(
						video,
						settings.community
							.Where((ChannelGroup cg) => cg.name.Contains("Anime"))
							.SelectMany((ChannelGroup cg) => cg.channels)
					)
				);
				anime.filters.Add((Video video) => Filters.ContainsTextInTitle(video, "【アニメ】"));

				live.filters.Add((Broadcast broadcast) =>
					Filters.IsChannelIdMatch(
						broadcast,
						settings.idols.SelectMany((ChannelGroup cg) => cg.channels)
					)
				);
				live.filters.Add((Broadcast broadcast) => Filters.IsLive(broadcast));

				schedule.filters.Add((Broadcast broadcast) =>
					Filters.IsChannelIdMatch(
						broadcast,
						settings.idols.SelectMany((ChannelGroup cg) => cg.channels)
					)
				);
				schedule.filters.Add((Broadcast broadcast) => !Filters.IsLive(broadcast));
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

				discover.filters.Add((Video video) => Filters.IsChannelIdMatch(video, channel));

				community.filters.Add((Video video) => !Filters.IsChannelIdMatch(video, channel));
				community.filters.Add((Video video) => Filters.IsChannelMatch(video, channel));

				anime.filters.Add((Video video) => Filters.IsChannelMatch(video, channel));
				anime.filters.Add((Video video) => Filters.ContainsTextInTitle(video, "【アニメ】"));

				live.filters.Add((Broadcast broadcast) => Filters.IsChannelIdMatch(broadcast, channel));
				live.filters.Add(Filters.IsLive);

				schedule.filters.Add((Broadcast broadcast) => Filters.IsChannelIdMatch(broadcast, channel));
				schedule.filters.Add((Broadcast broadcast) => !Filters.IsLive(broadcast));
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
				discover.Replace(discover.OrderByDescending((Video video) => DateTimeOffset.Parse(video.uploadDate)).ToArray());
				community.Replace(community.OrderByDescending((Video video) => DateTimeOffset.Parse(video.uploadDate)).ToArray());
				anime.Replace(anime.OrderByDescending((Video video) => DateTimeOffset.Parse(video.uploadDate)).ToArray());
				live.Replace(live.OrderByDescending((Broadcast broadcast) => DateTimeOffset.Parse(broadcast.schedule)).ToArray());
				schedule.Replace(schedule.OrderByDescending((Broadcast broadcast) => DateTimeOffset.Parse(broadcast.schedule)).ToArray());

				PostProcess(discover);
				PostProcess(community);
				PostProcess(anime);
				PostProcess(live);
				PostProcess(schedule);

				discover.Save();
				community.Save();
				anime.Save();
				live.Save();
				schedule.Save();

				void PostProcess<T>(Container<T> container)
					where T : Video
				{
					foreach(T video in container) {
						video.description = string.Empty;
					}
				}
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