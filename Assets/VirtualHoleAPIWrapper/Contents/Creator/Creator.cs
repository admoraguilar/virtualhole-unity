using System;

namespace VirtualHole.APIWrapper.Contents.Creators
{
	[Serializable]
	public class Creator
	{
		public string universalName = string.Empty;
		public string universalId = string.Empty;
		public string wikiUrl = string.Empty;
		public string avatarUrl = string.Empty;

		public bool isHidden = false;

		public string[] affiliations = new string[0];
		public bool isGroup = false;
		public int depth = 0;

		public Social[] socials = new Social[0];
		public string[] customKeywords = new string[0];
	}
}
