using UnityEngine;

namespace VirtualHole.Client.Controllers
{
	public class FPSController : MonoBehaviour
	{
		[SerializeField]
		private int _targetFramerate = 60;

		void Start()
		{
			Application.targetFrameRate = _targetFramerate;
		}
	}
}
