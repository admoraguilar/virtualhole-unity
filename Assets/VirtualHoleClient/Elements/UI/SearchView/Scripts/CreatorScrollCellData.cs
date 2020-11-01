using System;
using UnityEngine;

namespace VirtualHole.Client.UI
{
	public class CreatorScrollCellData
	{
		public Sprite creatorAvatar = null;
		public string creatorName = string.Empty;
		public string creatorId = string.Empty;

		public Action onClick = null;
	}
}
