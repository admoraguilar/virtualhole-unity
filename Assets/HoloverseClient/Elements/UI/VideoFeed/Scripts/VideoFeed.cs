using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Holoverse.Client.UI
{
	public class VideoFeed : MonoBehaviour
	{
		[Serializable]
		public class ContextButton
		{
			public Button button { get => _button; }
			[SerializeField]
			private Button _button = null;

			public Image image { get => _image; }
			[SerializeField]
			private Image _image = null;

			public TMP_Text text { get => _text; }
			[SerializeField]
			private TMP_Text _text = null;
		}

		public VideoScrollRect videoScroll => _videoScroll;
		[SerializeField]
		private VideoScrollRect _videoScroll = null;

		public ContextButton contextButton => _contextButton;
		[SerializeField]
		private ContextButton _contextButton = null;

		public TMP_Dropdown dropdown => _dropDown;
		[SerializeField]
		private TMP_Dropdown _dropDown = null;
	}
}
