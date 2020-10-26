using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Holoverse.Client.UI
{
	using Client.Data;

	public class VideoFeed : MonoBehaviour
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

		public Action<int> OnDropdownValueChangedCallback = delegate { };
		public Action OnNearScrollEnd = delegate { };

		[SerializeField]
		private int _cellRemainingThreshold = 7;

		public VideoScrollRect scroll => _scroll;
		[Header("References")]
		[SerializeField]
		private VideoScrollRect _scroll = null;

		public ContextButton contextButton => _contextButton;
		[SerializeField]
		private ContextButton _contextButton = null;

		public TMP_Dropdown dropdown => _dropDown;
		[SerializeField]
		private TMP_Dropdown _dropDown = null;

		private List<VideoScrollRectCellData> _cellData = new List<VideoScrollRectCellData>();
		private VideoFeedData _videoFeedData = null;

		public async Task InitializeAsync(
			VideoFeedData videoFeedData, Action<VideoScrollRectCellData> onCellDataCreated = null,
			CancellationToken cancellationToken = default)
		{
			_videoFeedData = videoFeedData;
			if(_videoFeedData == null) { return; }

			await _videoFeedData.InitializeAsync(cancellationToken);

			dropdown.ClearOptions();
			dropdown.AddOptions(_videoFeedData.feeds.Select(f => f.name).ToList());
			dropdown.value = 0;

			ClearFeed();
			cancellationToken.ThrowIfCancellationRequested();
			await LoadAsync(onCellDataCreated, cancellationToken);
		}

		public async Task LoadAsync(Action<VideoScrollRectCellData> onCellDataCreated = null, CancellationToken cancellationToken = default)
		{
			if(_videoFeedData == null) { return; }

			VideoFeedData.Feed feed = _videoFeedData.feeds[dropdown.value];
			if(feed.isDone) { return; }

			IEnumerable<VideoScrollRectCellData> cellData = await UIFactory.CreateVideoScrollRectCellData(
				_videoFeedData, feed, cancellationToken);
			foreach(VideoScrollRectCellData cell in cellData) {
				onCellDataCreated?.Invoke(cell);
			}

			bool isFromTop = _cellData.Count <= 0;

			_cellData.AddRange(cellData);
			scroll.UpdateData(_cellData);

			if(isFromTop) {
				scroll.ScrollTo(0f, 0f);
			}
		}

		public async Task UnloadAsync()
		{
			await Task.CompletedTask;
			ClearFeed();
		}

		public void ClearFeed()
		{
			_cellData.Clear();
			scroll.UpdateData(_cellData);

			if(_videoFeedData != null) {
				VideoFeedData.Feed feed = _videoFeedData.feeds[dropdown.value];
				feed.Clear();
			}
		}

		private void OnDropdownValueChanged(int value)
		{
			OnDropdownValueChangedCallback(value);
		}

		private void OnScrollerPositionChanged(float position)
		{
			if(position >= scroll.itemCount - _cellRemainingThreshold) {
				OnNearScrollEnd();
			}
		}

		private void OnEnable()
		{
			dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
			scroll.OnScrollerPositionChanged += OnScrollerPositionChanged;
		}

		private void OnDisable()
		{
			dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
			scroll.OnScrollerPositionChanged -= OnScrollerPositionChanged;
		}
	}
}
