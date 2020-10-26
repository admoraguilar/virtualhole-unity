using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Holoverse.Client.UI
{
	public class VideoPeekScroll : MonoBehaviour
	{
		public Image background => _background;
		[SerializeField]
		private Image _background = null;

		public TMP_Text header => _header;
		[SerializeField]
		private TMP_Text _header = null;

		public VideoScrollRect scroll => _scroll;
		[SerializeField]
		private VideoScrollRect _scroll = null;

		public Button optionButton => _optionButton;
		[SerializeField]
		private Button _optionButton = null;
	}
}
