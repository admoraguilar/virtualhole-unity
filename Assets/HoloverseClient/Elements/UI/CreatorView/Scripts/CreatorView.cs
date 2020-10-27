using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Holoverse.Client.UI
{
	using Api.Data.Contents;
	using Api.Data.Contents.Creators;
	
	using Client.Data;

	public class CreatorView : MonoBehaviour
	{
		public Creator creator { get; private set; } = null;

		[Header("References")]
		[SerializeField]
		private Image _avatarImage = null;

		[SerializeField]
		private TMP_Text _nameText = null;

		[Space]
		[SerializeField]
		private RectTransform _socialButtonContainer = null;

		[SerializeField]
		private Button _socialButtonTemplate = null;

		[SerializeField]
		private Button _followButton = null;

		[Space]
		[SerializeField]
		private RectTransform _peekScrollContainer = null;

		[SerializeField]
		private VideoPeekScroll _peekScrollTemplate = null;

		public async Task LoadCreatorAsync(
			Creator creator, IEnumerable<VideoFeedQuery> feeds, 
			CancellationToken cancellationToken = default)
		{
			this.creator = creator;

			_avatarImage.sprite = await CreatorCache.GetAvatarAsync(
				creator.universalId, creator.avatarUrl, 
				cancellationToken);
			_nameText.text = creator.universalName;

			while(_socialButtonContainer.childCount > 0) {
				Destroy(_socialButtonContainer.GetChild(0).gameObject);
			}

			foreach(Social social in creator.socials) {
				Button socialButton = Instantiate(_socialButtonTemplate, _socialButtonContainer, false);
				socialButton.name = $"{social.platform}-{social.name}";
				socialButton.image.sprite = UIResources.GetPlatformUI(social.platform).logo;
				socialButton.onClick.AddListener(() => Application.OpenURL(social.url));
			}

			foreach(VideoFeedQuery feed in feeds) {
				VideoPeekScroll peekScroll = Instantiate(_peekScrollTemplate, _peekScrollContainer, false);
				peekScroll.name = _peekScrollTemplate.name;
				peekScroll.gameObject.SetActive(true);

				await peekScroll.InitializeAsync(feed, cancellationToken);
			}
		}

		public async Task UnloadAsync()
		{
			await Task.CompletedTask;
		}
	}
}
