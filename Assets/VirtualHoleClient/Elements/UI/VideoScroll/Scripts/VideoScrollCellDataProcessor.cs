using UnityEngine;
using UnityEngine.UI;

namespace VirtualHole.Client.UI
{
	[CreateAssetMenu(menuName = "VirtualHole/UI/Video Scroll Cell Data Processor")]
	public class VideoScrollCellDataProcessor : ScrollCellDataProcessor<VideoButton, VideoScrollCellData>
	{
		public override void ProcessData(VideoButton instance, VideoScrollCellData data)
		{
			instance.thumbnailImage.sprite = data.videoDTO.thumbnailSprite;

			if(data.videoDTO.indicatorSprite != null) {
				instance.indicatorImage.gameObject.SetActive(true);
				instance.indicatorImage.sprite = data.videoDTO.indicatorSprite;
				instance.dateText.text = data.videoDTO.scheduleDateDisplay;
			} else {
				instance.indicatorImage.gameObject.SetActive(false);
				instance.dateText.text = data.videoDTO.creationDateDisplay;
			}

			instance.creatorImage.sprite = data.videoDTO.creatorDTO.avatarSprite;
			instance.creatorNameText.text = data.videoDTO.raw.creator;

			instance.titleText.text = data.videoDTO.raw.title;

			instance.button.onClick.RemoveAllListeners();
			instance.button.onClick.AddListener(() => data.onCellClick());

			if(data.onOptionsClick != null) {
				instance.optionsButton.gameObject.SetActive(true);
				instance.optionsButton.onClick.RemoveAllListeners();
				instance.optionsButton.onClick.AddListener(() => data.onOptionsClick());
			} else {
				instance.optionsButton.gameObject.SetActive(false);
			}
		}
	}
}
