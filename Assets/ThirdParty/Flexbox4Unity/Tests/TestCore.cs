using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Tests.FlexboxTestHelpers;

namespace Tests
{
    public class TestCore
    {
        [Test]
        public void A_Basics_1FlexCanvas()
        {
            FlexCanvasFullSizeForTesting(out float cw, out float ch, out FlexContainer fCanvas);
            
            Assert.AreEqual( cw, fCanvas.width(), "Should now match the width of the canvas");
            Assert.AreEqual( ch, fCanvas.height(), "Should now match the height of the canvas");
        }
        
        [Test]
        public void A_Basics_2SizeOfNewRootContainer()
        {
            FlexCanvasFullSizeForTesting(out float cw, out float ch, out FlexContainer fCanvas);

            FlexContainer fc = (fCanvas.transform as RectTransform).AddFlexRootContainer();
            Assert.AreNotEqual( cw, fc.width(), "Hasn't expanded yet, shouldn't be full width");
            Assert.AreNotEqual( ch, fc.height(), "Hasn't expanded yet, shouldn't be full height");
            (fc.transform as RectTransform).ExpandToFillParent();
            Assert.AreEqual( cw, fc.width(), "Should now match the width of the canvas");
            Assert.AreEqual( ch, fc.height(), "Should now match the height of the canvas");
        }
    }
}
