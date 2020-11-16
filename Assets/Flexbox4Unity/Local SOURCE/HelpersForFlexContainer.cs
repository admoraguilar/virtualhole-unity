using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEngine;

#if LITE_VERSION
#else
public static class HelpersForFlexContainer
{
	public static List<FlexItem> FlexChildren(this FlexContainer fc)
	{
		List<FlexItem> flexChildren = new List<FlexItem>();
		foreach( Transform t in fc.transform )
		{
			var fi = t.gameObject.GetComponent<FlexItem>();
			if( fi != null )
				flexChildren.Add(fi);
		}
		return flexChildren;
	}

	public static void CloneSettingsFrom(this FlexContainer dest, FlexContainer src)
	{
		dest.wrap = src.wrap;
		dest.direction = src.direction;
		dest.alignItems = src.alignItems;
		dest.justifyContent = src.justifyContent;
	}
}
#endif