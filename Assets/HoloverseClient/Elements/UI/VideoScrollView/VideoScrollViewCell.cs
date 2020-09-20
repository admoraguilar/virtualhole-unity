using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FancyScrollView;
using Midnight;

namespace Holoverse.Client.UI
{
	public class VideoScrollViewCell : FancyScrollRectCell<VideoScrollViewCellData, VideoScrollRectContext>
	{
		[SerializeField]
		private Image _thumbnailImage = null;

		[SerializeField]
		private TMP_Text _titleText = null;

		[SerializeField]
		private TMP_Text _channelText = null;

		[Space]
		[SerializeField]
		private Button _button = null;

		private Vector2 _baseTitleTextSize = Vector2.zero;
		private Vector2 _baseChannelTextSize = Vector2.zero;

		public override void UpdateContent(VideoScrollViewCellData itemData)
		{
			_thumbnailImage.sprite = itemData.thumbnail;
			_titleText.text = itemData.title;
			_channelText.text = itemData.channel;

			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(itemData.onClick.Invoke);
		}

		private void Awake()
		{
			Canvas.ForceUpdateCanvases();
			_baseTitleTextSize = _titleText.rectTransform.rect.size;
			_baseChannelTextSize = _channelText.rectTransform.rect.size;
		}
	}
}
