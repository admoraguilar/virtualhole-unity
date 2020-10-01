using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Concurrency;
using Midnight;
using TMPro;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Holoverse.Data.YouTube
{
	[CreateAssetMenu(menuName = "Holoverse/Data/YouTube/Scraper Settings")]
	public partial class YouTubeScraperSettings : ScriptableObject
	{
		[SerializeField]
		private YouTubeScrapeOperation.Settings _settings = new YouTubeScrapeOperation.Settings();

		public List<ChannelGroup> idols => _idols;
		[Space]
		[SerializeField]
		private List<ChannelGroup> _idols = new List<ChannelGroup>();

		public List<ChannelGroup> community => _community;
		[SerializeField]
		private List<ChannelGroup> _community = new List<ChannelGroup>();

		private async Task Execute()
		{
			MLog.Log("=====START SCRAPING====="); 

			YouTubeScrapeOperation operation = new YouTubeScrapeOperation(this);
			await operation.Execute();
			operation.Save();

			MLog.Log("=====END SCRAPING=====");
		}

#if UNITY_EDITOR
		[MenuItem("CONTEXT/YouTubeScraperSettings/Test Execute")]
		private static void TestExecute(MenuCommand command)
		{
			YouTubeScraperSettings settings = (YouTubeScraperSettings)command.context;
			TaskExt.FireForget(settings.Execute());
		}

		[MenuItem("CONTEXT/YouTubeScraperSettings/Migrate")]
		private static void Migrate(MenuCommand command)
		{
			YouTubeScraperSettings settings = (YouTubeScraperSettings)command.context;
			settings._settings.idols.Clear();
			settings._settings.idols.AddRange(settings.idols.Select(
				idol => {
					return new YouTubeScrapeOperation.ChannelGroup {
						name = idol.name,
						channels = new List<Channel>(idol.channels)
					};
				}
			));
			settings._settings.community.Clear();
			settings._settings.community.AddRange(settings.community.Select(
				community => {
					return new YouTubeScrapeOperation.ChannelGroup {
						name = community.name,
						channels = new List<Channel>(community.channels)
					};
				}
			));
		}
#endif
	}

	public partial class YouTubeScraperSettings
	{
		[Serializable]
		public class ChannelGroup
		{
			public string name = string.Empty;
			public List<Channel> channels = new List<Channel>();
		}
	}
}
