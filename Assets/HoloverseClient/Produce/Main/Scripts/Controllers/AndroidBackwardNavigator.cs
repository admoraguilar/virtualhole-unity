using UnityEngine;
using Midnight.SOM;
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
			if(_flowTree.isOnlyOneNode) {
#if !UNITY_EDITOR && UNITY_ANDROID
				AndroidJavaObject activity =
					new AndroidJavaClass("com.unity3d.player.UnityPlayer")
					.GetStatic<AndroidJavaObject>("currentActivity");
				activity.Call<bool>("moveTaskToBack", true);
#else
				_flowTree.Backward();
#endif
			} else {
				_flowTree.Backward();
			}
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
