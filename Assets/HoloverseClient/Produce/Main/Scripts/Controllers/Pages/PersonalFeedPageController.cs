using System.Collections.Generic;
using UnityEngine;
using Midnight.Pages;

namespace Holoverse.Client.Controllers
{
	using Client.Data;
	
	public class PersonalFeedPageController : FeedPageController
	{
		[Space]
		[Header("References")]
		[SerializeField]
		private Section _emptyFeedSection = null;

		protected override VideoFeedData CreateVideoFeedData() => null;

		private async void OnNodeVisit()
		{
			if(CreateVideoFeedData() == null) {
				videoFeedSection.gameObject.SetActive(false);
				_emptyFeedSection.gameObject.SetActive(true);
			} else {
				videoFeedSection.gameObject.SetActive(true);
				_emptyFeedSection.gameObject.SetActive(false);

				await InitializeAsync();
			}			
		}

		private void OnNodeLeave()
		{

		}
	}
}
