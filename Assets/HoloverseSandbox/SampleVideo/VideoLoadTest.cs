using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Euphoria.Backend;
using TMPro;

namespace HoloverseSandbox
{
	public class VideoLoadTest : MonoBehaviour
	{
		[Header("Data")]
		public string thumbnailUrl = string.Empty;
		public string channelName = string.Empty;
		public string viewsCount = string.Empty;

		[Header("UI")]
		public Image thumbnailUi = null;
		public TMP_Text channelUi = null;
		public TMP_Text viewsUi = null;

		private async Task LoadVideo()
		{
			thumbnailUi.sprite = await UnityWebRequestUtilities.SendImageRequestAsync(thumbnailUrl);
			channelUi.text = channelName;
			viewsUi.text = viewsCount;
		}

		private void Start()
		{
			_ = LoadVideo();
		}

#if UNITY_EDITOR

		[ContextMenu("Load Video")]
		private void Editor_LoadVideo()
		{
			_ = LoadVideo();
		}

#endif
	}
}
