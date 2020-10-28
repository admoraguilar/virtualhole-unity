using UnityEngine;
using Midnight.Mobile;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Client.ComponentMaps;

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
