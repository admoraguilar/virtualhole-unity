using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Midnight.Web;

namespace Holoverse.Client.Data
{
	using Api.Data.Contents;
	using Api.Data.Contents.Videos;

	public static class VideoCache
	{
		private static Dictionary<string, Video> _videoLookup = new Dictionary<string, Video>();
		private static Dictionary<string, Sprite> _thumbnailLookup = new Dictionary<string, Sprite>();

		public static async Task<Sprite> GetThumbnailAsync(
			Platform platform, string id, CancellationToken cancellationToken = default)
		{
			string key = CreateLookupKey(platform, id);
			if(!_thumbnailLookup.TryGetValue(key, out Sprite thumbnail)) {
				if(_videoLookup.TryGetValue(key, out Video video)) {
					_thumbnailLookup[key] =
						thumbnail = await ImageGetWebRequest.GetAsync(
							video.thumbnailUrl, null,
							cancellationToken);
				}
			}

			return thumbnail;
		}

		public static Video Get(Platform platform, string id)
		{
			_videoLookup.TryGetValue(CreateLookupKey(platform, id), out Video video);
			return video;
		}

		public static void Add(IEnumerable<Video> videos)
		{
			if(videos == null) { return; }
			foreach(Video video in videos) { Add(video); }
		}

		public static void Add(Video video)
		{
			Assert.IsNotNull(video);
			_videoLookup[CreateLookupKey(video.platform, video.id)] = video;
		}

		private static string CreateLookupKey(Platform platform, string id) => $"{platform}-{id}";
	}
}
