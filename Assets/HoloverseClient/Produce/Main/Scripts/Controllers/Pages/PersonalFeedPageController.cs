using System.Threading.Tasks;
using Midnight.SOM;
using Midnight.Pages;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Api.Data;

	using Client.UI;
	using Client.SOM;
	using Client.Data;

	public class PersonalFeedPageController : FeedPageController
	{
		private Section _emptyFeedSection = null;

		protected override VideoFeedData CreateVideoFeedData(HoloverseDataClient client) => null;

		protected override async void OnNodeVisit()
		{
			await Task.CompletedTask;
			_emptyFeedSection.gameObject.SetActive(true);
			await _emptyFeedSection.LoadAsync();
		}

		protected override void SetReferences(
			ref Page page, ref Section videoFeedSection,
			ref VideoFeed videoFeed, ref Node mainNode)
		{
			SceneObjectModel som = SceneObjectModel.Get(this);
			page = som.GetCachedComponent<MainFlowPersonalFeedMap>().page;
			videoFeedSection = som.GetCachedComponent<MainFlowPersonalFeedMap>().videoSection;
			videoFeed = som.GetCachedComponent<MainFlowPersonalFeedMap>().videoFeed;
			mainNode = som.GetCachedComponent<MainFlowMap>().personalFeedNode;

			_emptyFeedSection = som.GetCachedComponent<MainFlowPersonalFeedMap>().emptySection;
		}
	}
}
