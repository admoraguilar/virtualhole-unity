using UnityEngine;
using UnityEngine.UI;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	public class MainFlowNavigator : MonoBehaviour
	{
		[Header("Node Objects")]
		[SerializeField]
		private NodeObject _homeNodeObject = null;

		[SerializeField]
		private NodeObject _feedNodeObject = null;

		[SerializeField]
		private NodeObject _searchNodeObject = null;

		[SerializeField]
		private NodeObject _supportNodeObject = null;

		[Header("UI")]
		[SerializeField]
		private Button _backwardButton = null;

		[SerializeField]
		private Button _homeButton = null;

		[SerializeField]
		private Button _feedButton = null;

		[SerializeField]
		private Button _searchButton = null;

		[SerializeField]
		private Button _supportButton = null;

		private void OnBackwardButtonClick()
		{
			_homeNodeObject.Backward();
		}

		private void OnHomeButtonClick()
		{
			_homeNodeObject.Set();
		}

		private void OnFeedButtonClick()
		{
			_feedNodeObject.Set();
		}

		private void OnSearchButtonClick()
		{
			_searchNodeObject.Set();
		}

		private void OnSupportButtonClick()
		{
			_supportNodeObject.Set();
		}

		private void OnEnable()
		{
			_backwardButton.onClick.AddListener(OnBackwardButtonClick);
			_homeButton.onClick.AddListener(OnHomeButtonClick);
			_feedButton.onClick.AddListener(OnFeedButtonClick);
			_searchButton.onClick.AddListener(OnSearchButtonClick);
			_supportButton.onClick.AddListener(OnSupportButtonClick);
		}

		private void OnDisable()
		{
			_backwardButton.onClick.RemoveListener(OnBackwardButtonClick);
			_homeButton.onClick.RemoveListener(OnHomeButtonClick);
			_feedButton.onClick.RemoveListener(OnFeedButtonClick);
			_searchButton.onClick.RemoveListener(OnSearchButtonClick);
			_supportButton.onClick.RemoveListener(OnSupportButtonClick);
		}
	}
}
