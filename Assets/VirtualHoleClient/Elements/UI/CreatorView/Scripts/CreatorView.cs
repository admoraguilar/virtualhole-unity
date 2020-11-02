using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;

namespace VirtualHole.Client.UI
{
	using Api.DB.Contents;
	using Api.DB.Contents.Creators;
	
	using Client.Data;

	public class CreatorView : UILifecycle
	{
		public event Action<VideoPeekScroll> OnVideoPeekScrollProcess = delegate { };

		public Creator creator { get; set; } = null;
		public List<VideoFeedQuery> feeds { get; private set; } = new List<VideoFeedQuery>();

		public ScrollRect plainScroll => _plainScroll;
		[SerializeField]
		private ScrollRect _plainScroll = null;

		public Image avatarImage => _avatarImage;
		[SerializeField]
		private Image _avatarImage = null;

		public TMP_Text nameText => _nameText;
		[SerializeField]
		private TMP_Text _nameText = null;

		public RectTransform socialButtonContainer => _socialButtonContainer;
		[Space]
		[SerializeField]
		private RectTransform _socialButtonContainer = null;

		public Button socialButtonTemplate => _socialButtonTemplate;
		[SerializeField]
		private Button _socialButtonTemplate = null;

		public Button followButton => _followButton;
		[SerializeField]
		private Button _followButton = null;

		public RectTransform peekScrollContainer => _peekScrollContainer;
		[Space]
		[SerializeField]
		private RectTransform _peekScrollContainer = null;

		public VideoPeekScroll peekScrollTemplate => _peekScrollTemplate;
		[SerializeField]
		private VideoPeekScroll _peekScrollTemplate = null;

		private List<Button> _socialButtonInstances = new List<Button>();
		private List<VideoPeekScroll> _peekScrollInstances = new List<VideoPeekScroll>();

		protected override async Task InitializeAsync_Impl(CancellationToken cancellationToken = default)
		{
			avatarImage.sprite = await CreatorCache.GetAvatarAsync(creator, cancellationToken);
			nameText.text = creator.universalName;

			foreach(Social social in creator.socials) {
				Button socialButton = Instantiate(socialButtonTemplate, socialButtonContainer, false);
				socialButton.gameObject.SetActive(true);

				_socialButtonInstances.Add(socialButton);

				socialButton.name = $"{social.platform}-{social.name}";
				socialButton.image.sprite = UIResources.GetPlatformUI(social.platform).logo;
				socialButton.image.enabled = true;

				socialButton.onClick.AddListener(() => Application.OpenURL(social.url));
			}

			//List<Creator> followedCreators = UserCache.loadedProfile.followedCreators;
			List<Creator> followedCreators = new List<Creator>();
			if(followedCreators.Exists(c => c.universalId == creator.universalId)) {
				ColorHSV color = followButton.image.color;
				color.v = 1f;
				followButton.image.color = color;
				followButton.GetComponentInChildren<TMP_Text>(true).text = $"Following";

				followButton.onClick.AddListener(() => {
					int index = followedCreators.FindIndex(c => c.universalId == creator.universalId);
					followedCreators.RemoveAt(index);
				});
			} else {
				ColorHSV color = followButton.image.color;
				color.v = .3f;
				followButton.image.color = color;
				followButton.GetComponentInChildren<TMP_Text>(true).text = $"Follow";

				followButton.onClick.AddListener(() => {
					followedCreators.Add(creator);
				});
			}

			foreach(VideoFeedQuery feed in feeds) {
				VideoPeekScroll peekScroll = Instantiate(peekScrollTemplate, peekScrollContainer, false);
				peekScroll.gameObject.SetActive(true);

				_peekScrollInstances.Add(peekScroll);

				peekScroll.name = feed.name;
				peekScroll.header.text = feed.name;

				peekScroll.optionButton.onClick.RemoveAllListeners();
				peekScroll.optionButton.GetComponentInChildren<TMP_Text>(true).text = $"More {feed.name}";

				OnVideoPeekScrollProcess(peekScroll);

				peekScroll.feed = feed;
				await peekScroll.InitializeAsync(cancellationToken);
			}
		}

		protected override async Task UnloadAsync_Impl()
		{
			await Task.CompletedTask;

			if(plainScroll.horizontal) { plainScroll.horizontalNormalizedPosition = 1f; }
			if(plainScroll.vertical) { plainScroll.verticalNormalizedPosition = 1f; }

			avatarImage.sprite = null;
			nameText.text = string.Empty;

			followButton.onClick.RemoveAllListeners();

			foreach(Button socialButton in _socialButtonInstances) {
				Destroy(socialButton.gameObject);
			}
			_socialButtonInstances.Clear();

			foreach(VideoPeekScroll peekScroll in _peekScrollInstances) {
				Destroy(peekScroll.gameObject);
			}
			_peekScrollInstances.Clear();
		}

		public void ScrollToTop(float speed = 10f)
		{
			CoroutineUtilities.Start(Routine());

			IEnumerator Routine()
			{
				int totalCount = plainScroll.content.transform.childCount;
				if(totalCount > 0) {
					if(totalCount < 10) { plainScroll.verticalNormalizedPosition = .2f; } 
					else { plainScroll.verticalNormalizedPosition = 10f / totalCount; }
				}

				while(plainScroll.verticalNormalizedPosition > 0f) {
					plainScroll.verticalNormalizedPosition -= speed * Time.fixedDeltaTime;
					yield return null;
				}
			}
		}
	}
}
