using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight.Logs;
using Midnight.Tasks;
using VirtualHole.Client.Data;

namespace VirtualHole.Client.UI
{
	public class SearchView : UILifecycle
	{
		public event Action<CreatorScrollCellData> OnCellDataCreated = delegate { };

		public Func<string, CreatorQuery> creatorQueryFactory = null;  
		public float searchInputDelaySeconds = 2f;

		public TMP_InputField searchField => _searchField;
		[SerializeField]
		private TMP_InputField _searchField = null;

		public LoopScrollRect scroll => _scroll;
		[SerializeField]
		private LoopScrollRect _scroll = null;

		public ScrollCellDataContainer scrollDataContainer => _scrollDataContainer;
		[SerializeField]
		private ScrollCellDataContainer _scrollDataContainer = null;

		private List<CreatorScrollCellData> _scrollCellData = new List<CreatorScrollCellData>();
		private CancellationTokenSource _cts = null;

		protected override async Task LoadAsync_Impl(CancellationToken cancellationToken = default)
		{
			if(string.IsNullOrEmpty(searchField.text)) { return; }

			MLog.Log(nameof(SearchView), $"Start searching: {searchField.text}");

			CreatorQuery creatorQuery = creatorQueryFactory(searchField.text);
			IEnumerable<CreatorScrollCellData> cellData = await UIFactory.CreateCreatorScrollCellDataAsync(creatorQuery, cancellationToken);

			foreach(CreatorScrollCellData cell in cellData) { OnCellDataCreated(cell); }
			scrollDataContainer.UpdateData(cellData);
		}

		private async void OnSearchFieldValueChanged(string value)
		{
			if(string.IsNullOrEmpty(searchField.text)) { return; }

			CancellationTokenSourceExt.CancelAndCreate(ref _cts);

			try {
				await Task.Delay(TimeSpan.FromSeconds(searchInputDelaySeconds), _cts.Token);
				await LoadAsync(_cts.Token);
			} catch(Exception e) {
				if(!(e is OperationCanceledException)) {
					throw;
				}
			}
		}

		private void OnEnable()
		{
			searchField.onValueChanged.AddListener(OnSearchFieldValueChanged);
		}

		private void OnDisable()
		{
			searchField.onValueChanged.RemoveListener(OnSearchFieldValueChanged);
		}
	}
}
