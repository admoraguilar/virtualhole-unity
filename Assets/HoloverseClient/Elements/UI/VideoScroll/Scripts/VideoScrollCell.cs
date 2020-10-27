using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;

namespace Holoverse.Client.UI
{
	public class VideoScrollCell : MonoBehaviour, ILoopScrollCell
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

		public Type cellDataType => typeof(VideoScrollCellData);

		public RectTransform rectTrasform => this.GetComponent(ref _rectTransform, () => GetComponent<RectTransform>());
		private RectTransform _rectTransform = null;

		public LayoutElement layoutElement => this.GetComponent(ref _layoutElement, () => GetComponent<LayoutElement>());
		private LayoutElement _layoutElement = null;

		public void UpdateData(object data)
		{
			VideoScrollCellData itemData = (VideoScrollCellData)data;
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

		private void OnCellButtonClicked(VideoScrollCellData itemData)
		{
			itemData.onCellClick?.Invoke();
		}

		private void OnOptionsButtonClicked(VideoScrollCellData itemData)
		{
			itemData.onOptionsClick?.Invoke();
		}
	}
}
