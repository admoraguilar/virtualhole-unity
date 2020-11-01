using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VirtualHole.Client.UI
{
	public class CreatorButton : MonoBehaviour
	{
		public Image creatorAvatarImage => _creatorAvatarImage;
		[SerializeField]
		private Image _creatorAvatarImage = null;

		public TMP_Text creatorNameText => _creatorNameText;
		[SerializeField]
		private TMP_Text _creatorNameText = null;

		public Button button => _button;
		[SerializeField]
		private Button _button = null;
	}
}
