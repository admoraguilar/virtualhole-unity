using System;

namespace Holoverse.Data.YouTube
{
	[Serializable]
	public class Channel
	{
		public string name = string.Empty;
		public string id = string.Empty;
		public string url = string.Empty;
		public string avatarUrl = string.Empty;
		public string[] customKeywords = new string[0];
	}
}
