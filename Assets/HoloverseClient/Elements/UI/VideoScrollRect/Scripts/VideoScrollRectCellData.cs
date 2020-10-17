using System;
using UnityEngine;

namespace Holoverse.Client.UI
{
	public class VideoScrollRectCellData
	{
		public Sprite thumbnail = null;
		public string title = string.Empty;
		public string channel = string.Empty;

		public Action onClick = delegate { };
	}
}
