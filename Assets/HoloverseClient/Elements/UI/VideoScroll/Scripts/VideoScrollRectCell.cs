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
		private Image _indicatorImage = null;

		[SerializeField]
		private Image _creatorImage = null;

		[SerializeField]
		private TMP_Text _creatorNameText = null;

		[SerializeField]
		private TMP_Text _titleText = null;

		[SerializeField]
		private TMP_Text _dateText = null;

		[SerializeField]
		private Button _cellButton = null;

		[SerializeField]
		private Button _optionsButton = null;

		public override void UpdateContent(VideoScrollRectCellData itemData)
		{
			_thumbnailImage.sprite = itemData.thumbnailSprite;

			if(itemData.indicatorSprite != null) {
				_indicatorImage.gameObject.SetActive(true);
				_indicatorImage.sprite = itemData.indicatorSprite;
			} else {
				_indicatorImage.gameObject.SetActive(false);
			}

			_creatorImage.sprite = itemData.creatorSprite;
			_creatorNameText.text = itemData.creatorName;

			_titleText.text = itemData.title;
			_dateText.text = itemData.date;

			_cellButton.onClick.RemoveAllListeners();
			_cellButton.onClick.AddListener(() => OnCellButtonClicked(itemData));

			if(itemData.onOptionsClick != null) {
				_optionsButton.gameObject.SetActive(true);
				_optionsButton.onClick.RemoveAllListeners();
				_optionsButton.onClick.AddListener(() => OnOptionsButtonClicked(itemData));
			} else {
				_optionsButton.gameObject.SetActive(false);
			}
		}

		private void OnCellButtonClicked(VideoScrollRectCellData itemData)
		{
			itemData.onCellClick?.Invoke();
		}

		private void OnOptionsButtonClicked(VideoScrollRectCellData itemData)
		{
			itemData.onOptionsClick?.Invoke();
		}
	}
}
