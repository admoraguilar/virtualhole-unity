using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Flexbox4Unity
{
	public class DemoChildSpawner : MonoBehaviour
	{
		public RectTransform cloneContainer = null;
		public RectTransform cloneRect = null;
		
		public ScrollRect scrollRect = null;

		private void Update()
		{
			if(scrollRect.verticalNormalizedPosition <= .2f) {
				Instantiate(cloneRect.gameObject, cloneContainer, false);
			}
		}
	}
}
