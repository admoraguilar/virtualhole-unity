using UnityEngine;

namespace VirtualHole.Client.Data
{
	public class ImageData
	{
		public readonly string url = string.Empty;
		public readonly Sprite sprite = null;

		public ImageData(string url, Sprite sprite)
		{
			this.url = url;
			this.sprite = sprite;
		}
	}
}