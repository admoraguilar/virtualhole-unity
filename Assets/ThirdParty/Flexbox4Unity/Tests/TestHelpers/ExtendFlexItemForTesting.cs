using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEngine;

public static class ExtendFlexItemForTesting
{
	/** Need to write this out many many times when writing Unit-tests, so ... let's make it shorter (without polluting the main class) */
	public static float width(this FlexItem fi )
	{
		return fi.rectTransform.rect.width;
	}
	
	/** Need to write this out many many times when writing Unit-tests, so ... let's make it shorter (without polluting the main class) */
	public static float height(this FlexItem fi)
	{
		return fi.rectTransform.rect.height;
	}
}