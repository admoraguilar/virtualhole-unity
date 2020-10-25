
namespace Holoverse.Client.Data
{
	using Api.Data.Contents.Creators;
	using Api.Data.Contents.Videos;

	public static class DataCacheKeys
	{
		public static readonly string creatorGroup = nameof(Creator);
		public static readonly string creatorAvatarGroup = $"{nameof(Creator)}.{nameof(Creator.avatarUrl)}";

		public static readonly string videoGroup = nameof(Video);
		public static readonly string videoThumbnailGroup = $"{nameof(Video)}.{nameof(Video.thumbnailUrl)}";
	}
}
