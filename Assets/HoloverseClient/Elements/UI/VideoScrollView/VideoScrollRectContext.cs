using System;
using FancyScrollView;

namespace Holoverse.Client.UI
{
	public class VideoScrollRectContext : FancyScrollRectContext
	{
		public int selectedIndex = -1;
		public Action<int> onCellClicked = delegate { };
	}
}
