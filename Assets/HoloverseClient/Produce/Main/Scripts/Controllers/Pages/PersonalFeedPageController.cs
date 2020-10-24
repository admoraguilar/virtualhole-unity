using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Midnight.Pages;

namespace Holoverse.Client.Controllers
{
	using Api.Data;

	using Client.Data;

	public class PersonalFeedPageController : FeedPageController
	{
		[Space]
		[Header("References")]
		[SerializeField]
		private Section _emptyFeedSection = null;

		protected override VideoFeedData CreateVideoFeedData(HoloverseDataClient client) => null;

		protected override async void OnNodeVisit()
		{
			await Task.CompletedTask;
			_emptyFeedSection.gameObject.SetActive(true);
			await _emptyFeedSection.LoadAsync();
		}
	}
}
