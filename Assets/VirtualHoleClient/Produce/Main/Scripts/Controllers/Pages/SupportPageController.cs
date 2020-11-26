using UnityEngine;
using Midnight.Unity.FlowTree;
using VirtualHole.Client.UI;
using VirtualHole.Client.Data;
using VirtualHole.Client.ComponentMaps;

namespace VirtualHole.Client.Controllers
{
	public class SupportPageController : MonoBehaviour
	{
		private Node _supportNode => _mainFlowMap.supportNode;
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private SupportView _supportView => _supportFlowMap.supportView;
		[SerializeField]
		private SupportFlowMap _supportFlowMap = null;

		private async void OnSupportVisit()
		{
			_supportView.query = new SupportListQuery();
			await _supportView.InitializeAsync();
		}

		private void OnEnable()
		{
			_supportNode.OnVisit += OnSupportVisit;
		}

		private void OnDisable()
		{
			_supportNode.OnVisit -= OnSupportVisit;
		}
	}
}
