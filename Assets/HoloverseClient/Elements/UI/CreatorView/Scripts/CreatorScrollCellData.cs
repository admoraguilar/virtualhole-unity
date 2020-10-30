using System;
using UnityEngine;

namespace Holoverse.Client.UI
{
	public class CreatorScrollCellData
	{
		public Sprite creatorAvatar = null;
		public string creatorName = string.Empty;
		public string creatorId = string.Empty;

		public Action onCellClick = delegate { };
	}
}
