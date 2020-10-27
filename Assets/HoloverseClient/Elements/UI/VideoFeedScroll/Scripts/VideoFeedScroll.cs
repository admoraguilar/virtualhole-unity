using System;
using System.Linq;
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
	using Client.Data;

	public class VideoFeedScroll : MonoBehaviour
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

		public LoopScrollRect scroll => _scroll;
		[Header("References")]
		[SerializeField]
		private LoopScrollRect _scroll = null;

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

			CoroutineUtilities.ExecuteOnYield(
				null, () => {
					_scroll.GetComponent<LoopScrollRectCellDataContainer>().UpdateData(_cellData);

					if(isFromTop) {
						//scroll.ScrollToCell(0, 3000f);
					}
				}, false);
		}

		public async Task UnloadAsync()
		{
			await Task.CompletedTask;
			ClearFeed();
		}

		public void ClearFeed()
		{
			_cellData.Clear();
			_scroll.GetComponent<LoopScrollRectCellDataContainer>().UpdateData(_cellData);

			if(_videoFeedData != null) {
				VideoFeedData.Feed feed = _videoFeedData.feeds[dropdown.value];
				feed.Clear();
			}
		}

		private void OnDropdownValueChanged(int value)
		{
			OnDropdownValueChangedCallback(value);
		}

		private void OnScrollValueChanged(Vector2 position)
		{
			if(position.y >= .8f) {
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
