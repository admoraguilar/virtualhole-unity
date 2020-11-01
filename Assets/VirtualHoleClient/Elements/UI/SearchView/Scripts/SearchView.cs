using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;
using Midnight.Concurrency;

namespace VirtualHole.Client.UI
{
	using Api.DB;
	using Api.DB.Contents.Creators;

	using Client.Data;

	public class SearchView : MonoBehaviour
	{
		public event Action<CreatorScrollCellData> OnCellDataCreated = delegate { };

		public float searchInputDelaySeconds = 2f;

		public TMP_InputField searchField => _searchField;
		[SerializeField]
		private TMP_InputField _searchField = null;

		public LoopScrollRect scroll => _scroll;
		[SerializeField]
		private LoopScrollRect _scroll = null;

		public LoopScrollCellDataContainer scrollDataContainer => _scrollDataContainer;
		[SerializeField]
		private LoopScrollCellDataContainer _scrollDataContainer = null;

		private VirtualHoleDBClient _client = null;
		private CreatorQuery _creatorQuery = null;

		private List<CreatorScrollCellData> _scrollCellData = new List<CreatorScrollCellData>();
		private CancellationTokenSource _cts = null;

		public void Initialize(VirtualHoleDBClient client)
		{
			_client = client;
		}

		public async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			if(string.IsNullOrEmpty(searchField.text)) { return; }

			MLog.Log($"Start searching: {searchField.text}");

			_creatorQuery = new CreatorQuery(
				_client,
				new FindCreatorsRegexSettings {
					searchQueries = new List<string>() { _searchField.text },
					isCheckSocialName = false,
					isCheckForAffiliations = true,
					affiliations = new List<string>() { "hololiveProduction" }
				});

			_scrollCellData.Clear();
			_scrollCellData.AddRange(await UIFactory.CreateCreatorScrollCellDataAsync(_creatorQuery, cancellationToken));

			foreach(CreatorScrollCellData cellData in _scrollCellData) {
				OnCellDataCreated(cellData);
			}

			CoroutineUtilities.ExecuteOnYield(
				null, () => {
					scrollDataContainer.UpdateData(_scrollCellData);
				}, true);

			cancellationToken.ThrowIfCancellationRequested();
		}

		private async void OnSearchFieldValueChanged(string value)
		{
			if(string.IsNullOrEmpty(searchField.text)) { 
				return;
			}

			CancellationTokenSourceFactory.CancelAndCreateCancellationTokenSource(ref _cts);

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
