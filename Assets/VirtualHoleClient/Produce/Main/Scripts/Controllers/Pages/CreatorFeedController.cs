using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using Api.DB.Contents.Creators;

	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;
	
	public class CreatorFeedController : MonoBehaviour
	{
		[SerializeField]
		private VirtualHoleDBClientObject _client = null;

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
			IEnumerable<Creator> creators = new Creator[] { CreatorCache.selectedCreator };

			_videoFeed.contextButton.image.sprite = await CreatorCache.GetAvatarAsync(CreatorCache.selectedCreator, cancellationToken);
			_videoFeed.contextButton.text.text = CreatorCache.selectedCreator.universalName;

			_videoFeed.contextButton.button.onClick.RemoveAllListeners();
			_videoFeed.contextButton.button.onClick.AddListener(() => {
				_creatorPageNode.Set();
			});

			_videoFeed.feeds.Clear();
			_videoFeed.feeds.AddRange(new VideoFeedQuery[] {
				VideoFeedQuery.CreateDiscoverFeed(_client.GetClient(), creators),
				VideoFeedQuery.CreateCommunityFeed(_client.GetClient(), creators),
				VideoFeedQuery.CreateLiveFeed(_client.GetClient(), creators),
				VideoFeedQuery.CreateScheduledFeed(_client.GetClient(), creators)
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
