using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VirtualHole.Client.UI
{
	public class InfoButton : MonoBehaviour
	{
		public event Action OnClick = delegate { };

		public Sprite sprite
		{
			get => image != null ? image.sprite : null;
			set {
				if(image == null) { return; }
				
				image.sprite = value;
				image.gameObject.SetActive(image.sprite != null);
			}
		}

		public string header
		{
			get => _headerText.text;
			set => _headerText.text = value;
		}

		public string content
		{
			get => _contentText.text;
			set => _contentText.text = value;
		}

		public Button button => _button;
		[SerializeField]
		private Button _button = null;

		public Image image => _image;
		[SerializeField]
		private Image _image = null;

		public TMP_Text headerText => _headerText;
		[SerializeField]
		private TMP_Text _headerText = null;

		public TMP_Text contentText => _contentText;
		[SerializeField]
		private TMP_Text _contentText = null;

		public void SetData(InfoButtonData data)
		{
			if(data.onClick != null) {
				OnClick += data.onClick;
			}

			sprite = data.sprite;
			header = data.header;
			content = data.content;
		}

		private void OnButtonClick()
		{
			OnClick();
		}

		private void OnEnable()
		{
			button.onClick.AddListener(OnButtonClick);
		}

		private void OnDisable()
		{
			button.onClick.RemoveListener(OnButtonClick);
		}
	}
}
