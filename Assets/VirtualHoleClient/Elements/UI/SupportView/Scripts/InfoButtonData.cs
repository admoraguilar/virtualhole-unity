using System;
using UnityEngine;

namespace VirtualHole.Client.UI
{
	public class InfoButtonData
	{
		public Sprite image = null;
		public string header = string.Empty;
		public string content = string.Empty;

		public Action onClick = null;
	}
}
