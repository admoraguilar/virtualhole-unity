using UnityEngine;

namespace VirtualHole.Client.Controllers
{
	public class FPSController : MonoBehaviour
	{
		[SerializeField]
		private int _targetFramerate = 60;

		[SerializeField]
		private bool _runOnSuspend = true;

		[SerializeField]
		private bool _runOnFocus = false;

		private void SetTargetFramerateActive(bool active)
		{
			if(Application.isEditor) {
				Application.targetFrameRate = _targetFramerate;
			} else {
				if(!active) { Application.targetFrameRate = 1; } 
				else { Application.targetFrameRate = _targetFramerate; }
			}
		}

		private void Start()
		{
			SetTargetFramerateActive(true);
		}

		private void OnApplicationPause(bool pause)
		{
			if(!_runOnSuspend) { return; }
			SetTargetFramerateActive(!pause);
		}

		private void OnApplicationFocus(bool focus)
		{
			if(!_runOnFocus) { return; }
			SetTargetFramerateActive(focus);
		}
	}
}
