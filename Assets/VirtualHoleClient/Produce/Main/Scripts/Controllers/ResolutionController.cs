using UnityEngine;

namespace VirtualHole.Client.Controllers
{
	public class ResolutionController : MonoBehaviour
	{
		[SerializeField]
		private int width = 720;

		[SerializeField]
		private int height = 1280;

		[SerializeField]
		private int refreshRate = 60;

		private void Start()
		{
			Screen.SetResolution(width, height, true, refreshRate);
		}
	}
}
