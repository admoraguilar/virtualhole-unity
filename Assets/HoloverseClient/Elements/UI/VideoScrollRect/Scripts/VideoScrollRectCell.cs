using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FancyScrollView;

namespace Holoverse.Client.UI
{
	public class VideoScrollRectCell : FancyScrollRectCell<VideoScrollRectCellData, VideoScrollRectContext>
	{
		[SerializeField]
		private Image _thumbnailImage = null;

		[SerializeField]
		private Image _creatorImage = null;

		[SerializeField]
		private TMP_Text _creatorText = null;

		[SerializeField]
		private TMP_Text _titleText = null;

		[Space]
		[SerializeField]
		private Button _button = null;

		private void OnButtonClicked(VideoScrollRectCellData itemData)
		{
			itemData.onClick.Invoke();
		}

		public override void UpdateContent(VideoScrollRectCellData itemData)
		{
			_thumbnailImage.sprite = itemData.thumbnail;
			_titleText.text = itemData.title;
			_creatorText.text = itemData.channel;

			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(() => OnButtonClicked(itemData));
		}
	}
}
