using System;
using UnityEngine;

namespace VirtualHole.Client.UI
{
	public class InfoButtonData
	{
		public Action onClick = null;
		public Sprite sprite = null;
		public string header = string.Empty;
		public string content = string.Empty;
	}
}
