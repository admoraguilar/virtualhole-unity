using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data;
	using Api.Data.Contents.Creators;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;
	
	public class CreatorFeedController : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		private Node _creatorPageNode => _mainFlowMap.creatorPageNode;
		private Node _creatorFeedNode => _mainFlowMap.creatorFeedNode;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private Page _page => _creatorFeedMap.page;
		private Section _videoFeedSection => _creatorFeedMap.videoSection;
		private VideoFeedScroll _videoFeed => _creatorFeedMap.videoFeed;
		[Space]
		[SerializeField]
		private MainFlowCreatorFeedMap _creatorFeedMap = null;

		private async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			IEnumerable<Creator> creators = new Creator[] { CreatorCache.creator };

			_videoFeed.contextButton.image.sprite = await CreatorCache.GetAvatarAsync(CreatorCache.creator, cancellationToken);
			_videoFeed.contextButton.text.text = CreatorCache.creator.universalName;

			_videoFeed.contextButton.button.onClick.RemoveAllListeners();
			_videoFeed.contextButton.button.onClick.AddListener(() => {
				_creatorPageNode.Set();
			});

			await _videoFeed.InitializeAsync(
				new VideoFeedQuery[] {
					VideoFeedQuery.CreateDiscoverFeed(_client.client, creators),
					VideoFeedQuery.CreateCommunityFeed(_client.client, creators),
					VideoFeedQuery.CreateLiveFeed(_client.client, creators),
					VideoFeedQuery.CreateScheduledFeed(_client.client, creators)
				},
				cancellationToken);
		}

		private async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			await _videoFeed.LoadAsync(cancellationToken);
		}

		private async Task UnloadAsync()
		{
			await _videoFeed.UnloadAsync();
		}

		private async void OnNodeVisit()
		{
			await _page.UnloadAsync();
			await _page.InitializeAsync();
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

		private void OnEnable()
		{
			_creatorFeedNode.OnVisit += OnNodeVisit;

			_videoFeedSection.InitializeTaskFactory += InitializeAsync;
			_videoFeedSection.LoadTaskFactory += LoadAsync;
			_videoFeedSection.UnloadTaskFactory += UnloadAsync;

			_videoFeed.OnDropdownValueChangedCallback += OnDropdownValueChanged;
			_videoFeed.OnNearScrollEnd += OnNearScrollEnd;
		}

		private void OnDisable()
		{
			_creatorFeedNode.OnVisit -= OnNodeVisit;

			_videoFeedSection.InitializeTaskFactory -= InitializeAsync;
			_videoFeedSection.LoadTaskFactory -= LoadAsync;
			_videoFeedSection.UnloadTaskFactory -= UnloadAsync;

			_videoFeed.OnDropdownValueChangedCallback -= OnDropdownValueChanged;
			_videoFeed.OnNearScrollEnd -= OnNearScrollEnd;
		}
	}
}
