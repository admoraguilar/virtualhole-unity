using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualHole.Client.UI
{
	using Api.DB.Contents.Videos;
	using Client.Data;

	public static class UIFactory
	{
		public static async Task<IEnumerable<CreatorScrollCellData>> CreateCreatorScrollCellDataAsync(
			CreatorQuery query, CancellationToken cancellationToken = default)
		{
			List<CreatorScrollCellData> results = new List<CreatorScrollCellData>();

			IEnumerable<CreatorDTO> creatorDTOs = await query.GetDTOAsync(cancellationToken);
			if(creatorDTOs == null) { return results; }

			foreach(CreatorDTO creatorDTO in creatorDTOs) {
				results.Add(new CreatorScrollCellData {
					creatorDTO = creatorDTO
				});
			}

			return results;
		}

		public static async Task<IEnumerable<VideoScrollCellData>> CreateVideoScrollCellDataAsync(
			VideoFeedQuery query, CancellationToken cancellationToken = default)
		{
			List<VideoScrollCellData> results = new List<VideoScrollCellData>();

			IEnumerable<VideoDTO<Video>> videoDTOs = await query.GetDTOAsync(cancellationToken);
			if(videoDTOs == null) { return results; }

			foreach(VideoDTO<Video> videoDTO in videoDTOs) {
				// Skip videos without thumbnails, possible reasons for these are
				// they maybe privated or deleted.
				// Mostly observed on scheduled videos or livestreams that are already
				// finished.
				if(videoDTO.thumbnailSprite == null) { continue; }
				results.Add(new VideoScrollCellData() {
					videoDTO = videoDTO
				});
			}

			return results;
		}

		public static async Task<IEnumerable<InfoButtonData>> CreateInfoButtonDataAsync(
			SupportListQuery query, CancellationToken cancellationToken = default)
		{
			List<InfoButtonData> results = new List<InfoButtonData>();

			IEnumerable<SupportInfoDTO> infoDTOs = await query.GetDTOAsync(cancellationToken);
			foreach(SupportInfoDTO infoDTO in infoDTOs) {
				InfoButtonData infoButtonData = new InfoButtonData() {
					header = infoDTO.raw.header,
					content = infoDTO.raw.content,
					onClick = () => Application.OpenURL(infoDTO.raw.url)
				};

				infoButtonData.sprite = infoDTO.imageSprite;
				results.Add(infoButtonData);
			}

			return results;
		}
	}
}
