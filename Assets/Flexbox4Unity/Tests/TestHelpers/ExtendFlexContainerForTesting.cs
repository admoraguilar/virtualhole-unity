using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEngine;

public static class ExtendFlexContainerForTesting
{
	/** Need to write this out many many times when writing Unit-tests, so ... let's make it shorter (without polluting the main class) */
	public static float width(this FlexContainer fc)
	{
		return fc.rectTransform.rect.width;
	}
	
	/** Need to write this out many many times when writing Unit-tests, so ... let's make it shorter (without polluting the main class) */
	public static float height(this FlexContainer fc)
	{
		return fc.rectTransform.rect.height;
	}
}