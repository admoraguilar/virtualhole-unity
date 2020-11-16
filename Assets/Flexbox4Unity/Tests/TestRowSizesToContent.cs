//#define DEBUG_BREAK
using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Tests.FlexboxTestHelpers;
using static Tests.WidthHeighMatch;
using Assert = UnityEngine.Assertions.Assert;
using NAssert = NUnit.Framework.Assert;
using Object = UnityEngine.Object;

namespace Tests
{
	public class TestRowSizesToContent
	{
		private Vector2 iSize;
		private float cw, ch;
		private FlexContainer fCanvas;
		private FlexContainer rootContainer;
		private FlexItem rootItem;
		private FlexItem[] childItems;

		[SetUp]
		public void PerTestSetup()
		{
			iSize = 100f * Vector2.one;
			FlexCanvasFullSizeForTesting(out cw, out ch, out fCanvas, new[] {iSize.x});
			NAssert.Greater(cw, iSize.x * 3, "Need the canvas to be wide enough to hold three items with space to spare");
			rootContainer = fCanvas.AddChildFlexContainerAndFlexItem("rootContainer", out rootItem);
			rootContainer.RelayoutAfterCreatingAtRuntime(); // required, or else the container itself won't get sized by its parent

			/** Three items, all smaller than the canvas */
			childItems = new[] {rootContainer.AddChildFlexItem("item1"), rootContainer.AddChildFlexItem("item2"), rootContainer.AddChildFlexItem("item3")};
		}

		[TearDown]
		public void PerTestCleanup()
		{
			var guiElements = Object.FindObjectsOfType<RectTransform>();
			foreach( var e in guiElements )
				Object.DestroyImmediate(e.gameObject);
		}


		#if DEBUG_BREAK
		[UnityTest]
		public IEnumerator TestExpandsToHoldThreePixelItems()
		#else
		[Test]
		public void TestExpandsToHoldThreePixelItems()
#endif
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;
			
			foreach( var c in childItems )
			{
				c.flexGrow = c.flexShrink = 0;
				c.flexBasis = new FlexboxBasis(100f, FlexBasis.LENGTH);
			}

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

#if DEBUG_BREAK
			Debug.Break();
			yield return null;
			#endif
			/** It should match the main axis, but have kept the parent's cross-axis */
			AssertSize( rootContainer, WIDTH, 3 * 100f, 0, "Item should expand to fit the three child items" );
		}
	}
}