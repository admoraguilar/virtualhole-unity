using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEngine;

#if LITE_VERSION
#else
public static class HelpersForFlexItem
{
	public static void CloneSettingsFrom(this FlexItem dest, FlexItem src)
	{
		dest.flexGrow = src.flexGrow;
		dest.flexShrink = src.flexShrink;
		dest.flexBasis = src.flexBasis;
		dest.cssMargins = src.cssMargins;
		dest.cssPadding = src.cssPadding;
		
		Debug.LogError("This experimental method is incomplete and untested");
	}
}
#endif