using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.SOM;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data.Contents.Creators;

	using Client.UI;
	using Client.SOM;
	using Client.Data;

	public class CreatorPageController : MonoBehaviour
	{
		[SerializeField]
		private HoloverseDataClientObject _client = null;

		private SceneObjectModel _som = null;
		private Node _creatorPageNode = null;
		private Page _page = null;
		private Section _creatorPageSection = null;
		private CreatorView _creatorView = null;

		private async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			Creator creator = CreatorCache.creator;
			if(creator == null) { return; }

			IEnumerable<Creator> creators = new Creator[] { creator };
			await _creatorView.LoadCreatorAsync(
				creator,
				new VideoFeedQuery[] {
					VideoFeedQuery.CreateDiscoverFeed(_client.client, creators),
					VideoFeedQuery.CreateCommunityFeed(_client.client, creators),
					VideoFeedQuery.CreateLiveFeed(_client.client, creators),
					VideoFeedQuery.CreateScheduledFeed(_client.client, creators)
				},
				cancellationToken);
		}

		private async Task UnloadAsync()
		{
			await _creatorView.UnloadAsync();
		}

		private async void OnCreatorPageVisit()
		{
			await _page.InitializeAsync();
		}

		private void Awake()
		{
			_som = SceneObjectModel.Get(this);
			_creatorPageNode = _som.GetCachedComponent<MainFlowMap>().creatorPageNode;
			_page = _som.GetCachedComponent<MainFlowCreatorPageMap>().page;
			_creatorPageSection = _som.GetCachedComponent<MainFlowCreatorPageMap>().creatorPageSection;
			_creatorView = _som.GetCachedComponent<MainFlowCreatorPageMap>().creatorView;
		}

		private void OnEnable()
		{
			_creatorPageNode.OnVisit += OnCreatorPageVisit;

			_creatorPageSection.InitializeTaskFactory += InitializeAsync;
			_creatorPageSection.UnloadTaskFactory += UnloadAsync;
		}

		private void OnDisable()
		{
			_creatorPageSection.InitializeTaskFactory -= InitializeAsync;
			_creatorPageSection.UnloadTaskFactory -= UnloadAsync;
		}
	}
}
