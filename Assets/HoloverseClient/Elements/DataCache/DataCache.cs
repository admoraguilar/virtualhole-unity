
namespace Holoverse.Client.Caching
{
	using Api.Data.Contents.Creators;
	using Api.Data.Contents.Videos;

	public static partial class DataCache
	{
		public static KeyValueCache<string, Creator> creators { get; private set; } = null;
		public static KeyValueCache<string, Video> videos { get; private set; } = null;

		static DataCache()
		{
			creators = new KeyValueCache<string, Creator>((Creator c) => c.universalId);
			videos = new KeyValueCache<string, Video>((Video v) => v.id);
		}
	}
}
