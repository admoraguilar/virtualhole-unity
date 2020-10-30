using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;

namespace Holoverse.Client.UI
{
	using Client.Data;

	public class VideoFeedScroll : MonoBehaviour, ISimpleCycleAsync
	{
		[Serializable]
		public class ContextButton
		{
			public Button button { get => _button; }
			[SerializeField]
			private Button _button = null;

			public Image image { get => _image; }
			[SerializeField]
			private Image _image = null;

			public TMP_Text text { get => _text; }
			[SerializeField]
			private TMP_Text _text = null;
		}

		public event Action<object> OnInitializeStart = delegate { };
		public event Action<Exception, object> OnInitializeError = delegate { };
		public event Action<object> OnInitializeFinish = delegate { };

		public event Action<object> OnLoadStart = delegate { };
		public event Action<Exception, object> OnLoadError = delegate { };
		public event Action<object> OnLoadFinish = delegate { };

		public event Action<object> OnUnloadStart = delegate { };
		public event Action<Exception, object> OnUnloadError = delegate { };
		public event Action<object> OnUnloadFinish = delegate { };

		public event Action<VideoScrollCellData> OnCellDataCreated = delegate { };
		public event Action<int> OnDropdownValueChangedCallback = delegate { };
		public event Action OnNearScrollEnd = delegate { };

		public LoopScrollRect scroll => _scroll;
		[SerializeField]
		private LoopScrollRect _scroll = null;

		private LoopScrollCellDataContainer scrollDataContainer
		{
			get {
				return this.GetComponent(
					ref _scrollDataContainer,
					() => scroll == null ? null : scroll.GetComponent<LoopScrollCellDataContainer>());
			}
		}
		private LoopScrollCellDataContainer _scrollDataContainer = null;

		public ContextButton contextButton => _contextButton;
		[SerializeField]
		private ContextButton _contextButton = null;

		public TMP_Dropdown dropdown => _dropdown;
		[SerializeField]
		private TMP_Dropdown _dropdown = null;

		public bool isInitializing { get; private set; } = false;
		public bool isInitialized { get; private set; } = false;
		public bool isLoading { get; private set; } = false;

		private Func<CancellationToken, Task> _dataFactory = null;
		private List<VideoFeedQuery> _feeds = new List<VideoFeedQuery>();
		private List<VideoScrollCellData> _cellData = new List<VideoScrollCellData>();
		private CycleLoadParameters _loadParameters = null;

		public void SetData(Func<CancellationToken, Task> dataFactory)
		{
			_dataFactory = dataFactory;
		}

		public void SetData(IEnumerable<VideoFeedQuery> feeds)
		{
			_feeds.AddRange(feeds);
		}

		public async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			if(!this.CanInitialize()) { return; }
			isInitializing = true;
			OnInitializeStart(null);

			try {
				if(_dataFactory != null) {
					await _dataFactory(cancellationToken);
				}

				dropdown.ClearOptions();
				dropdown.AddOptions(_feeds.Select(f => f.name).ToList());
				dropdown.value = 0;

				ClearFeed();
				cancellationToken.ThrowIfCancellationRequested();
				await LoadAsync(cancellationToken);
			} catch(Exception e) {
				if(!(e is OperationCanceledException)) {
					OnInitializeError(e, null);
				}
			}

			OnInitializeFinish(null);
			isInitialized = true;
		}

		public async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			if(isLoading) { return; }
			isLoading = true;
			OnLoadStart(_loadParameters);

			try {
				if(_feeds.Count <= 0) { return; }

				VideoFeedQuery feed = _feeds[dropdown.value];
				if(feed.isDone) { return; }

				IEnumerable<VideoScrollCellData> cellData = await UIFactory.CreateVideoScrollCellDataAsync(
					feed, cancellationToken);
				foreach(VideoScrollCellData cell in cellData) {
					OnCellDataCreated?.Invoke(cell);
				}

				_cellData.AddRange(cellData);
				scrollDataContainer.UpdateData(_cellData);
			} catch(Exception e) when (!(e is OperationCanceledException)) {
				OnLoadError(e, null);
			} finally {
				isLoading = false;
			}

			OnLoadFinish(null);
		}

		public async Task UnloadAsync()
		{
			await Task.CompletedTask;
			if(isLoading) { return; }
			OnUnloadStart(null);

			ClearFeed();
			_feeds.Clear();

			isLoading = false;
			isInitializing = false;
			isInitialized = false;
			OnUnloadFinish(null);
		}

		public void ClearFeed()
		{
			_cellData.Clear();
			scrollDataContainer.UpdateData(_cellData);

			if(_feeds != null) {
				VideoFeedQuery feed = _feeds[dropdown.value];
				feed.Clear();
			}
		}

		public void ScrollToTop()
		{
			if(scroll.totalCount > 30) {
				scroll.verticalNormalizedPosition = 10f / _scroll.totalCount; 
			}

			scroll.RefreshCells();
			scroll.ScrollToCell(0, 30000f);
		}

		private async void OnDropdownValueChanged(int value)
		{
			ClearFeed();

			_loadParameters = new CycleLoadParameters() { isShowLoadingIndicator = true };
			await LoadAsync();
			_loadParameters = null;

			OnDropdownValueChangedCallback(value);
		}

		private async void OnScrollValueChanged(Vector2 position)
		{
			if(position.y >= .8f) {
				await LoadAsync();
				OnNearScrollEnd();
			}
		}

		private void OnEnable()
		{
			dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
			scroll.onValueChanged.AddListener(OnScrollValueChanged);
		}

		private void OnDisable()
		{
			dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
			scroll.onValueChanged.RemoveListener(OnScrollValueChanged);
		}
	}
}
