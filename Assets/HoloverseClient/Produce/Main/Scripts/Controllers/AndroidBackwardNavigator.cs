using UnityEngine;
using Midnight;
using Midnight.SOM;
using Midnight.Mobile;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Client.SOM;

	public class AndroidBackwardNavigator : MonoBehaviour
	{
		private SceneObjectModel _som = null;
		private FlowTree _flowTree = null;

		private void OnEscapeButton()
		{
			if(_flowTree.isLessThanOrOneNode) { MobileApplication.Suspend(); } 
			else { _flowTree.Backward(); }
		}

		private void Awake()
		{
			_som = SceneObjectModel.Get(this);
			_flowTree = _som.GetCachedComponent<MainFlowMap>().flowTree;
		}

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode.Escape)) { OnEscapeButton(); }
		}
	}
}
