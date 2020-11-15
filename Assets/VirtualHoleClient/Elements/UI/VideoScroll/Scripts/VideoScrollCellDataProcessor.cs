using UnityEngine;
using UnityEngine.UI;

namespace VirtualHole.Client.UI
{
	[CreateAssetMenu(menuName = "VirtualHole/UI/Video Scroll Cell Data Processor")]
	public class VideoScrollCellDataProcessor : LoopScrollCellDataProcessor<VideoButton, VideoScrollCellData>
	{
		public override void ProcessData(VideoButton instance, VideoScrollCellData data)
		{
			// Usage of .SetText() instead of .text
			// https://forum.unity.com/threads/textmeshpro-settext-vs-text-sometext-speed-test.926411/
			instance.thumbnailImage.sprite = data.videoDTO.thumbnailSprite;

			if(data.videoDTO.indicatorSprite != null) {
				instance.indicatorImage.gameObject.SetActive(true);
				instance.indicatorImage.sprite = data.videoDTO.indicatorSprite;
				instance.dateText.SetText(data.videoDTO.scheduleDateDisplay, false);
			} else {
				instance.indicatorImage.gameObject.SetActive(false);
				instance.dateText.SetText(data.videoDTO.creationDateDisplay, false);
			}

			instance.creatorImage.sprite = data.videoDTO.creatorDTO.avatarSprite;
			instance.creatorNameText.text = data.videoDTO.raw.creator;

			instance.titleText.SetText(data.videoDTO.raw.title, false);

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
