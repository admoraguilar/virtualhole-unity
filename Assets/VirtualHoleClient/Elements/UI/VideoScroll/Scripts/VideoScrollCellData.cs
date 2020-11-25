using System;
using VirtualHole.Client.Data;
using VirtualHole.APIWrapper.Contents.Videos;

namespace VirtualHole.Client.UI
{
	using APIWrapper.Contents.Videos;
	using Client.Data;

	public class VideoScrollCellData
	{
		public VideoDTO<Video> videoDTO;
		public Action onCellClick = null;
		public Action onOptionsClick = null;
	}
}
