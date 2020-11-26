using VirtualHole.APIWrapper.Contents.Videos;
using VirtualHole.APIWrapper.Contents.Creators;

namespace VirtualHole.APIWrapper.Contents
{
	public class ContentClient
	{
		public CreatorClient creators { get; private set; } = null;
		public VideoClient videos { get; private set; } = null;

		public ContentClient(string domain)
		{
			creators = new CreatorClient(domain);
			videos = new VideoClient(domain);
		}
	}
}
