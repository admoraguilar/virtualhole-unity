using UnityEngine;
using Midnight.Unity.Mobile;
using Midnight.Unity.FlowTree;
using VirtualHole.Client.ComponentMaps;

namespace VirtualHole.Client.Controllers
{
	public class AndroidBackwardNavigator : MonoBehaviour
	{
		private FlowTree _flowTree => _mainFlowMap.flowTree;
		[Space]
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private void OnEscapeButton()
		{
			if(_flowTree.isLessThanOrOneNode) { MobileApplication.Suspend(); } 
			else { _flowTree.Backward(); }
		}

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode.Escape)) { OnEscapeButton(); }
		}
	}
}
