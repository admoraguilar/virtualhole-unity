using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Midnight.Web;

namespace Holoverse.Client.UI
{
	using Api.Data.Contents;
	using Api.Data.Contents.Creators;

	public class CreatorView : MonoBehaviour
	{
		public Creator creator { get; private set; } = null;

		[Header("References")]
		[SerializeField]
		private Image _avatarImage = null;

		[SerializeField]
		private TMP_Text _nameText = null;

		[Space]
		[SerializeField]
		private RectTransform _socialButtonContainer = null;

		[SerializeField]
		private Button _socialButtonTemplate = null;

		[SerializeField]
		private Button _followButton = null;

		[Space]
		[SerializeField]
		private RectTransform _peekContentContainer = null;

		[SerializeField]
		private PeekContent _peekContentTemplate = null;


		public async Task LoadCreator(Creator creator, CancellationToken cancellationToken = default)
		{
			this.creator = creator;

			_avatarImage.sprite = await ImageGetWebRequest.GetAsync(creator.avatarUrl, null, cancellationToken);
			_nameText.text = creator.universalName;

			while(_socialButtonContainer.childCount > 0) {
				Destroy(_socialButtonContainer.GetChild(0).gameObject);
			}

			foreach(Social social in creator.socials) {
				Button socialButton = Instantiate(_socialButtonTemplate, _socialButtonContainer, false);
				socialButton.name = $"{social.platform}-{social.name}";
				socialButton.image.sprite = UIResources.GetPlatformUI(social.platform).logo;
				socialButton.onClick.AddListener(() => Application.OpenURL(social.url));
			}
		}
	}
}
