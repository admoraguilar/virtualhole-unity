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
	public class TestNoWrapCrossSizing
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
			/** Three items, of different initial heights (to test cross-sizing + overrides) - the last one too high for the canvas! */
			childItems[0].cssDefaultHeight = new CSS3Length( 150f, CSS3LengthType.PIXELS);
			childItems[1].cssDefaultHeight = new CSS3Length( ch/2f, CSS3LengthType.PIXELS);
			childItems[2].cssDefaultHeight = new CSS3Length( ch*2f, CSS3LengthType.PIXELS);
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
		public void TestSingleLine_Stretch()
#endif
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;
			
			foreach( var c in childItems )
			{
				c.flexGrow = c.flexShrink = 0;
			}

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

#if DEBUG_BREAK
			Debug.Break();
			yield return null;
			#endif
			
			/** All items should match the cross axis - even though shrink is disabled, cross-axis sizing DOES NOT USE shrink */
			AssertSize( childItems[0], HEIGHT, 0, ch, "Item should have been set to cross-axis size" );
			AssertSize( childItems[1], HEIGHT, 0, ch, "Item should have been set to cross-axis size" );
			AssertSize( childItems[2], HEIGHT, 0, ch, "Item should have been set to cross-axis size" );
		}
		
		[Test]
		public void TestAutoColumnOfAutoRows_EachRowSetsCrossSizeCorrectly_NoGrowingOrShrinking()
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;
			
			/** Replace the defaults - TODO: this test should probably move to a different class */
			foreach( var c in childItems )
			{
				GameObject.DestroyImmediate(c.gameObject);
			}
			childItems = null;
			
			/** Change the rootcontainer: we need it to be a column for this test */
			rootContainer.direction = FlexDirection.COLUMN;
			
			/** Create what we need: two rows of two items */
			FlexContainer row1Container = rootContainer.AddChildFlexContainerAndFlexItem("row-1", out FlexItem row1Item);
			FlexContainer row2Container = rootContainer.AddChildFlexContainerAndFlexItem("row-2", out FlexItem row2Item);
			FlexItem row1Item1 = row1Container.AddChildFlexItem("item1.1");
			FlexItem row1Item2 = row1Container.AddChildFlexItem("item1.2");
			FlexItem row2Item1 = row2Container.AddChildFlexItem("item2.1");
			FlexItem row2Item2 = row2Container.AddChildFlexItem("item2.2");
			
			/** Rows should be defaults, but let's be safe */
			row1Container.alignItems = row2Container.alignItems = AlignItems.STRETCH;
			row1Container.justifyContent = row2Container.justifyContent = FlexJustify.START;
			
			/** Disable wrap, and refuse to shrink - shrinking allows the possibility of working around bugs, by the parent doing post-resizing fixups! */
			row1Container.wrap = row2Container.wrap = FlexWrap.NOWRAP;
			row1Item.flexGrow = row2Item.flexGrow = 0;
			row1Item.flexShrink = row2Item.flexShrink = 0;
			
			/** Make all children fixed height, so that the rows SHOULD infer a small fixed height from that */
			childItems = new[] {row1Item1, row1Item2, row2Item1, row2Item2};
			float childSetHeight = 100f;
			foreach( var c in childItems )
			{
				c.cssDefaultHeight = new CSS3Length(childSetHeight, CSS3LengthType.PIXELS);
				c.flexGrow = c.flexShrink = 0; // disable shrinking, because it can mask bugs here
			}

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
			
			/**
			 * Expected:
			 *
			 * 1. all rows have same height as we set all children to have
			 * 2. all children have their set heights (no grow/shrink forced on them by a parent)
			 */
			AssertSize( row1Item, HEIGHT, 0, childSetHeight, "Row should match child-height, since it's AUTO" );
			AssertSize( row2Item, HEIGHT, 0, childSetHeight, "Row should match child-height, since it's AUTO" );
			
			AssertSize( row1Item1, HEIGHT, 0, childSetHeight, "Child should still have its child-height" );
			AssertSize( row1Item2, HEIGHT, 0, childSetHeight, "Child should still have its child-height" );
			AssertSize( row2Item1, HEIGHT, 0, childSetHeight, "Child should still have its child-height" );
			AssertSize( row2Item2, HEIGHT, 0, childSetHeight, "Child should still have its child-height" );
			
		}
		
		#if DEBUG_BREAK
		[UnityTest]
		public IEnumerator TestAutoColumnOfAutoRows_EachRowSetsCrossSizeCorrectly_ShrinkAllowed()
#else
		[Test]
		public void TestAutoColumnOfAutoRows_EachRowSetsCrossSizeCorrectly_ShrinkAllowed_NoGrow()
		#endif
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;
			
			/** Replace the defaults - TODO: this test should probably move to a different class */
			foreach( var c in childItems )
			{
				GameObject.DestroyImmediate(c.gameObject);
			}
			childItems = null;
			
			/** Change the rootcontainer: we need it to be a column for this test */
			rootContainer.direction = FlexDirection.COLUMN;
			
			/** Create what we need: two rows of two items */
			FlexContainer row1Container = rootContainer.AddChildFlexContainerAndFlexItem("row-1", out FlexItem row1Item);
			FlexContainer row2Container = rootContainer.AddChildFlexContainerAndFlexItem("row-2", out FlexItem row2Item);
			FlexItem row1Item1 = row1Container.AddChildFlexItem("item1.1");
			FlexItem row1Item2 = row1Container.AddChildFlexItem("item1.2");
			FlexItem row2Item1 = row2Container.AddChildFlexItem("item2.1");
			FlexItem row2Item2 = row2Container.AddChildFlexItem("item2.2");
			
			/** Rows should be defaults, but let's be safe */
			row1Container.alignItems = row2Container.alignItems = AlignItems.STRETCH;
			row1Container.justifyContent = row2Container.justifyContent = FlexJustify.START;
			
			/** Disable wrap + grow, and allow shrinking this time */
			row1Container.wrap = row2Container.wrap = FlexWrap.NOWRAP;
			row1Item.flexGrow = row2Item.flexGrow = 0;
			row1Item.flexShrink = row2Item.flexShrink = 1;
			
			/** Make all children fixed height, so that the rows SHOULD infer a small fixed height from that */
			childItems = new[] {row1Item1, row1Item2, row2Item1, row2Item2};
			float childSetBasis = 100f;
			float childSetCross = 100f;
			foreach( var c in childItems )
			{
				c.flexBasis = new FlexboxBasis(childSetBasis,FlexBasis.LENGTH);
				c.cssDefaultHeight = new CSS3Length(childSetCross, CSS3LengthType.PIXELS);
				c.flexGrow = c.flexShrink = 0; // disable shrinking, because it can mask bugs here
			}

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
			
			#if DEBUG_BREAK
			Debug.Break();
			yield return null;
			#endif
			
			/**
			 * Expected:
			 *
			 * 1. all rows have same height as we set all children to have
			 * 2. all children have their set heights (no grow/shrink forced on them by a parent)
			 */
			AssertSize( row1Item, HEIGHT, 0, childSetCross, "Row should match child-height, since it's AUTO" );
			AssertSize( row2Item, HEIGHT, 0, childSetCross, "Row should match child-height, since it's AUTO" );
			
			AssertSize( row1Item1, HEIGHT, 0, childSetCross, "Child should still have its child-height" );
			AssertSize( row1Item2, HEIGHT, 0, childSetCross, "Child should still have its child-height" );
			AssertSize( row2Item1, HEIGHT, 0, childSetCross, "Child should still have its child-height" );
			AssertSize( row2Item2, HEIGHT, 0, childSetCross, "Child should still have its child-height" );
			
		}

		[Test]
		public void TestAutoColumnOfAutoRows_EachRowSetsCrossSizeCorrectly_ShrinkAndGrowAllowed_RowsCantStretch()
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;
			
			/** Replace the defaults - TODO: this test should probably move to a different class */
			foreach( var c in childItems )
			{
				GameObject.DestroyImmediate(c.gameObject);
			}
			childItems = null;
			
			/** Change the rootcontainer: we need it to be a column for this test */
			rootContainer.direction = FlexDirection.COLUMN;
			
			/** Create what we need: two rows of two items */
			FlexContainer row1Container = rootContainer.AddChildFlexContainerAndFlexItem("row-1", out FlexItem row1Item);
			FlexContainer row2Container = rootContainer.AddChildFlexContainerAndFlexItem("row-2", out FlexItem row2Item);
			FlexItem row1Item1 = row1Container.AddChildFlexItem("item1.1");
			FlexItem row1Item2 = row1Container.AddChildFlexItem("item1.2");
			FlexItem row2Item1 = row2Container.AddChildFlexItem("item2.1");
			FlexItem row2Item2 = row2Container.AddChildFlexItem("item2.2");
			
			/** Disable stretching */
			row1Container.alignItems = row2Container.alignItems = AlignItems.START;
			row1Container.justifyContent = row2Container.justifyContent = FlexJustify.START;
			
			/** Disable wrap, and allow growing/shrinking this time */
			row1Container.wrap = row2Container.wrap = FlexWrap.NOWRAP;
			row1Item.flexGrow = row2Item.flexGrow = 1;
			row1Item.flexShrink = row2Item.flexShrink = 1;
			
			/** Make all children fixed height, so that the rows SHOULD infer a small fixed height from that */
			childItems = new[] {row1Item1, row1Item2, row2Item1, row2Item2};
			float childSetBasis = 100f;
			float childSetCross = 100f;
			foreach( var c in childItems )
			{
				c.flexBasis = new FlexboxBasis(childSetBasis,FlexBasis.LENGTH);
				c.cssDefaultHeight = new CSS3Length(childSetCross, CSS3LengthType.PIXELS);
				c.flexGrow = c.flexShrink = 0; // disable shrinking, because it can mask bugs here
			}

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
			
			/**
			 * Expected:
			 *
			 * 1. all rows have resized (grow) to fill available space
			 * 2. all children have their set heights (no grow/shrink forced on them by a parent)
			 */
			AssertSize( row1Item, HEIGHT, 0, ch/2f, "Row should be 1/N-rows, since it's growing" );
			AssertSize( row2Item, HEIGHT, 0, ch/2f, "Row should be 1/N-rows, since it's growing" );
			
			AssertSize( row1Item1, HEIGHT, 0, childSetCross, "Child should still have its child-height" );
			AssertSize( row1Item2, HEIGHT, 0, childSetCross, "Child should still have its child-height" );
			AssertSize( row2Item1, HEIGHT, 0, childSetCross, "Child should still have its child-height" );
			AssertSize( row2Item2, HEIGHT, 0, childSetCross, "Child should still have its child-height" );
			
		}
		
		[Test]
		public void TestAutoColumnOfAutoRows_EachRowSetsCrossSizeCorrectly_ShrinkAndGrowAllowed_RowsStretchChildren()
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			rootItem.flexGrow = 0;
			
			/** Replace the defaults - TODO: this test should probably move to a different class */
			foreach( var c in childItems )
			{
				GameObject.DestroyImmediate(c.gameObject);
			}
			childItems = null;
			
			/** Change the rootcontainer: we need it to be a column for this test */
			rootContainer.direction = FlexDirection.COLUMN;
			
			/** Create what we need: two rows of two items */
			FlexContainer row1Container = rootContainer.AddChildFlexContainerAndFlexItem("row-1", out FlexItem row1Item);
			FlexContainer row2Container = rootContainer.AddChildFlexContainerAndFlexItem("row-2", out FlexItem row2Item);
			FlexItem row1Item1 = row1Container.AddChildFlexItem("item1.1");
			FlexItem row1Item2 = row1Container.AddChildFlexItem("item1.2");
			FlexItem row2Item1 = row2Container.AddChildFlexItem("item2.1");
			FlexItem row2Item2 = row2Container.AddChildFlexItem("item2.2");
			
			/** Rows should be defaults, but let's be safe */
			row1Container.alignItems = row2Container.alignItems = AlignItems.STRETCH;
			row1Container.justifyContent = row2Container.justifyContent = FlexJustify.START;
			
			/** Disable wrap, and allow growing/shrinking this time */
			row1Container.wrap = row2Container.wrap = FlexWrap.NOWRAP;
			row1Item.flexGrow = row2Item.flexGrow = 1;
			row1Item.flexShrink = row2Item.flexShrink = 1;
			
			/** Make all children fixed height, so that the rows SHOULD infer a small fixed height from that */
			childItems = new[] {row1Item1, row1Item2, row2Item1, row2Item2};
			float childSetBasis = 100f;
			float childSetCross = 100f;
			foreach( var c in childItems )
			{
				c.flexBasis = new FlexboxBasis(childSetBasis,FlexBasis.LENGTH);
				c.cssDefaultHeight = new CSS3Length(childSetCross, CSS3LengthType.PIXELS);
				c.flexGrow = c.flexShrink = 0; // disable shrinking, because it can mask bugs here
			}

			//rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
			
			/**
			 * Expected:
			 *
			 * 1. all rows have resized (grow) to fill available space
			 * 2. all children have their set heights (no grow/shrink forced on them by a parent)
			 */
			AssertSize( row1Item, HEIGHT, 0, ch/2f, "Row should be 1/N-rows, since it's growing" );
			AssertSize( row2Item, HEIGHT, 0, ch/2f, "Row should be 1/N-rows, since it's growing" );
			
			AssertSize( row1Item1, HEIGHT, 0, row1Item.rectTransform.rect.size.y, "Child should be stretched to match row-height" );
			AssertSize(row1Item2, HEIGHT, 0, row1Item.rectTransform.rect.size.y, "Child should be stretched to match row-height");
			AssertSize( row2Item1, HEIGHT, 0, row2Item.rectTransform.rect.size.y, "Child should be stretched to match row-height");
			AssertSize( row2Item2, HEIGHT, 0, row2Item.rectTransform.rect.size.y, "Child should be stretched to match row-height");
			
		}

	}
}