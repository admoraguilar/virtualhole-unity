using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VirtualHole.Client.UI
{
	public class InfoButton : MonoBehaviour
	{
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
	}
}
