using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midnight.SOM;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
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
			await _creatorView.LoadCreatorAsync(
				null,
				new VideoFeedQuery[] {
					VideoFeedQuery.CreateDiscoverFeed(_client.client, null),
					VideoFeedQuery.CreateCommunityFeed(_client.client, null),
					VideoFeedQuery.CreateLiveFeed(_client.client, null),
					VideoFeedQuery.CreateScheduledFeed(_client.client, null)
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
