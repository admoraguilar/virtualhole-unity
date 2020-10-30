using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;

namespace Holoverse.Client.UI
{
	using Api.Data.Contents;
	using Api.Data.Contents.Creators;
	
	using Client.Data;
	using System;

	public class CreatorView : MonoBehaviour
	{
		public event Action<VideoPeekScroll> OnVideoPeekScrollProcess = delegate { };

		public Creator creator { get; private set; } = null;

		[Header("References")]
		[SerializeField]
		private ScrollRect _plainScroll = null;

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

		private List<Button> _socialButtonInstances = new List<Button>();
		private List<VideoPeekScroll> _peekScrollInstances = new List<VideoPeekScroll>(); 

		public async Task LoadCreatorAsync(
			Creator creator, IEnumerable<VideoFeedQuery> feeds, 
			CancellationToken cancellationToken = default)
		{
			this.creator = creator;

			_avatarImage.sprite = await CreatorCache.GetAvatarAsync(
				creator.universalId, creator.avatarUrl, 
				cancellationToken);
			_nameText.text = creator.universalName;

			foreach(Social social in creator.socials) {
				Button socialButton = Instantiate(_socialButtonTemplate, _socialButtonContainer, false);
				socialButton.gameObject.SetActive(true);

				_socialButtonInstances.Add(socialButton);

				socialButton.name = $"{social.platform}-{social.name}";
				socialButton.image.sprite = UIResources.GetPlatformUI(social.platform).logo;
				socialButton.image.enabled = true;

				socialButton.onClick.AddListener(() => Application.OpenURL(social.url));
			}

			foreach(VideoFeedQuery feed in feeds) {
				VideoPeekScroll peekScroll = Instantiate(_peekScrollTemplate, _peekScrollContainer, false);
				peekScroll.gameObject.SetActive(true);

				_peekScrollInstances.Add(peekScroll);

				peekScroll.name = feed.name;
				peekScroll.header.text = feed.name;

				peekScroll.optionButton.onClick.RemoveAllListeners();
				peekScroll.optionButton.GetComponentInChildren<TMP_Text>(true).text = $"More {feed.name}";

				OnVideoPeekScrollProcess(peekScroll);

				await peekScroll.InitializeAsync(feed, cancellationToken);
			}
		}

		public async Task UnloadAsync()
		{
			await Task.CompletedTask;

			if(_plainScroll.horizontal) { _plainScroll.horizontalNormalizedPosition = 1f; }
			if(_plainScroll.vertical) { _plainScroll.verticalNormalizedPosition = 1f; }

			_avatarImage.sprite = null;
			_nameText.text = string.Empty;

			_followButton.onClick.RemoveAllListeners();

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
				int totalCount = _plainScroll.content.transform.childCount;
				if(totalCount > 0) {
					if(totalCount < 10) { _plainScroll.verticalNormalizedPosition = .2f; } 
					else { _plainScroll.verticalNormalizedPosition = 10f / totalCount; }
				}

				while(_plainScroll.verticalNormalizedPosition > 0f) {
					_plainScroll.verticalNormalizedPosition -= speed * Time.fixedDeltaTime;
					yield return null;
				}
			}
		}
	}
}
