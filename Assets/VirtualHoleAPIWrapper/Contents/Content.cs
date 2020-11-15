using System;

namespace VirtualHole.APIWrapper.Contents
{
	[Serializable]
	public class Content
	{
		public string title = string.Empty;
		public Platform platform = Platform.None;
		public string id = string.Empty;
		public string url = string.Empty;

		public string creator = string.Empty;
		public string creatorId = string.Empty;
		public string creatorUniversal = string.Empty;
		public string creatorIdUniversal = string.Empty;

		public DateTimeOffset creationDate = DateTimeOffset.MinValue;
		public string creationDateDisplay = string.Empty;
		public string[] tags = new string[0];
	}
}