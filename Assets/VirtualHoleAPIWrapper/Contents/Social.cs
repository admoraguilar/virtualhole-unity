using System;

namespace VirtualHole.APIWrapper.Contents
{
	[Serializable]
	public class Social
	{
		public string name;
		public Platform platform;
		public string id;
		public string url;
		public string avatarUrl;

		public string[] customKeywords = new string[0];
	}
}