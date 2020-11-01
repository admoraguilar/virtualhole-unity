using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;

namespace VirtualHole.Client.UI
{
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(LayoutElement))]
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

		public void SetData(VideoScrollCellData data)
		{
			_thumbnailImage.sprite = data.thumbnailSprite;

			if(data.indicatorSprite != null) {
				_indicatorImage.gameObject.SetActive(true);
				_indicatorImage.sprite = data.indicatorSprite;
			} else {
				_indicatorImage.gameObject.SetActive(false);
			}

			_creatorImage.sprite = data.creatorSprite;
			_creatorNameText.text = data.creatorName;

			_titleText.text = data.title;
			_dateText.text = data.date;

			_cellButton.onClick.RemoveAllListeners();
			_cellButton.onClick.AddListener(() => OnCellButtonClicked(data));

			if(data.onOptionsClick != null) {
				_optionsButton.gameObject.SetActive(true);
				_optionsButton.onClick.RemoveAllListeners();
				_optionsButton.onClick.AddListener(() => OnOptionsButtonClicked(data));
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

		Type ILoopScrollCell.dataType => typeof(VideoScrollCellData);
		void ILoopScrollCell.SetData(object data) => ObjectUtilities.SetDataIfCompatible<VideoScrollCellData>(data, SetData);
	}
}
