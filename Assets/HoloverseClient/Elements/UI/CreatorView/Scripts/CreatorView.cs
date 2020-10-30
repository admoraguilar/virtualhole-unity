using System;
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
	

	public class CreatorView : MonoBehaviour, ISimpleCycleAsync
	{
		public event Action<object> OnInitializeStart = delegate { };
		public event Action<Exception, object> OnInitializeError = delegate { };
		public event Action<object> OnInitializeFinish = delegate { };

		public event Action<object> OnLoadStart = delegate { };
		public event Action<Exception, object> OnLoadError = delegate { };
		public event Action<object> OnLoadFinish = delegate { };

		public event Action<object> OnUnloadStart = delegate { };
		public event Action<Exception, object> OnUnloadError = delegate { };
		public event Action<object> OnUnloadFinish = delegate { };

		public event Action<VideoPeekScroll> OnVideoPeekScrollProcess = delegate { };

		public Creator creator { get; private set; } = null;

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

		public bool isInitializing { get; private set; } = false;
		public bool isInitialized { get; private set; } = false;
		public bool isLoading { get; private set; } = false;

		private Func<CancellationToken, Task> _dataFactory = null;
		private List<Button> _socialButtonInstances = new List<Button>();
		private List<VideoFeedQuery> _feeds = new List<VideoFeedQuery>();
		private List<VideoPeekScroll> _peekScrollInstances = new List<VideoPeekScroll>();

		public void SetData(Func<CancellationToken, Task> dataFactory)
		{
			_dataFactory = dataFactory;
		}

		public void SetData(Creator creator, IEnumerable<VideoFeedQuery> feeds)
		{
			this.creator = creator;
			_feeds.AddRange(feeds);
		} 

		public async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			if(!this.CanInitialize()) { return; }
			isInitializing = true;
			OnInitializeStart(null);

			try {
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

				foreach(VideoFeedQuery feed in _feeds) {
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
			} catch(Exception e) {
				OnInitializeError(e, null);
				throw;
			}

			OnInitializeFinish(null);
			isInitialized = true;
		}

		public async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			if(!this.CanLoad()) { return; }
			await Task.CompletedTask;
		}

		public async Task UnloadAsync()
		{
			await Task.CompletedTask;
			if(!this.CanUnload()) { return; }
			OnUnloadStart(null);

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

			isLoading = false;
			isInitializing = false;
			isInitialized = false;
			OnUnloadFinish(null);
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
