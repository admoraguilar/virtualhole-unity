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
		private CreatorFeedFlowMap _creatorFeedMap = null;

		private async Task VideoFeedDataFactoryAsync(CancellationToken cancellationToken = default)
		{
			IEnumerable<Creator> creators = new Creator[] { CreatorCache.creator };

			_videoFeed.contextButton.image.sprite = await CreatorCache.GetAvatarAsync(CreatorCache.creator, cancellationToken);
			_videoFeed.contextButton.text.text = CreatorCache.creator.universalName;

			_videoFeed.contextButton.button.onClick.RemoveAllListeners();
			_videoFeed.contextButton.button.onClick.AddListener(() => {
				_creatorPageNode.Set();
			});

			_videoFeed.SetData(new VideoFeedQuery[] {
				VideoFeedQuery.CreateDiscoverFeed(_client.client, creators),
				VideoFeedQuery.CreateCommunityFeed(_client.client, creators),
				VideoFeedQuery.CreateLiveFeed(_client.client, creators),
				VideoFeedQuery.CreateScheduledFeed(_client.client, creators)
			});
		}

		protected async void OnNodeVisit()
		{
			_videoFeed.SetData(VideoFeedDataFactoryAsync);
			await _videoFeed.InitializeAsync();
		}

		private void OnEnable()
		{
			_creatorFeedNode.OnVisit += OnNodeVisit;
		}

		private void OnDisable()
		{
			_creatorFeedNode.OnVisit -= OnNodeVisit;
		}
	}
}
