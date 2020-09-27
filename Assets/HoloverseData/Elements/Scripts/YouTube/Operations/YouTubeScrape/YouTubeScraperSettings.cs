using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Concurrency;
using Midnight;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Holoverse.Data.YouTube
{
	[CreateAssetMenu(menuName = "Holoverse/Data/YouTube/Scraper Settings")]
	public partial class YouTubeScraperSettings : ScriptableObject
	{
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
