using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;

namespace Holoverse.Client.UI
{
	using Api.Data;

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

		public void Initialize(HoloverseDataClient client)
		{
			_client = client;
		}

		private void OnSearchFieldValueChanged(string value)
		{
			
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
