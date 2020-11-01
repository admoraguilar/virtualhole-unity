using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VirtualHole.Client.UI
{
	public class VideoButton : MonoBehaviour
	{
		public Image thumbnailImage => _thumbnailImage;
		[SerializeField]
		private Image _thumbnailImage = null;

		public Image indicatorImage => _indicatorImage;
		[SerializeField]
		private Image _indicatorImage = null;

		public Image creatorImage => _creatorImage;
		[SerializeField]
		private Image _creatorImage = null;

		public TMP_Text creatorNameText => _creatorNameText;
		[SerializeField]
		private TMP_Text _creatorNameText = null;

		public TMP_Text titleText => _titleText;
		[SerializeField]
		private TMP_Text _titleText = null;

		public TMP_Text dateText => _dateText;
		[SerializeField]
		private TMP_Text _dateText = null;

		public Button button => _button;
		[SerializeField]
		private Button _button = null;

		public Button optionsButton => _optionsButton;
		[SerializeField]
		private Button _optionsButton = null;
	}
}
