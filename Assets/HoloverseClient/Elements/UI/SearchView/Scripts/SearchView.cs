using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;
using Midnight.Concurrency;

namespace Holoverse.Client.UI
{
	using Api.Data;
	using Api.Data.Contents.Creators;

	using Client.Data;

	public class SearchView : MonoBehaviour
	{
		public TMP_InputField searchField => _searchField;
		[SerializeField]
		private TMP_InputField _searchField = null;

		public LoopScrollRect scroll => _scroll;
		[SerializeField]
		private LoopScrollRect _scroll = null;

		public LoopScrollCellDataContainer scrollDataContainer => _scrollDataContainer;
		[SerializeField]
		private LoopScrollCellDataContainer _scrollDataContainer = null;

		private HoloverseDataClient _client = null;
		private CreatorQuery _creatorQuery = null;

		private List<CreatorScrollCellData> _scrollCellData = new List<CreatorScrollCellData>();
		private CancellationTokenSource _cts = null;

		public void Initialize(HoloverseDataClient client)
		{
			_client = client;
		}

		public async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			if(string.IsNullOrEmpty(searchField.text)) { return; }

			_creatorQuery = new CreatorQuery(
				_client,
				new FindCreatorsRegexSettings {
					searchQueries = new List<string>() { _searchField.text },
					isCheckForAffiliations = true,
					affiliations = new List<string>() { "hololiveProduction" }
				});

			cancellationToken.ThrowIfCancellationRequested();
		}

		private void OnSearchFieldValueChanged(string value)
		{
			if(string.IsNullOrEmpty(searchField.text)) { return; }

			CoroutineUtilities.Start(SearchRoutine());

			IEnumerator SearchRoutine()
			{
				yield return new WaitForSeconds(2f);
				CancellationTokenSourceFactory.CancelAndCreateCancellationTokenSource(ref _cts);
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
