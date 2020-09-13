using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Holoverse.Client
{
	public class DynamicFontLoader : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text[] _texts = null;

		private void Awake()
		{
			_texts = GetComponentsInChildren<TMP_Text>();
		}

		private void Start()
		{
			string[] fontPaths = Font.GetPathsToOSFonts();
			foreach(string font in fontPaths) {
				Debug.Log(font);
			}

			//Font osFont = new Font("");
			//TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(osFont);
			//fontAsset.fall
		}
	}
}
