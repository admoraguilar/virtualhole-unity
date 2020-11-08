using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Humanizer;
using Midnight;
using Midnight.Web;

namespace VirtualHole.Client.Data
{
	using Api.DB;
	using Api.DB.Common;
	using Api.DB.Contents.Videos;
	using Api.DB.Contents.Creators;
	using Client.UI;

	public class VideoDTO<T> : DataQueryDTO<T>
		where T : Video
	{
		public CreatorDTO creatorDTO = new CreatorDTO(null);
		public Sprite indicatorSprite = null;
		public Sprite thumbnailSprite = null;
		public string creationDateDisplay = string.Empty;
		public string scheduleDateDisplay = string.Empty;

		public VideoDTO(T video) : base(video) 
		{ }
	}

	public class VideoFeedQuerySettings : PaginatedQuerySettings<Video, VideoDTO<Video>>
	{
		public VirtualHoleDBClient dbClient { get; set; } = null;
		public ICache<CreatorDTO> creatorDTOCache { get; set; } = null; 

		public VideoFeedQuerySettings() : base()
		{
			dbClient = VirtualHoleDBClientFactory.Get();
			creatorDTOCache = SimpleCache<CreatorDTO>.Get();
		}
	}

	public class VideoFeedQuery<T> : VideoFeedQuery
		where T : Video
	{
		private FindVideosSettings<T> _findVideosSettings = null;
		private FindResults<T> _findVideoResults = null;

		public VideoFeedQuery(
			string name, FindVideosSettings<T> findVideosSettings = null,
			VideoFeedQuerySettings querySettings = null) 
			: base(name, querySettings)
		{
			_findVideosSettings = findVideosSettings;
		}

		protected override async Task PreProcessDTOAsync(IEnumerable<Video> raws, CancellationToken cancellationToken = default)
		{
			List<string> creatorIdsToLoad = new List<string>(); 
			foreach(Video raw in raws) {
				string creatorIdUniversal = raw.creatorIdUniversal;
				if(!_querySettings.creatorDTOCache.Contains(creatorIdUniversal)) {
					creatorIdsToLoad.Add(creatorIdUniversal);
				}
			}

			if(creatorIdsToLoad.Count > 0) {
				CreatorQuery creatorQuery = new CreatorQuery(
					new FindCreatorsRegexSettings() {
						searchQueries = creatorIdsToLoad,
						isCheckForAffiliations = false,
						isCheckCustomKeywords = false,
						isCheckForDepth = false,
						isCheckSocialCustomKeywords = false,
						isCheckSocialName = false,
						isCheckUniversalName = false,
					});

				await creatorQuery.GetDTOAsync(cancellationToken);
			}
		}

		protected override async Task ProcessDTOAsync(VideoDTO<Video> dto, CancellationToken cancellationToken = default)
		{
			if(_querySettings.creatorDTOCache.TryGet(dto.raw.creatorIdUniversal, out CreatorDTO creatorDTO)) {
				dto.creatorDTO = creatorDTO;
			}

			//dto.thumbnailSprite = await ImageGetWebRequest.GetAsync(dto.raw.thumbnailUrl, null, cancellationToken);
			dto.thumbnailSprite = await QueryUtilities.GetImageAsync(dto.raw.thumbnailUrl, cancellationToken);
			dto.creationDateDisplay = dto.raw.creationDate.Humanize();

			if(dto.raw is Broadcast broadcast) {
				dto.indicatorSprite = UIResources.GetIndicatorSprite(broadcast.isLive);
				dto.scheduleDateDisplay = broadcast.schedule.Humanize();
			}
		}

		protected override string GetCacheKey(Video raw)
		{
			return raw.id;
		}

		protected override VideoDTO<Video> FromRawToDTO(Video raw)
		{
			return new VideoDTO<Video>(raw);
		}

		protected override async Task<IEnumerable<Video>> GetRawAsync_Impl(CancellationToken cancellationToken = default)
		{
			IEnumerable<Video> results = default;

			using(new StopwatchScope(
					GetType().Name,
					$"Start load {name}...",
					$"End load {name}.")) {
				if(_findVideoResults == null) {
					VideoClient videoClient = _querySettings.dbClient.contents.videos;
					_findVideoResults = await videoClient.FindVideosAsync(_findVideosSettings, cancellationToken);
					if(!await _findVideoResults.MoveNextAsync()) { DisposeResults(); }
				}

				if(_findVideoResults != null) {
					results = _findVideoResults.current;
					if(!await _findVideoResults.MoveNextAsync()) { DisposeResults(); }
				}
			}

			return results;
		}

		public override void Reset()
		{
			base.Reset();
			_findVideoResults = null;
		}

		private void DisposeResults()
		{
			MLog.Log(GetType().Name, $"No more [{name}] videos found!");
			_findVideoResults.Dispose();
			_findVideoResults = null;
			isDone = true;
		}
	}

	public abstract class VideoFeedQuery : PaginatedQuery<Video, VideoDTO<Video>, VideoFeedQuerySettings>
	{
		public static VideoFeedQuery CreateDiscoverFeed(
			IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Video>(
				"Discover",
				new FindCreatorVideosSettings<Video>() {
					creators = new List<Creator>(creators),
					sortMode = FindVideosSettings<Video>.SortMode.ByCreationDate,
					isSortAscending = false,
					batchSize = batchSize
				});

		public static VideoFeedQuery CreateCommunityFeed(
			IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Video>(
				"Community",
				new FindCreatorRelatedVideosSettings<Video> {
					creators = new List<Creator>(creators),
					sortMode = FindVideosSettings<Video>.SortMode.ByCreationDate,
					isSortAscending = false,
					batchSize = batchSize,
				});

		public static VideoFeedQuery CreateLiveFeed(
			IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Broadcast>(
				"Live",
				new FindCreatorVideosSettings<Broadcast> {
					isBroadcast = true,
					isLive = true,
					creators = new List<Creator>(creators),
					sortMode = FindVideosSettings<Broadcast>.SortMode.BySchedule,
					isSortAscending = false,
					batchSize = batchSize
				});

		public static VideoFeedQuery CreateScheduledFeed(
			IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Broadcast>(
				"Schedule",
				new FindCreatorVideosSettings<Broadcast> {
					isBroadcast = true,
					isLive = false,
					creators = new List<Creator>(creators),
					sortMode = FindVideosSettings<Broadcast>.SortMode.BySchedule,
					isSortAscending = false,
					batchSize = batchSize
				});

		public string name { get; protected set; } = string.Empty;

		public VideoFeedQuery(string name, VideoFeedQuerySettings querySettings = null) : base(querySettings)
		{
			this.name = name;
		}
	}
}
