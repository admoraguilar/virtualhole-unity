using System;
using UnityEngine;

namespace VirtualHole.Client.UI
{
	public class VideoScrollCellData
	{
		public Sprite thumbnailSprite = null;
		public Sprite indicatorSprite = null;
		public string title = string.Empty;
		public string date = string.Empty;

		public Sprite creatorSprite = null;
		public string creatorName = string.Empty;
		public string creatorUniversalId = string.Empty;

		public Action onCellClick = null;
		public Action onOptionsClick = null;
	}
}
