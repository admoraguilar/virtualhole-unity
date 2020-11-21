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
	public class TestSimplePercentages
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
		public void TestFillRow_100percent_WithPercentages()
		{
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			foreach( var c in childItems )
			{
				c.flexBasis = new FlexboxBasis(100f/childItems.Length, FlexBasis.PERCENT);
			}
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, cw / childItems.Length, pixelTolerance, "Item should be 1/Nth of width");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, cw / childItems.Length, pixelTolerance, "Item should be 1/Nth of width");
			Assert.AreApproximatelyEqual(childItems[2].rectTransform.rect.width, cw / childItems.Length, pixelTolerance, "Item should be 1/Nth of width");

			//foreach( var c in childItems )
			//Debug.Log( c.name+" - rect: "+c.rectTransform.rect+" - anchored: "+c.rectTransform.anchoredPosition );            
		}
		
		[Test]
		public void TestFillRow_50percent_WithPercentages()
		{
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			foreach( var c in childItems )
			{
				c.flexBasis = new FlexboxBasis(50f/childItems.Length, FlexBasis.PERCENT);
			}
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, 0.5f*cw / childItems.Length, pixelTolerance, "Item should be 0.5 * 1/Nth of width");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, 0.5f*cw / childItems.Length, pixelTolerance, "Item should be 0.5 * 1/Nth of width");
			Assert.AreApproximatelyEqual(childItems[2].rectTransform.rect.width, 0.5f*cw / childItems.Length, pixelTolerance, "Item should be 0.5 * 1/Nth of width");

			//foreach( var c in childItems )
			//Debug.Log( c.name+" - rect: "+c.rectTransform.rect+" - anchored: "+c.rectTransform.anchoredPosition );            
		}
		
		[Test]
		public void TestFillRow_50percent_WithPercentages_Grow()
		{
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			foreach( var c in childItems )
			{
				c.flexBasis = new FlexboxBasis(50f/childItems.Length, FlexBasis.PERCENT);
				c.flexGrow = 1;
			}
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, cw / childItems.Length, pixelTolerance, "Item should be 1/Nth of width");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, cw / childItems.Length, pixelTolerance, "Item should be 1/Nth of width");
			Assert.AreApproximatelyEqual(childItems[2].rectTransform.rect.width, cw / childItems.Length, pixelTolerance, "Item should be 1/Nth of width");

			//foreach( var c in childItems )
			//Debug.Log( c.name+" - rect: "+c.rectTransform.rect+" - anchored: "+c.rectTransform.anchoredPosition );            
		}

		
		[Test]
		public void TestFillRow_200percent_WithPercentages_NoShrink()
		{
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			foreach( var c in childItems )
			{
				c.flexBasis = new FlexboxBasis(200f/childItems.Length, FlexBasis.PERCENT);
				c.flexShrink = 0;
			}
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, 2f * cw / childItems.Length, pixelTolerance, "Item should be 2/Nth of width");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, 2f * cw / childItems.Length, pixelTolerance, "Item should be 2/Nth of width");
			Assert.AreApproximatelyEqual(childItems[2].rectTransform.rect.width, 2f * cw / childItems.Length, pixelTolerance, "Item should be 2/Nth of width");

			//foreach( var c in childItems )
			//Debug.Log( c.name+" - rect: "+c.rectTransform.rect+" - anchored: "+c.rectTransform.anchoredPosition );            
		}
		
		[Test]
		public void TestFillRow_200percent_WithPercentages_Shrinkable()
		{
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			foreach( var c in childItems )
			{
				c.flexBasis = new FlexboxBasis(200f/childItems.Length, FlexBasis.PERCENT);
				c.flexShrink = 1;
			}
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the widths are now correct */
			Assert.AreApproximatelyEqual(childItems[0].rectTransform.rect.width, cw / childItems.Length, pixelTolerance, "Item should be 1/Nth of width");
			Assert.AreApproximatelyEqual(childItems[1].rectTransform.rect.width, cw / childItems.Length, pixelTolerance, "Item should be 1/Nth of width");
			Assert.AreApproximatelyEqual(childItems[2].rectTransform.rect.width, cw / childItems.Length, pixelTolerance, "Item should be 1/Nth of width");

			//foreach( var c in childItems )
			//Debug.Log( c.name+" - rect: "+c.rectTransform.rect+" - anchored: "+c.rectTransform.anchoredPosition );            
		}
	}
}