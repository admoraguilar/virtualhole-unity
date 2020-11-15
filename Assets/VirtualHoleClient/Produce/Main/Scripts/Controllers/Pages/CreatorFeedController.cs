using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using APIWrapper.Contents.Creators;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;
	
	public class CreatorFeedController : MonoBehaviour
	{
		private Node _creatorPageNode => _mainFlowMap.creatorPageNode;
		private Node _creatorFeedNode => _mainFlowMap.creatorFeedNode;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private VideoFeedScroll _videoFeed => _creatorFeedFlowMap.videoFeed;
		[SerializeField]
		private CreatorFeedFlowMap _creatorFeedFlowMap = null;

		private async Task VideoFeedDataFactoryAsync(CancellationToken cancellationToken = default)
		{
			await Task.CompletedTask;

			CreatorDTO selectedCreatorDTO = Selection.instance.creatorDTO;
			_videoFeed.contextButton.image.sprite = selectedCreatorDTO.avatarSprite;
			_videoFeed.contextButton.text.text = selectedCreatorDTO.raw.universalName;
			_videoFeed.contextButton.button.onClick.RemoveAllListeners();
			_videoFeed.contextButton.button.onClick.AddListener(() => {
				_creatorPageNode.Set();
			});

			IEnumerable<Creator> creators = new Creator[] { selectedCreatorDTO.raw };
			_videoFeed.feeds.Clear();
			_videoFeed.feeds.AddRange(new VideoFeedQuery[] {
				VideoFeedQuery.CreateDiscoverFeed(creators),
				VideoFeedQuery.CreateCommunityFeed(creators),
				VideoFeedQuery.CreateLiveFeed(creators),
				VideoFeedQuery.CreateScheduledFeed(creators)
			});
		}

		private async void OnCreatorFeedVisit()
		{
			await _videoFeed.UnloadAsync();

			_videoFeed.SetDataAsyncFactory(VideoFeedDataFactoryAsync);
			await _videoFeed.InitializeAsync();
		}

		private void OnEnable()
		{
			_creatorFeedNode.OnVisit += OnCreatorFeedVisit;
		}

		private void OnDisable()
		{
			_creatorFeedNode.OnVisit -= OnCreatorFeedVisit;
		}
	}
}
