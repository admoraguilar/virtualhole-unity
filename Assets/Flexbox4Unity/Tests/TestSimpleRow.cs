using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Assert = UnityEngine.Assertions.Assert;
using static Tests.FlexboxTestHelpers;
using static Tests.WidthHeighMatch;

namespace Tests
{
    
    public class TestSimpleRow
    {
        [Test]
        public void FixedLengthRow_1AutoFlexItem()
        {
            FlexCanvasFullSizeForTesting(out float cw, out float ch, out FlexContainer fCanvas);
            //fCanvas.showDebugMessages = true;
            
            FlexContainer rootContainer = fCanvas.AddChildFlexContainerAndFlexItem( "rootContainer", out FlexItem rootItem );
            //rootContainer.showDebugMessages = true;
            
            FlexItem childItem1 = rootContainer.AddChildFlexItem("item1");

            rootContainer.RelayoutAfterCreatingAtRuntime();
            
            Assert.AreEqual( cw, childItem1.width(), "Should now match the width of the canvas");
            Assert.AreEqual( ch, childItem1.height(), "Should now match the height of the canvas");
        }
        
        [Test]
        public void FixedLengthRow_1FixedSizeFlexItem()
        {
            FlexCanvasFullSizeForTesting(out float cw, out float ch, out FlexContainer fCanvas, new[] { 100f } );
            FlexContainer rootContainer = fCanvas.AddChildFlexContainerAndFlexItem( "rootContainer", out FlexItem rootItem );
            rootContainer.RelayoutAfterCreatingAtRuntime(); // required, or else rootContainer never gets re-laid out by its own parent. TODO: add tests that stimulate the auto OnRectTransformChanged etc, and TODO: add a test that changing a child transform forces relayout of parent
            
            FlexItem childItem1 = rootContainer.AddChildFlexItem("item1");
            
            /** Give it a fixed size, but: By default it'll grow to fit anyway */
            childItem1.flexBasis = new FlexboxBasis(100f, FlexBasis.LENGTH);
            rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
            AssertSize( childItem1, BOTH, cw, ch, "Fixed size grows anyway to fill canvas" );
            
            /** Disable grow: should now be fixed size */
            childItem1.flexGrow = 0f;
            rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
            AssertSize(childItem1, HEIGHT_ONLY, cw, ch, "Fixed width should be smaller than canvas" );
            
            /** Switch from Stretch to Start: should now be fixed in BOTH dimensions */
            rootContainer.alignItems = AlignItems.START;
            rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
            AssertSize( childItem1, NEITHER, cw, ch, "Fixed size should be smaller than canvas" );
        }
        
        [Test]
        public void FixedLengthRow_1LayoutContainerWhenChildHasChanged()
        {
            FlexCanvasFullSizeForTesting(out float cw, out float ch, out FlexContainer fCanvas, new float[] { 100f } );
            FlexContainer rootContainer = fCanvas.AddChildFlexContainerAndFlexItem( "rootContainer", out FlexItem rootItem );
            rootContainer.RelayoutAfterCreatingAtRuntime();
            
            AssertSize( rootContainer, WidthHeighMatch.BOTH, cw, ch, "Root container should have filled canvas" );
            
            FlexItem childItem1 = rootContainer.AddChildFlexItem("item1");
            
            /** Give it a fixed size, but: By default it'll grow to fit anyway */
            childItem1.flexBasis = new FlexboxBasis(100f, FlexBasis.LENGTH);
            rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
            AssertSize( childItem1, BOTH, cw, ch, "Fixed size grows anyway to fill canvas" );
            
            /** Disable grow: should now be fixed size */
            childItem1.flexGrow = 0f;
            rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
            AssertSize(childItem1, HEIGHT_ONLY, cw, ch, "Fixed width should be smaller than canvas" );
            
            /** Switch from Stretch to Start: should now be fixed in BOTH dimensions */
            rootContainer.alignItems = AlignItems.START;
            rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
            AssertSize( childItem1, NEITHER, cw, ch, "Fixed size should be smaller than canvas" );
        }

        

    }
}
