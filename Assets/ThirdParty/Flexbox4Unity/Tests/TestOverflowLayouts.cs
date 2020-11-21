//#define DEBUG_BREAK
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
    public class TestOverflowLayouts
    {
		private float cw, ch;
		private FlexContainer fCanvas;
		private FlexContainer rootContainer;
		private FlexItem rootItem;

		[SetUp]
		public void PerTestSetup()
		{
			FlexCanvasFullSizeForTesting(out cw, out ch, out fCanvas, new float[0]);
			rootContainer = fCanvas.AddChildFlexContainerAndFlexItem("rootContainer", out rootItem);
			rootContainer.RelayoutAfterCreatingAtRuntime(); // required, or else the container itself won't get sized by its parent
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
		public IEnumerator TestRow_OverflowsUsingPercents_Right()
		#else
		[Test]
		public void TestRow_OverflowsUsingPercents_Right()
#endif
		{
			/** Three items, all 1/2 the canvas, which cannot shrink */
			var childItems = new[] {rootContainer.AddChildFlexItem("item1"), rootContainer.AddChildFlexItem("item2"), rootContainer.AddChildFlexItem("item3")};
			foreach( var child in childItems )
			{
				child.flexBasis = new FlexboxBasis(50f, FlexBasis.PERCENT);
				child.flexShrink = 0;
			}

			/** START, so should overflow of the RIGHT edge of screen */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.direction = FlexDirection.ROW;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;
			
			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

#if DEBUG_BREAK
			Debug.Break();
			yield return null;
			#endif
			
			/** Nothing shrunk (that's cheating!) */
			AssertSize( childItems[0], WIDTH, cw * 0.5f, ch, "Item shouldn't have shrunk" );
			AssertSize( childItems[1], WIDTH, cw * 0.5f, ch, "Item shouldn't have shrunk" );
			AssertSize( childItems[2], WIDTH, cw * 0.5f, ch, "Item shouldn't have shrunk" );
			
			/** First and Second items on-screen */
			AssertFullyOnScreen( childItems[0].rectTransform, fCanvas.rectTransform.rect.size, "First should be on-screen" );
			AssertFullyOnScreen( childItems[1].rectTransform, fCanvas.rectTransform.rect.size, "2nd should be on-screen" );
			AssertNotFullyOnScreen( childItems[2].rectTransform, fCanvas.rectTransform.rect.size, "3rd should be OFF-screen" );
			
		}

		
		[Test]
		public void TestRow_OverflowsUsingPercents_Left()
		{
			/** Three items, all 1/2 the canvas, which cannot shrink */
			var childItems = new[] {rootContainer.AddChildFlexItem("item1"), rootContainer.AddChildFlexItem("item2"), rootContainer.AddChildFlexItem("item3")};
			foreach( var child in childItems )
			{
				child.flexBasis = new FlexboxBasis(50f, FlexBasis.PERCENT);
				child.flexShrink = 0;
			}

			/** END, so should overflow of the LEFT edge of screen */
			rootContainer.justifyContent = FlexJustify.END;
			rootContainer.direction = FlexDirection.ROW;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
	
			/** Nothing shrunk (that's cheating!) */
			AssertSize( childItems[0], WIDTH, cw * 0.5f, ch, "Item shouldn't have shrunk" );
			AssertSize( childItems[1], WIDTH, cw * 0.5f, ch, "Item shouldn't have shrunk" );
			AssertSize( childItems[2], WIDTH, cw * 0.5f, ch, "Item shouldn't have shrunk" );
			
			
			/** First and Second items on-screen */
			AssertNotFullyOnScreen( childItems[0].rectTransform, fCanvas.rectTransform.rect.size, "1st should NOT be on-screen" );
			AssertFullyOnScreen( childItems[1].rectTransform, fCanvas.rectTransform.rect.size, "2nd should be on-screen" );
			AssertFullyOnScreen( childItems[2].rectTransform, fCanvas.rectTransform.rect.size, "3rd should be on-screen" );
			
		}
		
		[Test]
		public void TestRow_OverflowsUsingPercents_Center()
		{
			/** Three items, all 1/2 the canvas, which cannot shrink */
			var childItems = new[] {rootContainer.AddChildFlexItem("item1"), rootContainer.AddChildFlexItem("item2"), rootContainer.AddChildFlexItem("item3")};
			foreach( var child in childItems )
			{
				child.flexBasis = new FlexboxBasis(50f, FlexBasis.PERCENT);
				child.flexShrink = 0;
			}

			/** Center, so should overflow BOTH edges of screen */
			rootContainer.justifyContent = FlexJustify.CENTER;
			rootContainer.direction = FlexDirection.ROW;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
	
			/** Nothing shrunk (that's cheating!) */
			AssertSize( childItems[0], WIDTH, cw * 0.5f, ch, "Item shouldn't have shrunk" );
			AssertSize( childItems[1], WIDTH, cw * 0.5f, ch, "Item shouldn't have shrunk" );
			AssertSize( childItems[2], WIDTH, cw * 0.5f, ch, "Item shouldn't have shrunk" );
			
			
			/** First and Second items on-screen */
			AssertNotFullyOnScreen( childItems[0].rectTransform, fCanvas.rectTransform.rect.size, "1st should NOT be on-screen" );
			AssertFullyOnScreen( childItems[1].rectTransform, fCanvas.rectTransform.rect.size, "2nd should be on-screen" );
			AssertNotFullyOnScreen( childItems[2].rectTransform, fCanvas.rectTransform.rect.size, "3rd should NOT be on-screen" );
			
		}
		
		[Test]
		public void TestColumn_OverflowsUsingPercents_Bottom()
		{
			/** Three items, all 1/2 the canvas, which cannot shrink */
			var childItems = new[] {rootContainer.AddChildFlexItem("item1"), rootContainer.AddChildFlexItem("item2"), rootContainer.AddChildFlexItem("item3")};
			foreach( var child in childItems )
			{
				child.flexBasis = new FlexboxBasis(50f, FlexBasis.PERCENT);
				child.flexShrink = 0;
			}

			/** START, so should overflow of the BOTTOM edge of screen */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.direction = FlexDirection.COLUMN;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;
			
			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

#if DEBUG_BREAK
			Debug.Break();
			yield return null;
			#endif
			
			/** Nothing shrunk (that's cheating!) */
			AssertSize( childItems[0], HEIGHT, 0, ch * 0.5f, "Item shouldn't have shrunk" );
			AssertSize( childItems[1], HEIGHT, 0, ch * 0.5f, "Item shouldn't have shrunk" );
			AssertSize( childItems[2], HEIGHT, 0,ch * 0.5f, "Item shouldn't have shrunk" );
			
			/** First and Second items on-screen */
			AssertFullyOnScreen( childItems[0].rectTransform, fCanvas.rectTransform.rect.size, "First should be on-screen" );
			AssertFullyOnScreen( childItems[1].rectTransform, fCanvas.rectTransform.rect.size, "2nd should be on-screen" );
			AssertNotFullyOnScreen( childItems[2].rectTransform, fCanvas.rectTransform.rect.size, "3rd should be OFF-screen" );
			
		}

		
		[Test]
		public void TestColumn_OverflowsUsingPercents_Top()
		{
			/** Three items, all 1/2 the canvas, which cannot shrink */
			var childItems = new[] {rootContainer.AddChildFlexItem("item1"), rootContainer.AddChildFlexItem("item2"), rootContainer.AddChildFlexItem("item3")};
			foreach( var child in childItems )
			{
				child.flexBasis = new FlexboxBasis(50f, FlexBasis.PERCENT);
				child.flexShrink = 0;
			}

			/** END, so should overflow of the TOP edge of screen */
			rootContainer.justifyContent = FlexJustify.END;
			rootContainer.direction = FlexDirection.COLUMN;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
	
			/** Nothing shrunk (that's cheating!) */
			AssertSize( childItems[0], HEIGHT, 0, ch * 0.5f, "Item shouldn't have shrunk" );
			AssertSize( childItems[1], HEIGHT, 0, ch * 0.5f, "Item shouldn't have shrunk" );
			AssertSize( childItems[2], HEIGHT, 0,ch * 0.5f, "Item shouldn't have shrunk" );
			
			
			/** First and Second items on-screen */
			AssertNotFullyOnScreen( childItems[0].rectTransform, fCanvas.rectTransform.rect.size, "1st should NOT be on-screen" );
			AssertFullyOnScreen( childItems[1].rectTransform, fCanvas.rectTransform.rect.size, "2nd should be on-screen" );
			AssertFullyOnScreen( childItems[2].rectTransform, fCanvas.rectTransform.rect.size, "3rd should be on-screen" );
			
		}
		
		[Test]
		public void TestColumn_OverflowsUsingPercents_Center()
		{
			/** Three items, all 1/2 the canvas, which cannot shrink */
			var childItems = new[] {rootContainer.AddChildFlexItem("item1"), rootContainer.AddChildFlexItem("item2"), rootContainer.AddChildFlexItem("item3")};
			foreach( var child in childItems )
			{
				child.flexBasis = new FlexboxBasis(50f, FlexBasis.PERCENT);
				child.flexShrink = 0;
			}

			/** Center, so should overflow BOTH top/bottom edges of screen */
			rootContainer.justifyContent = FlexJustify.CENTER;
			rootContainer.direction = FlexDirection.COLUMN;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
	
			/** Nothing shrunk (that's cheating!) */
			AssertSize( childItems[0], HEIGHT, 0, ch * 0.5f, "Item shouldn't have shrunk" );
			AssertSize( childItems[1], HEIGHT, 0, ch * 0.5f, "Item shouldn't have shrunk" );
			AssertSize( childItems[2], HEIGHT, 0,ch * 0.5f, "Item shouldn't have shrunk" );
			
			
			/** First and Second items on-screen */
			AssertNotFullyOnScreen( childItems[0].rectTransform, fCanvas.rectTransform.rect.size, "1st should NOT be on-screen" );
			AssertFullyOnScreen( childItems[1].rectTransform, fCanvas.rectTransform.rect.size, "2nd should be on-screen" );
			AssertNotFullyOnScreen( childItems[2].rectTransform, fCanvas.rectTransform.rect.size, "3rd should NOT be on-screen" );
			
		}
	}
}