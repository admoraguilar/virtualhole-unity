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
	
	public class VideoPeekScroll : UILifecycle
	{
		public event Action<VideoScrollCellData> OnCellDataCreated = delegate { };

		public VideoFeedQuery feed { get; set; } = null;

		public Image background => _background;
		[SerializeField]
		private Image _background = null;

		public TMP_Text header => _header;
		[SerializeField]
		private TMP_Text _header = null;

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

		public Button optionButton => _optionButton;
		[SerializeField]
		private Button _optionButton = null;

		protected override async Task InitializeAsync_Impl(CancellationToken cancellationToken = default)
		{
			IEnumerable<VideoScrollCellData> cellData = await UIFactory.CreateVideoScrollCellDataAsync(feed, cancellationToken);
			foreach(VideoScrollCellData cell in cellData) { OnCellDataCreated(cell); }
			scrollDataContainer.UpdateData(cellData);
		}

		protected override async Task UnloadAsync_Impl()
		{
			await Task.CompletedTask;
			ClearFeed();
		}

		public void ClearFeed()
		{
			scrollDataContainer.UpdateData(null);
			if(feed != null) { feed.Clear(); }
		}
	}
}
