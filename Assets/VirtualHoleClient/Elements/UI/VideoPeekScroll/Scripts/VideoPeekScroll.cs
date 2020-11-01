using System;
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
	
	public class VideoPeekScroll : MonoBehaviour
	{
		public event Action<VideoScrollCellData> OnCellDataCreated = delegate { };

		public Image background => _background;
		[SerializeField]
		private Image _background = null;

		public TMP_Text header => _header;
		[SerializeField]
		private TMP_Text _header = null;

		public LoopScrollRect scroll => _scroll;
		[SerializeField]
		private LoopScrollRect _scroll = null;

		public LoopScrollCellDataContainer scrollDataContainer => _scrollDataContainer;
		[SerializeField]
		private LoopScrollCellDataContainer _scrollDataContainer = null;

		public Button optionButton => _optionButton;
		[SerializeField]
		private Button _optionButton = null;

		private VideoFeedQuery _feed = null;
		private List<VideoScrollCellData> _cellData = new List<VideoScrollCellData>();

		public async Task InitializeAsync(VideoFeedQuery feed, CancellationToken cancellationToken = default)
		{
			_feed = feed;

			IEnumerable<VideoScrollCellData> cellData = await UIFactory.CreateVideoScrollCellDataAsync(
				_feed, cancellationToken);
			foreach(VideoScrollCellData cell in cellData) {
				OnCellDataCreated?.Invoke(cell);
			}

			_cellData.AddRange(cellData);
			scrollDataContainer.UpdateData(_cellData);
		}

		public async Task UnloadAsync()
		{
			await Task.CompletedTask;
			ClearFeed();
		}

		public void ClearFeed()
		{
			_cellData.Clear();
			scrollDataContainer.UpdateData(_cellData);

			if(_feed != null) {
				_feed.Clear();
			}
		}
	}
}
