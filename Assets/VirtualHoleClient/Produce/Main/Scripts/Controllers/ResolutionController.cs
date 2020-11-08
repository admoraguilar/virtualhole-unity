using UnityEngine;

namespace VirtualHole.Client.Controllers
{
	public class ResolutionController : MonoBehaviour
	{
		[Range(0.3f, 1f)]
		[SerializeField]
		private float _resolutionScaling = 0f;

		[SerializeField]
		private int _preferredRefreshRate = 60;

		private Vector2Int _renderResolution = Vector2Int.zero;
		private Vector2Int _deviceResolution = Vector2Int.zero;

		private void UpdateRenderResolution()
		{
			float scale = Mathf.Max(0.3f, _resolutionScaling);
			Vector2 desiredResolution = new Vector2(_deviceResolution.x * scale, _deviceResolution.y * scale);
			_renderResolution = new Vector2Int((int)desiredResolution.x, (int)desiredResolution.y);
			Screen.SetResolution(_renderResolution.x, _renderResolution.y, true, _preferredRefreshRate);
		}

		private void UpdateDeviceResolution()
		{
			_deviceResolution = new Vector2Int(Screen.width, Screen.height);
		}

		private void Start()
		{
			UpdateDeviceResolution();
			UpdateRenderResolution();
		}
	}
}
