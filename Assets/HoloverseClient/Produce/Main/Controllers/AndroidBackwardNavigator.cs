using UnityEngine;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	public class AndroidBackwardNavigator : MonoBehaviour
	{
		[SerializeField]
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

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode.Escape)) { OnEscapeButton(); }
		}
	}
}
