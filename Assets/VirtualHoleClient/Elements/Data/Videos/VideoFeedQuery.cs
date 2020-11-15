using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;

namespace VirtualHole.Client.Data
{
	using APIWrapper;
	using APIWrapper.Contents.Videos;
	using APIWrapper.Contents.Creators;
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
		public VirtualHoleAPIWrapperClient apiWrapperClient { get; set; } = null;
		public ICache<CreatorDTO> creatorDTOCache { get; set; } = null; 

		public VideoFeedQuerySettings() : base()
		{
			apiWrapperClient = VirtualHoleAPIWrapperClientFactory.Get();
			creatorDTOCache = SimpleCache<CreatorDTO>.Get();
		}
	}

	public class VideoFeedQuery<T> : VideoFeedQuery
		where T : Video
	{
		private ListVideosRequest _request = null;
		private int _page = 0;

		public VideoFeedQuery(
			string name, ListVideosRequest request = null,
			VideoFeedQuerySettings querySettings = null) 
			: base(name, querySettings)
		{
			_request = request;
			_request.skip = 0;
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
					new ListCreatorsRegexRequest() {
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

			dto.thumbnailSprite = await HTTPUtilities.GetImageAsync(dto.raw.thumbnailUrl, cancellationToken);
			dto.creationDateDisplay = dto.raw.creationDateDisplay;

			if(dto.raw is Broadcast broadcast) {
				dto.indicatorSprite = UIResources.GetIndicatorSprite(broadcast.isLive);
				dto.scheduleDateDisplay = broadcast.scheduleDateDisplay;
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
			List<Video> results = new List<Video>();

			using(new StopwatchScope(
					GetType().Name,
					$"Start load {name}...",
					$"End load {name}.")) {
				_request.skip = _request.batchSize * _page;

				VideoClient videoClient = _querySettings.apiWrapperClient.contents.videos;
				results.AddRange(await videoClient.ListVideosAsync<T, ListVideosRequest>(_request, cancellationToken));
				_page++;

				if(results.Count <= 0) {
					DisposeResults();
				}
			}

			return results;
		}

		public override void Reset()
		{
			base.Reset();
			_page = 0;
		}

		private void DisposeResults()
		{
			MLog.Log(GetType().Name, $"No more [{name}] videos found!");
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
				new ListCreatorVideosRequest() {
					creators = new List<Creator>(creators),
					sortMode = SortMode.ByCreationDate,
					isSortAscending = false,
					batchSize = batchSize
				});

		public static VideoFeedQuery CreateCommunityFeed(
			IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Video>(
				"Community",
				new ListCreatorRelatedVideosRequest() {
					creators = new List<Creator>(creators),
					sortMode = SortMode.ByCreationDate,
					isSortAscending = false,
					batchSize = batchSize,
				});

		public static VideoFeedQuery CreateLiveFeed(
			IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Broadcast>(
				"Live",
				new ListCreatorVideosRequest() {
					isBroadcast = true,
					isLive = true,
					creators = new List<Creator>(creators),
					sortMode = SortMode.BySchedule,
					isSortAscending = false,
					batchSize = batchSize
				});

		public static VideoFeedQuery CreateScheduledFeed(
			IEnumerable<Creator> creators,
			int batchSize = 20) =>
			new VideoFeedQuery<Broadcast>(
				"Schedule",
				new ListCreatorVideosRequest() {
					isBroadcast = true,
					isLive = false,
					creators = new List<Creator>(creators),
					sortMode = SortMode.BySchedule,
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
