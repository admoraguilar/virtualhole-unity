using UnityEngine;
using UnityEngine.UI;

namespace VirtualHole.Client.UI
{
	[CreateAssetMenu(menuName = "VirtualHole/UI/Video Scroll Cell Data Processor")]
	public class VideoScrollCellDataProcessor : LoopScrollCellDataProcessor<VideoButton, VideoScrollCellData>
	{
		public override void ProcessData(VideoButton instance, VideoScrollCellData data)
		{
			instance.thumbnailImage.sprite = data.thumbnailSprite;

			if(data.indicatorSprite != null) {
				instance.indicatorImage.gameObject.SetActive(true);
				instance.indicatorImage.sprite = data.indicatorSprite;
			} else {
				instance.indicatorImage.gameObject.SetActive(false);
			}

			instance.creatorImage.sprite = data.creatorSprite;
			instance.creatorNameText.text = data.creatorName;

			instance.titleText.text = data.title;
			instance.dateText.text = data.date;

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
