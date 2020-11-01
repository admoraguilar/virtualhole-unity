using UnityEngine;

namespace VirtualHole.Client.Data
{
	[CreateAssetMenu(menuName = "VirtualHole/Data/CDN Client Object")]
	public class CDNClientObject : ScriptableObject
	{
		public string cdnUrl => _cdnUrl;
		[SerializeField]
		private string _cdnUrl = string.Empty;
	}
}
