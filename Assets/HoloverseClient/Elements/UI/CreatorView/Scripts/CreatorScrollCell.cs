using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight;

namespace Holoverse.Client.UI
{
	public class CreatorScrollCell : MonoBehaviour, ILoopScrollCell
	{
		[SerializeField]
		private Image _creatorAvatarImage = null;

		[SerializeField]
		private TMP_Text _creatorNameText = null;

		[SerializeField]
		private Button _cellButton = null;

		public Type cellDataType => typeof(CreatorScrollCellData);

		public RectTransform rectTrasform => this.GetComponent(ref _rectTransform, () => GetComponent<RectTransform>());
		private RectTransform _rectTransform = null;

		public LayoutElement layoutElement => this.GetComponent(ref _layoutElement, () => GetComponent<LayoutElement>());
		private LayoutElement _layoutElement = null;

		public void UpdateData(object data)
		{
			CreatorScrollCellData itemData = (CreatorScrollCellData)data;
			_creatorAvatarImage.sprite = itemData.creatorAvatar;
			_creatorNameText.text = itemData.creatorName;

			_cellButton.onClick.RemoveAllListeners();
			_cellButton.onClick.AddListener(() => OnCellButtonClicked(itemData));
		}

		private void OnCellButtonClicked(CreatorScrollCellData itemData)
		{
			itemData.onCellClick?.Invoke();
		}
	}
}
