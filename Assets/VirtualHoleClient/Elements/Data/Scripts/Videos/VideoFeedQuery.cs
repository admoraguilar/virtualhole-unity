using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Midnight;

namespace VirtualHole.Client.Data
{
	using Api.DB;
	using Api.DB.Common;
	using Api.DB.Contents.Videos;
	using Api.DB.Contents.Creators;
	
	public class VideoFeedQuery<T> : VideoFeedQuery where T : Video
	{
		private FindVideosSettings<T> _findVideosSettings = null;
		private FindResults<T> _findVideoResults = null;

		private bool _isLoading = false;

		public VideoFeedQuery(
			VirtualHoleDBClient client, string name, 
			FindVideosSettings<T> findVideosSettings = null) : base(client, name)
		{
			_findVideosSettings = findVideosSettings;
		}

		public override async Task<IEnumerable<Video>> LoadAsync(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if(_isLoading || isDone) { return default; }
			_isLoading = true;

			IEnumerable<T> results = null;
			using(new StopwatchScope(nameof(VideoFeedQuery<T>), $"Start load [{name}]...", $"End load [{name}].")) {
				if(_findVideoResults == null) {
					_findVideoResults = await client
						.contents.videos
						.FindVideosAsync(_findVideosSettings, cancellationToken);

					if(!await _findVideoResults.MoveNextAsync()) { DisposeResults(); }
				}

				if(_findVideoResults != null) {
					results = _findVideoResults.current;
					_videos.AddRange(results);

					if(!await _findVideoResults.MoveNextAsync()) { DisposeResults(); }
				}
			}

			VideoCache.Add(results);
			
			_isLoading = false;
			return results;
		}

		public override void Clear()
		{
			_videos.Clear();
			_findVideoResults = null;
			isDone = false;
		}

		private void DisposeResults()
		{
			MLog.Log(nameof(VideoFeedQuery<T>), $"No more [{name}] videos found!");
			_findVideoResults.Dispose();
			_findVideoResults = null;
			isDone = true;
		}
	}

	public abstract class VideoFeedQuery
	{
		public static VideoFeedQuery CreateDiscoverFeed(
			VirtualHoleDBClient client, IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Video>(
				client, "Discover",
				new FindCreatorVideosSettings<Video>() {
					creators = new List<Creator>(creators),
					sortMode = FindVideosSettings<Video>.SortMode.ByCreationDate,
					isSortAscending = false,
					batchSize = batchSize
				});

		public static VideoFeedQuery CreateCommunityFeed(
			VirtualHoleDBClient client, IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Video>(
				client, "Community",
				new FindCreatorRelatedVideosSettings<Video> {
					creators = new List<Creator>(creators),
					sortMode = FindVideosSettings<Video>.SortMode.ByCreationDate,
					isSortAscending = false,
					batchSize = batchSize,
				});

		public static VideoFeedQuery CreateLiveFeed(
			VirtualHoleDBClient client, IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Broadcast>(
				client, "Live",
				new FindCreatorVideosSettings<Broadcast> {
					isBroadcast = true,
					isLive = true,
					creators = new List<Creator>(creators),
					sortMode = FindVideosSettings<Broadcast>.SortMode.BySchedule,
					isSortAscending = false,
					batchSize = batchSize
				});

		public static VideoFeedQuery CreateScheduledFeed(
			VirtualHoleDBClient client, IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Broadcast>(
				client, "Schedule",
				new FindCreatorVideosSettings<Broadcast> {
					isBroadcast = true,
					isLive = false,
					creators = new List<Creator>(creators),
					sortMode = FindVideosSettings<Broadcast>.SortMode.BySchedule,
					isSortAscending = false,
					batchSize = batchSize
				});

		protected VirtualHoleDBClient client { get; private set; } = null;

		public string name { get; protected set; } = string.Empty;
		public bool isDone { get; protected set; } = false;

		public IReadOnlyList<Video> videos => _videos;
		protected List<Video> _videos = new List<Video>();

		public VideoFeedQuery(VirtualHoleDBClient client, string name)
		{
			this.client = client;
			this.name = name;
		}

		public abstract Task<IEnumerable<Video>> LoadAsync(CancellationToken cancellationToken = default);
		public abstract void Clear();
	}
}
