using System;
using System.Collections.Generic;
using Midnight;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Holoverse.Data.YouTube
{
	public partial class YouTubeScraperSettings
	{
		[Serializable]
		public class ChannelGroup
		{
			public string name = string.Empty;
			public List<Channel> channels = new List<Channel>();
		}

		[Serializable]
		public class Channel
		{
			public string name = string.Empty;
			public string id = string.Empty;
			public string url = string.Empty;
		}
	}

	[CreateAssetMenu(menuName = "Holoverse/Data/YouTube/Scraper Settings")]
	public partial class YouTubeScraperSettings : ScriptableObject
	{
		[SerializeField]
		private TextAsset _idolSource = null;

		[SerializeField]
		private TextAsset _communitySource = null;

		public List<ChannelGroup> idols => _idols;
		[Space]
		[SerializeField]
		private List<ChannelGroup> _idols = new List<ChannelGroup>();

		public List<ChannelGroup> community => _community;
		[SerializeField]
		private List<ChannelGroup> _community = new List<ChannelGroup>();

#if UNITY_EDITOR
		[MenuItem("CONTEXT/YouTubeScraperSettings/Convert Text Source")]
		private static void ConvertTextSource(MenuCommand command)
		{
			YouTubeScraperSettings settings = (YouTubeScraperSettings)command.context;

			Process(settings._idolSource.text, settings.idols);
			Process(settings._communitySource.text, settings.community);

			void Process(string source, List<ChannelGroup> channelGroups)
			{
				channelGroups.Clear();

				ChannelGroup currentChannelGroup = new ChannelGroup();
				channelGroups.Add(currentChannelGroup);

				IReadOnlyList<string> lines = TextFileUtilities.GetNLSV(source, StringSplitOptions.None);
				foreach(string line in lines) {
					if(string.IsNullOrEmpty(line)) {
						currentChannelGroup = new ChannelGroup();
						channelGroups.Add(currentChannelGroup);
						continue;
					}

					IReadOnlyList<string> value = TextFileUtilities.GetCSV(line);
					currentChannelGroup.name = string.Empty;
					currentChannelGroup.channels.Add(new Channel {
						name = value[2].Substring(1),
						id = value[0].Split('/')[4],
						url = value[0],
					});
				}
			}
		}
#endif
	}
}
