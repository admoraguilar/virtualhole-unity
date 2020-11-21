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
	public class TestRowMinWidths
	{
		private Vector2 iSize;
		private float cw, ch;
		private FlexContainer fCanvas;
		private FlexContainer rootContainer;
		private FlexItem[] childItems;
		
		[SetUp]
		public void PerTestSetup()
		{
			iSize = 100f * Vector2.one;
			FlexCanvasFullSizeForTesting(out cw, out ch, out fCanvas, new[] {iSize.x});
			NAssert.Greater(cw, iSize.x * 3, "Need the canvas to be wide enough to hold three items with space to spare");
			rootContainer = fCanvas.AddChildFlexContainerAndFlexItem("rootContainer", out FlexItem rootItem);
			rootContainer.RelayoutAfterCreatingAtRuntime(); // required, or else the container itself won't get sized by its parent

			/** Three items, all smaller than the canvas */
			childItems = new[] {rootContainer.AddChildFlexItem("item1"), rootContainer.AddChildFlexItem("item2"), rootContainer.AddChildFlexItem("item3")};
			foreach( var c in childItems )
			{
				c.flexGrow = 0;
			}

			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // causes all the new childitems to be laid-out in one pass
		}

		[TearDown]
		public void PerTestCleanup()
		{
			var guiElements = Object.FindObjectsOfType<RectTransform>();
			foreach( var e in guiElements )
				Object.DestroyImmediate( e.gameObject );
			
			childItems = null;
		}



		[Test]
		public void Test0Length_With_MinWidthPx()
		{
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			float minPixelWidth = 200f;
			NAssert.Greater(cw, 3f * minPixelWidth, "Need a canvas wide enough for this test" );
			foreach( var c in childItems )
			{
				c.flexGrow = c.flexShrink = 0;
				c.cssMinWidth = new CSS3Length( minPixelWidth, CSS3LengthType.PIXELS );
			}
			childItems[0].flexBasis = new FlexboxBasis(0f, FlexBasis.PERCENT);
			childItems[1].flexBasis = new FlexboxBasis(0f, FlexBasis.LENGTH);
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, minPixelWidth, pixelTolerance, "Item should be the fixed pixel min-width");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, minPixelWidth, pixelTolerance, "Item should be the fixed pixel min-width");
			
			//foreach( var c in childItems )
			//Debug.Log( c.name+" - rect: "+c.rectTransform.rect+" - anchored: "+c.rectTransform.anchoredPosition );            
		}
		
		[Test]
		public void Test0Length_With_MinWidthPercent()
		{
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			float minPixelWidth = 200f;
			NAssert.Greater(cw, 3f * minPixelWidth, "Need a canvas wide enough for this test" );
			
			foreach( var c in childItems )
			{
				c.flexGrow = c.flexShrink = 0;
				c.cssMinWidth = new CSS3Length( (100f*minPixelWidth)/cw, CSS3LengthType.PERCENT );
			}
			childItems[0].flexBasis = new FlexboxBasis(0f, FlexBasis.PERCENT);
			childItems[1].flexBasis = new FlexboxBasis(0f, FlexBasis.LENGTH);
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, minPixelWidth, pixelTolerance, "Item should be the fixed percent min-width");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, minPixelWidth, pixelTolerance, "Item should be the fixed percent min-width");
			
			//foreach( var c in childItems )
			//Debug.Log( c.name+" - rect: "+c.rectTransform.rect+" - anchored: "+c.rectTransform.anchoredPosition );            
		}
		
		[Test]
		public void Test0Length_With_MinWidthNoShrink_ForcesOthersToShrink()
		{
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			float minPixelWidth = cw / 2f;
			
			foreach( var c in childItems )
			{
				c.flexGrow = c.flexShrink = 1;
				c.flexBasis = new FlexboxBasis(100f/3f, FlexBasis.PERCENT);
			}
			/** Make the first item the minwidth'r */
			childItems[0].cssMinWidth = new CSS3Length( 50f, CSS3LengthType.PERCENT );
			childItems[0].flexShrink = 0;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, cw/2f, pixelTolerance, "Item should have half the total space");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, cw/4f, pixelTolerance, "Item should have half the space not taken by first item");
			Assert.AreApproximatelyEqual(childItems[2].rectTransform.rect.width, cw/4f, pixelTolerance, "Item should have half the space not taken by first item");

			/********** Check it works with the last item being the one that forces the shrink *******/
			foreach( var c in childItems )
			{
				c.flexGrow = c.flexShrink = 1;
				c.flexBasis = new FlexboxBasis(100f/3f, FlexBasis.PERCENT);
				c.cssMinWidth = CSS3Length.None;
			}
			/** Make the last item the minwidth'r */
			childItems[2].cssMinWidth = new CSS3Length( 50f, CSS3LengthType.PERCENT );
			childItems[2].flexShrink = 0;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
			
			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, cw/4f, pixelTolerance, "Item should have half the space not taken by last item");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, cw/4f, pixelTolerance, "Item should have half the space not taken by last item");
			Assert.AreApproximatelyEqual(childItems[2].rectTransform.rect.width, cw/2f, pixelTolerance, "Item should have half the total space");
			            
		}
		
		/** Even with shrink=1, a min-width should refuse to shrink less than the minwidth value */
		[Test]
		public void Test0Length_With_MinWidth_ForcesOthersToShrink()
		{
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			float minPixelWidth = cw / 2f;
			
			foreach( var c in childItems )
			{
				c.flexGrow = c.flexShrink = 1;
				c.flexBasis = new FlexboxBasis(100f/3f, FlexBasis.PERCENT);
			}
			/** Make the first item the minwidth'r */
			childItems[0].cssMinWidth = new CSS3Length( 50f, CSS3LengthType.PERCENT );
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, cw/2f, pixelTolerance, "Item should have half the total space");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, cw/4f, pixelTolerance, "Item should have half the space not taken by first item");
			Assert.AreApproximatelyEqual(childItems[2].rectTransform.rect.width, cw/4f, pixelTolerance, "Item should have half the space not taken by first item");

			/********** Check it works with the last item being the one that forces the shrink *******/
			foreach( var c in childItems )
			{
				c.flexGrow = c.flexShrink = 1;
				c.flexBasis = new FlexboxBasis(100f/3f, FlexBasis.PERCENT);
				c.cssMinWidth = CSS3Length.None;
			}
			/** Make the last item the minwidth'r */
			childItems[2].cssMinWidth = new CSS3Length( 50f, CSS3LengthType.PERCENT );
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
			
			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, cw/4f, pixelTolerance, "Item should have half the space not taken by last item");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, cw/4f, pixelTolerance, "Item should have half the space not taken by last item");
			Assert.AreApproximatelyEqual(childItems[2].rectTransform.rect.width, cw/2f, pixelTolerance, "Item should have half the total space");
			            
		}
		
		
	}
}