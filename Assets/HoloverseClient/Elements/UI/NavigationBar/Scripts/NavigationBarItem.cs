using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Holoverse.Client.UI
{
	public class NavigationBarItem : MonoBehaviour
	{
		public Button button { get => _button; set => _button = value; }
		[SerializeField]
		private Button _button = null;

		public Image image { get => _image; set => _image = value; }
		[SerializeField]
		private Image _image = null;

		public TMP_Text text { get => _text; set => _text = value; }
		[SerializeField]
		private TMP_Text _text = null;
	}
}