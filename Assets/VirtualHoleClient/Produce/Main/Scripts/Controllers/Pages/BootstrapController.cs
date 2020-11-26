using UnityEngine;
using Midnight.Unity;
using Midnight.Unity.FlowTree;
using VirtualHole.Client.Data;
using VirtualHole.Client.ComponentMaps;

namespace VirtualHole.Client.Controllers
{
	public class BootstrapController : MonoBehaviour
	{
		[SerializeField]
		private UserDataClientObject _userDataClient = null;

		private FlowTree _flowTree => _mainFlowMap.flowTree;
		private Node _bootstrapNode => _mainFlowMap.bootstrapNode;
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private async void OnBootstrapVisit()
		{
			UserDataClient userDataClient = _userDataClient.CreateClient("", PathUtilities.CreateDataPath());
			await userDataClient.LoadAsync();

			_flowTree.Next();
		}

		private void OnEnable()
		{
			_bootstrapNode.OnVisit += OnBootstrapVisit;
		}

		private void OnDisable()
		{
			_bootstrapNode.OnVisit -= OnBootstrapVisit;
		}
	}
}
