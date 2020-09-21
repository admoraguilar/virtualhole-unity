using System;
using UnityEngine;

namespace Holoverse.Client.UI
{
	public class VideoScrollViewCellData
	{
		public Sprite thumbnail = null;
		public string title = string.Empty;
		public string channel = string.Empty;

		public Action onClick = delegate { };
	}
}
