using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Midnight.SOM;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data;

	using Client.UI;
	using Client.SOM;
	using Client.Data;

	public abstract class FeedPageController : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		private SceneObjectModel _som = null;
		private Node _mainNode = null;
		private Node _optionsNode = null;
		private Page _page = null;
		private Section _videoFeedSection = null;
		private VideoFeedScroll _videoFeed = null;

		protected abstract CreatorQuery CreateCreatorQuery(HoloverseDataClient client);

		private async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			CreatorQuery creatorQuery = CreateCreatorQuery(_client.client);
			await creatorQuery.LoadAsync(cancellationToken);

			await _videoFeed.InitializeAsync(
				new VideoFeedQuery[] {
					VideoFeedQuery.CreateDiscoverFeed(_client.client, creatorQuery.creatorLookup.Values),
					VideoFeedQuery.CreateCommunityFeed(_client.client, creatorQuery.creatorLookup.Values),
					VideoFeedQuery.CreateLiveFeed(_client.client, creatorQuery.creatorLookup.Values),
					VideoFeedQuery.CreateScheduledFeed(_client.client, creatorQuery.creatorLookup.Values)
				},
				OnVideoScrollRectCellDataCreated, 
				cancellationToken);
		}

		private async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			await _videoFeed.LoadAsync(OnVideoScrollRectCellDataCreated, cancellationToken);
		}

		private async Task UnloadAsync()
		{
			await _videoFeed.UnloadAsync();
		}

		protected virtual async void OnNodeVisit()
		{
			await _page.InitializeAsync();
		}

		private void OnVideoScrollRectCellDataCreated(VideoScrollRectCellData cellData)
		{
			cellData.onOptionsClick += _optionsNode.Push;
		}

		private async void OnDropdownValueChanged(int value)
		{
			_videoFeed.ClearFeed();

			_videoFeedSection.SetDisplayActive(_videoFeedSection.GetDisplay(Section.DisplayType.LoadingIndicator), true);
			await _videoFeedSection.LoadAsync();
			_videoFeedSection.SetDisplayActive(_videoFeedSection.GetDisplay(Section.DisplayType.Content), true);
		}

		private async void OnNearScrollEnd()
		{
			await _page.LoadAsync();
		}

		protected virtual void SetReferences(
			ref Page page, ref Section videoFeedSection,
			ref VideoFeedScroll videoFeed, ref Node mainNode) { }

		private void Awake()
		{
			_som = SceneObjectModel.Get(this);
			_optionsNode = _som.GetCachedComponent<MainFlowMap>().creatorPageNode;

			SetReferences(
				ref _page, ref _videoFeedSection, 
				ref _videoFeed, ref _mainNode);
		}

		private void OnEnable()
		{
			_mainNode.OnVisit += OnNodeVisit;

			_videoFeedSection.InitializeTaskFactory += InitializeAsync;
			_videoFeedSection.LoadTaskFactory += LoadAsync;
			_videoFeedSection.UnloadTaskFactory += UnloadAsync;

			_videoFeed.OnDropdownValueChangedCallback += OnDropdownValueChanged;
			_videoFeed.OnNearScrollEnd += OnNearScrollEnd;
		}

		private void OnDisable()
		{
			_mainNode.OnVisit -= OnNodeVisit;

			_videoFeedSection.InitializeTaskFactory -= InitializeAsync;
			_videoFeedSection.LoadTaskFactory -= LoadAsync;
			_videoFeedSection.UnloadTaskFactory -= UnloadAsync;

			_videoFeed.OnDropdownValueChangedCallback -= OnDropdownValueChanged;
			_videoFeed.OnNearScrollEnd -= OnNearScrollEnd;
		}
	}
}
