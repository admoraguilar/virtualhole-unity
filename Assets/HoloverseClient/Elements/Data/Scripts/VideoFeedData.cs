using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Midnight;

namespace Holoverse.Client.Data
{
	using Api.Data;
	using Api.Data.Common;
	using Api.Data.Contents.Videos;
	using Api.Data.Contents.Creators;
	
	public class VideoFeedData
	{
		public abstract class Feed
		{
			public readonly string name = string.Empty;

			public IReadOnlyList<Video> videos => _videos;
			protected List<Video> _videos = new List<Video>();

			public bool isDone { get; protected set; } = false;

			public Feed(string name)
			{
				this.name = name;
			}

			public abstract Task<IEnumerable<Video>> LoadVideosAsync(CancellationToken cancellationToken = default);
			public abstract void Clear();
		}

		private class Feed<T> : Feed where T : Video
		{
			private HoloverseDataClient _client = null;

			private FindVideosSettings<T> _findVideosSettings = null;
			private FindResults<T> _findVideoResults = null;

			private bool _isLoading = false;

			public Feed(
				HoloverseDataClient client, string name, 
				FindVideosSettings<T> findVideosSettings) : base(name)
			{
				_client = client;
				_findVideosSettings = findVideosSettings;
			}

			public override async Task<IEnumerable<Video>> LoadVideosAsync(CancellationToken cancellationToken = default)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if(_isLoading || isDone) { return default; }
				_isLoading = true;

				if(_findVideoResults == null) {
					_findVideoResults = await _client
						.contents.videos
						.FindVideosAsync(_findVideosSettings, cancellationToken);

					if(!await _findVideoResults.MoveNextAsync()) { SetEndOfCursor(); }
				}

				IEnumerable<T> results = null;
				if(_findVideoResults != null) {
					results = _findVideoResults.current;
					_videos.AddRange(results);

					if(!await _findVideoResults.MoveNextAsync()) { SetEndOfCursor(); }
				}

				_isLoading = false;
				return results;

				void SetEndOfCursor()
				{
					MLog.Log(nameof(Feed<T>), $"No more videos found!");
					_findVideoResults.Dispose();
					_findVideoResults = null;
					isDone = true;
				}
			}

			public override void Clear()
			{
				_videos.Clear();
				_findVideoResults = null;
				isDone = false;
			}
		}

		private HoloverseDataClient _client = null;
		private FindCreatorsSettings _creatorSettings = null;

		public IReadOnlyDictionary<string, Creator> creatorLookup => _creatorLookup;
		private Dictionary<string, Creator> _creatorLookup = new Dictionary<string, Creator>();

		public IReadOnlyList<Feed> feeds => _feeds;
		private List<Feed> _feeds = new List<Feed>();

		public VideoFeedData(HoloverseDataClient client, FindCreatorsSettings creatorSettings)
		{
			_client = client;
			_creatorSettings = creatorSettings;
		}

		public VideoFeedData(HoloverseDataClient client, IEnumerable<Creator> creators)
		{
			_client = client;
			AddCreatorsToLookup(creators);
		}

		public async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			if(_creatorSettings != null) {
				using(new StopwatchScope(nameof(VideoFeedData), "Start Getting creators data...", "End")) {
					using(FindResults<Creator> results = await _client
						.contents.creators.FindCreatorsAsync(
							_creatorSettings, cancellationToken)) {
						while(await results.MoveNextAsync()) {
							AddCreatorsToLookup(results.current);
						}
					}
				}
			}

			List<Creator> creators = _creatorLookup.Values.ToList();

			_feeds.Add(
				new Feed<Video>(
				_client, "Discover",
				new FindCreatorVideosSettings<Video> {
					creators = creators,
					sortMode = FindVideosSettings<Video>.SortMode.ByCreationDate,
					isSortAscending = false
				}));

			_feeds.Add(
				new Feed<Video>(
				_client, "Community",
				new FindCreatorRelatedVideosSettings<Video> {
					creators = creators,
					sortMode = FindVideosSettings<Video>.SortMode.ByCreationDate,
					isSortAscending = false
				}));

			_feeds.Add(
				new Feed<Broadcast>(
				_client, "Live",
				new FindCreatorVideosSettings<Broadcast> {
					isBroadcast = true,
					isLive = true,
					creators = creators,
					sortMode = FindVideosSettings<Broadcast>.SortMode.BySchedule,
					isSortAscending = false
				}));

			_feeds.Add(new Feed<Broadcast>(
				_client, "Schedule",
				new FindCreatorVideosSettings<Broadcast> {
					isBroadcast = true,
					isLive = false,
					creators = creators,
					sortMode = FindVideosSettings<Broadcast>.SortMode.BySchedule,
					isSortAscending = false
				}));
		}

		private void AddCreatorsToLookup(IEnumerable<Creator> creators)
		{
			foreach(Creator result in creators) {
				_creatorLookup[result.universalId] = result;
			}
		}
	}
}
