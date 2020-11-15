
namespace VirtualHole.APIWrapper.Contents
{
	using Videos;
	using Creators;

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
