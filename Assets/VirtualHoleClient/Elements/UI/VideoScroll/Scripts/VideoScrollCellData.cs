using System;

namespace VirtualHole.Client.UI
{
	using Api.DB.Contents.Videos;
	using Client.Data;

	public class VideoScrollCellData
	{
		public VideoDTO<Video> videoDTO;
		public Action onCellClick = null;
		public Action onOptionsClick = null;
	}
}
