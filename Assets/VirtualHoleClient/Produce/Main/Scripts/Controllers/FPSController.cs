using UnityEngine;

namespace VirtualHole.Client.Controllers
{
	public class FPSController : MonoBehaviour
	{
		[SerializeField]
		private int _targetFramerate = 60;

		private void SetTargetFramerateActive(bool active)
		{
			if(!active) { Application.targetFrameRate = 1; } 
			else { Application.targetFrameRate = _targetFramerate; }
		}

		private void Start()
		{
			SetTargetFramerateActive(true);
		}

		private void OnApplicationPause(bool pause)
		{
			SetTargetFramerateActive(!pause);
		}

		private void OnApplicationFocus(bool focus)
		{
			SetTargetFramerateActive(focus);
		}
	}
}
