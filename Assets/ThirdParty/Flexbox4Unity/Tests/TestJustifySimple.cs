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
	public class TestJustifySimple
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
				c.flexBasis = new FlexboxBasis(100f, FlexBasis.LENGTH);
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

#if DEBUG_BREAK
		[UnityTest]
		public IEnumerator JustifyThreeSmallItems_UnstretchedStart_START()
#else
		[Test]
		public void JustifyThreeSmallItems_UnstretchedStart_START()
#endif
		{
			/** Switch from Stretch to Start: should now be fixed in BOTH dimensions */
			rootContainer.alignItems = AlignItems.START;
			/** ...and justify to Start since this is Justify test-class */
			rootContainer.justifyContent = FlexJustify.START;
			
			/** Make sure each child has at least SOMETHING driving it's height (now that height is fully self-fixed, container isn't providing one) */
			foreach( var c in childItems )
				c.cssDefaultHeight = new CSS3Length(100f, CSS3LengthType.PIXELS);
			
			
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container
			
		//	foreach( var c in childItems )
			//	Debug.Log(c.name + " - rect: " + c.rectTransform.rect + " - anchored: " + c.rectTransform.anchoredPosition);
#if DEBUG_BREAK
			Debug.Break();
			yield return null;
#endif
			
			AssertSize(childItems[0], NEITHER, cw, ch, "Fixed size should be smaller than canvas" );
			AssertSize(childItems[0], WIDTH, 100f, 0, "Fixed size should be exactly 100f wide" );
			AssertSize(childItems[0], HEIGHT, 0, 100f, "Fixed size should be exactly 100f high" );	
		}

		[Test]
		public void JustifyThreeSmallItems_UnstretchedStart_CENTER()
		{
			/** Switch from Stretch to Start: should now be fixed in BOTH dimensions */
			rootContainer.alignItems = AlignItems.START;
			/** ...and justify to Start since this is Justify test-class */
			rootContainer.justifyContent = FlexJustify.CENTER;
			
			/** Make sure each child has at least SOMETHING driving it's height (now that height is fully self-fixed, container isn't providing one) */
			foreach( var c in childItems )
				c.cssDefaultHeight = new CSS3Length(100f, CSS3LengthType.PIXELS);
			
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container
			
			AssertSize(childItems[0], NEITHER, cw, ch, "Fixed size should be smaller than canvas" );
			AssertSize(childItems[0], WIDTH, 100f, 0, "Fixed size should be exactly 100f wide" );
			AssertSize(childItems[0], HEIGHT, 0, 100f, "Fixed size should be exactly 100f high" );
		}
		
		[Test]
		public void JustifyThreeSmallItems_UnstretchedStart_END()
		{
			/** Switch from Stretch to Start: should now be fixed in BOTH dimensions */
			rootContainer.alignItems = AlignItems.START;
			/** ...and justify to Start since this is Justify test-class */
			rootContainer.justifyContent = FlexJustify.END;
			
			/** Make sure each child has at least SOMETHING driving it's height (now that height is fully self-fixed, container isn't providing one) */
			foreach( var c in childItems )
				c.cssDefaultHeight = new CSS3Length(100f, CSS3LengthType.PIXELS);
			
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container
			
			AssertSize(childItems[0], NEITHER, cw, ch, "Fixed size should be smaller than canvas" );
			AssertSize(childItems[0], WIDTH, 100f, 0, "Fixed size should be exactly 100f wide" );
			AssertSize(childItems[0], HEIGHT, 0, 100f, "Fixed size should be exactly 100f high" );
		}
		
		[Test]
		public void JustifyThreeSmallItems_UnstretchedStart_SPACE_BETWEEN()
		{
			/** Switch from Stretch to Start: should now be fixed in BOTH dimensions */
			rootContainer.alignItems = AlignItems.START;
			/** ...and justify to Start since this is Justify test-class */
			rootContainer.justifyContent = FlexJustify.SPACE_BETWEEN;
			
			/** Make sure each child has at least SOMETHING driving it's height (now that height is fully self-fixed, container isn't providing one) */
			foreach( var c in childItems )
				c.cssDefaultHeight = new CSS3Length(100f, CSS3LengthType.PIXELS);
			
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container
			
			AssertSize(childItems[0], NEITHER, cw, ch, "Fixed size should be smaller than canvas" );
			AssertSize(childItems[0], WIDTH, 100f, 0, "Fixed size should be exactly 100f wide" );
			AssertSize(childItems[0], HEIGHT, 0, 100f, "Fixed size should be exactly 100f high" );
			
			/** Check that the positions are now correct */
			AssertLeftEdgeAt(childItems[0].rectTransform, -cw / 2f, iSize, "First item edge should be at x = -canvas/2f");
			AssertCenterAt(childItems[1].rectTransform, WIDTH, 0f, -1f, iSize, "Second item center should be at x = 0");
			AssertRightEdgeAt(childItems[2].rectTransform, cw / 2f, iSize, "Third item edge should be at x = canvas/2f");
		}
		[Test]
		public void JustifyThreeSmallItems_UnstretchedStart_SPACE_AROUND()
		{
			/** Switch from Stretch to Start: should now be fixed in BOTH dimensions */
			rootContainer.alignItems = AlignItems.START;
			/** ...and justify to Start since this is Justify test-class */
			rootContainer.justifyContent = FlexJustify.SPACE_AROUND;
			
			/** Make sure each child has at least SOMETHING driving it's height (now that height is fully self-fixed, container isn't providing one) */
			foreach( var c in childItems )
				c.cssDefaultHeight = new CSS3Length(iSize.x, CSS3LengthType.PIXELS);
			
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container
			
			AssertSize(childItems[0], NEITHER, cw, ch, "Fixed size should be smaller than canvas" );
			AssertSize(childItems[0], WIDTH, iSize.x, 0, "Fixed size should be exactly 100f wide" );
			AssertSize(childItems[0], HEIGHT, 0, iSize.x, "Fixed size should be exactly 100f high" );
			
			/** Check that the positions are now correct */
			float spareWidth = cw - iSize.x * 3f;
			AssertLeftEdgeAt(childItems[0].rectTransform, -cw / 2f + spareWidth/6f, iSize, "First item edge should be at x = -canvas/2f + 1/N *spare/2f");
			AssertCenterAt(childItems[1].rectTransform, WIDTH, 0f, -1f, iSize, "Second item center should be at x = 0");
			AssertRightEdgeAt(childItems[2].rectTransform, cw / 2f - spareWidth/6f, iSize, "Third item edge should be at x = canvas/2f - 1/N *spare/2f");
		}
		[Test]
		public void JustifyThreeSmallItems_UnstretchedStart_SPACE_EVENLY()
		{
			/** Switch from Stretch to Start: should now be fixed in BOTH dimensions */
			rootContainer.alignItems = AlignItems.START;
			/** ...and justify to Start since this is Justify test-class */
			rootContainer.justifyContent = FlexJustify.SPACE_EVENLY;
			
			/** Make sure each child has at least SOMETHING driving it's height (now that height is fully self-fixed, container isn't providing one) */
			foreach( var c in childItems )
				c.cssDefaultHeight = new CSS3Length(iSize.x, CSS3LengthType.PIXELS);
			
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container
			
			AssertSize(childItems[0], NEITHER, cw, ch, "Fixed size should be smaller than canvas" );
			AssertSize(childItems[0], WIDTH, iSize.x, 0, "Fixed size should be exactly 100f wide" );
			AssertSize(childItems[0], HEIGHT, 0, iSize.x, "Fixed size should be exactly 100f high" );
			
			/** Check that the positions are now correct */
			float spareWidth = cw - iSize.x * 3f;
			AssertLeftEdgeAt(childItems[0].rectTransform, -cw / 2f + spareWidth/4f, iSize, "First item edge should be at x = -canvas/2f + 1/(N+1) * spare");
			AssertCenterAt(childItems[1].rectTransform, WIDTH, 0f, -1f, iSize, "Second item center should be at x = 0");
			AssertRightEdgeAt(childItems[2].rectTransform, cw / 2f - spareWidth/4f, iSize, "Third item edge should be at x = canvas/2f - 1/(N+1) * spare");
		}
		
		[Test]
		public void JustifyThreeSmallItems_UnstretchedEnd_START()
		{
			/** Switch from Stretch to Start: should now be fixed in BOTH dimensions */
			rootContainer.alignItems = AlignItems.END;
			/** ...and justify to Start since this is Justify test-class */
			rootContainer.justifyContent = FlexJustify.START;
			
			/** Make sure each child has at least SOMETHING driving it's height (now that height is fully self-fixed, container isn't providing one) */
			foreach( var c in childItems )
				c.cssDefaultHeight = new CSS3Length(100f, CSS3LengthType.PIXELS);
			
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container
			
			AssertSize(childItems[0], NEITHER, cw, ch, "Fixed size should be smaller than canvas" );
			AssertSize(childItems[0], WIDTH, 100f, 0, "Fixed size should be exactly 100f wide" );
			AssertSize(childItems[0], HEIGHT, 0, 100f, "Fixed size should be exactly 100f high" );	
		}

		[Test]
		public void JustifyThreeSmallItemsStretched_START()
		{
			rootContainer.alignItems = AlignItems.STRETCH;
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the positions are now correct */
			AssertLeftEdgeAt(childItems[0].rectTransform, -cw / 2f, iSize, "First item edge should be at x == -canvas.width/2f");
			AssertLeftEdgeAt(childItems[1].rectTransform, -cw / 2f + 100f, iSize, "Second item edge should be at x = first item + 100f");
			AssertLeftEdgeAt(childItems[2].rectTransform, -cw / 2f + 200f, iSize, "Third item edge should be at x = first item + 100f + 100f");
		}
		
		#if DEBUG_BREAK
		[UnityTest]
		public IEnumerator JustifyThreeSmallItemsStretched_CENTER()
#else
		[Test]
		public void JustifyThreeSmallItemsStretched_CENTER()
		#endif
		{
			
			rootContainer.justifyContent = FlexJustify.CENTER;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			#if DEBUG_BREAK
			//Debug.Break();
			yield return null;
#endif
			
			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the positions are now correct */
			AssertCenterAt(childItems[0].rectTransform, WIDTH, -100f, 0f, iSize, "First item should be at x == -width");
			AssertCenterAt(childItems[1].rectTransform, WIDTH, 0, 0f, iSize, "Second item should be at x = 0");
			AssertCenterAt(childItems[2].rectTransform, WIDTH, 100f, 0f, iSize, "Third item should be at x = +width");
		}


		[Test]
		public void JustifyThreeSmallItemsStretched_END()
		{
			rootContainer.justifyContent = FlexJustify.END;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the positions are now correct */
			AssertRightEdgeAt(childItems[0].rectTransform, -200f + cw / 2f, iSize, "First item edge should be at x = third -200f");
			AssertRightEdgeAt(childItems[1].rectTransform, -100f + cw / 2f, iSize, "Second item edge should be at x = third -100f");
			AssertRightEdgeAt(childItems[2].rectTransform, cw / 2f, iSize, "Third item edge should be at x = canvaswidth/2f");
		}
		
		[Test]
		public void JustifyThreeSmallItemsStretched_SPACE_BETWEEN()
		{
			rootContainer.justifyContent = FlexJustify.SPACE_BETWEEN;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the positions are now correct */
			AssertLeftEdgeAt(childItems[0].rectTransform, -cw / 2f, iSize, "First item edge should be at x = -canvas/2f");
			AssertCenterAt(childItems[1].rectTransform, WIDTH, 0f, -1f, iSize, "Second item center should be at x = 0");
			AssertRightEdgeAt(childItems[2].rectTransform, cw / 2f, iSize, "Third item edge should be at x = canvas/2f");
		}
		
		[Test]
		public void JustifyThreeSmallItemsStretched_SPACE_AROUND()
		{
			float spareWidth = cw - iSize.x * 3f;
			rootContainer.justifyContent = FlexJustify.SPACE_AROUND;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the positions are now correct */
			AssertLeftEdgeAt(childItems[0].rectTransform, -cw / 2f + spareWidth/6f, iSize, "First item edge should be at x = -canvas/2f + 1/N *spare/2f");
			AssertCenterAt(childItems[1].rectTransform, WIDTH, 0f, -1f, iSize, "Second item center should be at x = 0");
			AssertRightEdgeAt(childItems[2].rectTransform, cw / 2f - spareWidth/6f, iSize, "Third item edge should be at x = canvas/2f - 1/N *spare/2f");
		}
		
		[Test]
		public void JustifyThreeSmallItemsStretched_SPACE_EVENLY()
		{
			float spareWidth = cw - iSize.x * 3f;
			rootContainer.justifyContent = FlexJustify.SPACE_EVENLY;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Height should match canvas" );

			/** Check that the positions are now correct */
			AssertLeftEdgeAt(childItems[0].rectTransform, -cw / 2f + spareWidth/4f, iSize, "First item edge should be at x = -canvas/2f + 1/(N+1) * spare");
			AssertCenterAt(childItems[1].rectTransform, WIDTH, 0f, -1f, iSize, "Second item center should be at x = 0");
			AssertRightEdgeAt(childItems[2].rectTransform, cw / 2f - spareWidth/4f, iSize, "Third item edge should be at x = canvas/2f - 1/(N+1) * spare");
		}
		
		[Test]
		public void JustifyThreeSmallItems_ExplicitSizes_Stretched_START()
		{
			rootContainer.alignItems = AlignItems.STRETCH;
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** ... but what happens if they have an explicit size? */
			foreach( var c in childItems )
			{
				c.cssDefaultWidth = new CSS3Length(150f, CSS3LengthType.PIXELS);
				c.cssDefaultHeight = new CSS3Length(150f, CSS3LengthType.PIXELS);
			}
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container
			
			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Stretching: Height should match canvas" );
			AssertSize(childItems[1], HEIGHT_ONLY, cw, ch, "Stretching: Height should match canvas" );
			AssertSize(childItems[2], HEIGHT_ONLY, cw, ch, "Stretching: Height should match canvas" );
		}
		
		[Test]
		public void JustifyThreeSmallItems_ExplicitSizes_Stretched_CENTER()
		{
			rootContainer.alignItems = AlignItems.STRETCH;
			rootContainer.justifyContent = FlexJustify.CENTER;
			
			/** ... but what happens if they have an explicit size? */
			foreach( var c in childItems )
			{
				c.cssDefaultWidth = new CSS3Length(150f, CSS3LengthType.PIXELS);
				c.cssDefaultHeight = new CSS3Length(150f, CSS3LengthType.PIXELS);
			}
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
			
			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Stretching: Height should match canvas" );
			AssertSize(childItems[1], HEIGHT_ONLY, cw, ch, "Stretching: Height should match canvas" );
			AssertSize(childItems[2], HEIGHT_ONLY, cw, ch, "Stretching: Height should match canvas" );

			/** Check that the positions are now correct */
			AssertCenterAt(childItems[0].rectTransform, WIDTH, -100f, 0f, iSize, "First item should be at x == -width");
			AssertCenterAt(childItems[1].rectTransform, WIDTH, 0, 0f, iSize, "Second item should be at x = 0");
			AssertCenterAt(childItems[2].rectTransform, WIDTH, 100f, 0f, iSize, "Third item should be at x = +width");
		}
		
		[Test]
		public void JustifyThreeSmallItems_ExplicitSizes_Stretched_END()
		{
			rootContainer.alignItems = AlignItems.STRETCH;
			rootContainer.justifyContent = FlexJustify.END;
			
			/** ... but what happens if they have an explicit size? */
			foreach( var c in childItems )
			{
				c.cssDefaultWidth = new CSS3Length(150f, CSS3LengthType.PIXELS);
				c.cssDefaultHeight = new CSS3Length(150f, CSS3LengthType.PIXELS);
			}
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
			
			/** should now be own-fixed-val in WIDTH, but stretching to fit canvas's height */
			AssertSize(childItems[0], HEIGHT_ONLY, cw, ch, "Stretching: Height should match canvas" );
			AssertSize(childItems[1], HEIGHT_ONLY, cw, ch, "Stretching: Height should match canvas" );
			AssertSize(childItems[2], HEIGHT_ONLY, cw, ch, "Stretching: Height should match canvas" );

			/** Check that the positions are now correct */
			AssertRightEdgeAt(childItems[0].rectTransform, -200f + cw / 2f, iSize, "First item edge should be at x = third -200f");
			AssertRightEdgeAt(childItems[1].rectTransform, -100f + cw / 2f, iSize, "Second item edge should be at x = third -100f");
			AssertRightEdgeAt(childItems[2].rectTransform, cw / 2f, iSize, "Third item edge should be at x = canvaswidth/2f");
		}
	}
}