using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Flexbox4Unity
{
	public static class FlexboxFixUnityRectTransform
	{
		public static void SetSize(this RectTransform rt, Vector2 newSize)
		{
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
		}

		/**
		 * Unlike the Unity APIs, this returns correctly-typed RectTransform items, and only
		 * returns the children, NOT the descendents (Unity's API has always been wrongly named but it's a bug they won't fix)
		 */
		public static List<RectTransform> Children(this RectTransform rt)
		{
			List<RectTransform> directChildren = new List<RectTransform>();
			foreach( RectTransform t in rt )
			{
				directChildren.Add(t);
			}
			return directChildren;
		}
	}
}