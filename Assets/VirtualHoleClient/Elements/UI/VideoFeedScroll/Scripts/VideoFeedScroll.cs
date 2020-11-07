using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;

namespace VirtualHole.Client.UI
{
	using Client.Data;

	public class VideoFeedScroll : UILifecycle
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

		public event Action<VideoScrollCellData> OnCellDataCreated = delegate { };
		public event Action<int> OnDropdownValueChangedCallback = delegate { };
		public event Action OnNearScrollEnd = delegate { };

		public List<VideoFeedQuery> feeds { get; private set; } = new List<VideoFeedQuery>();
		private List<VideoScrollCellData> _cellData = new List<VideoScrollCellData>();

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

		protected override async Task InitializeAsync_Impl(CancellationToken cancellationToken = default)
		{
			await Task.CompletedTask;

			dropdown.ClearOptions();
			dropdown.AddOptions(feeds.Select(f => f.name).ToList());
			dropdown.value = 0;

			ClearFeed();
			cancellationToken.ThrowIfCancellationRequested();	
		}

		protected override async Task PostInitializeAsync_Impl(CancellationToken cancellationToken = default)
		{
			await LoadAsync(cancellationToken);
		}

		protected override async Task LoadAsync_Impl(CancellationToken cancellationToken = default)
		{
			if(feeds.Count <= 0) { return; }

			VideoFeedQuery feed = feeds[dropdown.value];
			if(feed.isDone) { return; }

			IEnumerable<VideoScrollCellData> cellData = await UIFactory.CreateVideoScrollCellDataAsync(feed, cancellationToken);
			foreach(VideoScrollCellData cell in cellData) { OnCellDataCreated(cell); }

			_cellData.AddRange(cellData);
			scrollDataContainer.UpdateData(_cellData);
		}

		protected override async Task UnloadAsync_Impl()
		{
			await Task.CompletedTask;
			ClearFeed();
			feeds.Clear();
		}

		public void ClearFeed()
		{
			_cellData.Clear();
			scrollDataContainer.UpdateData(_cellData);
			if(feeds != null && feeds.Count > dropdown.value) {
				VideoFeedQuery feed = feeds[dropdown.value];
				feed.Reset();
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

			_loadingParameters = new CycleLoadParameters() { isShowLoadingIndicator = true };
			await LoadAsync();
			_loadingParameters = null;

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
