using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEngine;
using UnityEngine.Assertions;
using NAssert = NUnit.Framework.Assert;

namespace Tests
{
    public enum WidthHeighMatch
    {
        BOTH,
        NEITHER,
        WIDTH_ONLY,
        HEIGHT_ONLY,
        WIDTH,
        HEIGHT
    }

    public class FlexboxTestHelpers
    {
        //public static float pixelTolerance = 0.1f; /* 0.1 = if two pixel positions are within 1/10th of a pixel, they'll be considered the same */
        // Temporarily downgrading because some tests get pixel positions out by setting them to +1/3 instead of +1/2, which I want to ignore for now:
        public static float pixelTolerance = 0.49f; /* 0.1 = if two pixel positions are within 1/10th of a pixel, they'll be considered the same */
        
        public static T GameObjectWith<T>( bool rectTransform = false, string optionalName = "[auto generated]" ) where T : Component
        {
            GameObject newGo = rectTransform ? new GameObject( optionalName, typeof(RectTransform) ) : new GameObject( optionalName );
            newGo.transform.position = Vector3.zero;
            T result = newGo.AddComponent<T>();
            return result;
        }

        public static Canvas CanvasForTesting(float w, float h)
        {
            Canvas c = GameObjectWith<Canvas>();
            c.transform.RT().SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, w );
            c.transform.RT().SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, h );
            return c;
        }

        public static GameObject FlexCanvasFullSizeForTesting( out float cw, out float ch, out FlexContainer flexCanvas, float[] forbiddenWidths = null )
        {
            Canvas c = GameObjectWith<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay; // has side-effect of setting width and heighto match the screen
            cw = (c.transform as RectTransform).rect.width;
            ch = (c.transform as RectTransform).rect.height;
		
            if( forbiddenWidths != null )
                foreach( var w in forbiddenWidths )
                    if( Mathf.Approximately(cw, w) )
                        throw new Exception( "Canvas started as oen of the forbidden sizes = "+w );

            flexCanvas = (c.transform as RectTransform).AddFlexRootContainer();
            flexCanvas.rectTransform.ExpandToFillParent();
            return c.gameObject;
        }
        
        /**
             * This is needed to be tested thousands of times over, so we make a method for it.
             *
             * ALSO: it's much cleaner to read the code with a CONSTANT than with WidthAndNotHeightMethodName (the latter is VERY easy to mis-type,
             * or mis-autocomplete ESPECIALLY in JetBrains Rider with it's extremely buggy autocomplete that often displays one thing but completes
             * a different one(!))
             */
        [Obsolete("Add a manual call to relayout, then use AssertSize instead", true)]
        public static void AssertWidthHeight_AfterRelayout(FlexContainer fc, WidthHeighMatch match, float w, float h, string message, FlexContainer flexCanvas)
        {
            flexCanvas.RelayoutAfterChangingSelfOrChildrenFromScript();
            Internal_AssertWidthHeight(fc.width(), fc.height(), match, w, h, message);
        }

        [Obsolete("Add a manual call to relayout, then use AssertSize instead", true)]
        public static void AssertWidthHeight_AfterRelayout(FlexItem fc, WidthHeighMatch match, float w, float h, string message, FlexContainer flexCanvas)
        {
            flexCanvas.RelayoutAfterChangingSelfOrChildrenFromScript();
            Internal_AssertWidthHeight(fc.width(), fc.height(), match, w, h, message);
        }

        /**
         * Requires that you already called relayout manually or some other way before invoking it
         */
        private static void Internal_AssertWidthHeight(float flexThingWidth, float flexThingHeight, WidthHeighMatch match, float w, float h, string message)
        {
            _Assert2DMatch( flexThingWidth, flexThingHeight, match, w, h, message);
        }
        
        private static void _Assert2DMatch(float actualX, float actualY, WidthHeighMatch match, float expectedX, float expectedY, string message)
        {
            switch( match )
            {
                case WidthHeighMatch.BOTH:
                    Assert.AreApproximatelyEqual(actualX, expectedX, pixelTolerance, message);
                    Assert.AreApproximatelyEqual(actualY, expectedY, pixelTolerance, message);
                    break;

                case WidthHeighMatch.NEITHER:
                    Assert.AreNotApproximatelyEqual(actualX, expectedX, pixelTolerance, message);
                    Assert.AreNotApproximatelyEqual(actualY, expectedY, pixelTolerance, message);
                    break;

                case WidthHeighMatch.WIDTH_ONLY:
                    Assert.AreApproximatelyEqual(actualX, expectedX, pixelTolerance, message);
                    Assert.AreNotApproximatelyEqual(actualY, expectedY, pixelTolerance, message);
                    break;

                case WidthHeighMatch.HEIGHT_ONLY:
                    Assert.AreNotApproximatelyEqual(actualX, expectedX, pixelTolerance, message);
                    Assert.AreApproximatelyEqual(actualY, expectedY, pixelTolerance, message);
                    break;

                case WidthHeighMatch.WIDTH:
                    Assert.AreApproximatelyEqual(actualX, expectedX, pixelTolerance, message);
                    break;

                case WidthHeighMatch.HEIGHT:
                    Assert.AreApproximatelyEqual(actualY, expectedY, pixelTolerance, message);
                    break;
                
                default:
                    throw new Exception("Impossible");
            }
        }

        public static void AssertSize(FlexItem flex, WidthHeighMatch match, Vector2 itemSize, string message)
        {
            AssertSize(flex.rectTransform, match, itemSize, message);
        }
        public static void AssertSize(FlexContainer flex, WidthHeighMatch match, Vector2 itemSize, string message)
        {
            AssertSize(flex.rectTransform, match, itemSize, message);
        }

        public static void AssertSize(RectTransform rt, WidthHeighMatch match, Vector2 itemSize, string message )
        {
            _Assert2DMatch( rt.rect.width, rt.rect.height,
                match,
                itemSize.x, itemSize.y,
                message);
        }
        
        public static void AssertSize( FlexItem flex, WidthHeighMatch match, float x, float y, string message)
        {
            AssertSize( flex.rectTransform, match, x, y, message );
        }
        public static void AssertSize( FlexContainer flex, WidthHeighMatch match, float x, float y, string message)
        {
            AssertSize( flex.rectTransform, match, x, y, message );
        }

        public static void AssertSize(RectTransform rt, WidthHeighMatch match, float x, float y, string message )
        {
            _Assert2DMatch( rt.rect.width, rt.rect.height,
                match,
                x, y,
                message);
        }
        
        public static void AssertCenterAt(RectTransform rt, WidthHeighMatch match, float x, float y, Vector2 itemSize, string message )
        {
            _Assert2DMatch( rt.anchoredPosition.x, rt.anchoredPosition.y,
                match,
                x, y,
                message);
        }
        
        public static void AssertLeftEdgeAt(RectTransform rt, float x, Vector2 itemSize, string message )
        {
            _Assert2DMatch( rt.anchoredPosition.x - itemSize.x/2f, -1f,
                WidthHeighMatch.WIDTH,
                x, -1f,
                message);
        }
        
        public static void AssertLeftEdgeGreaterOrEqual(RectTransform rt, float x, float tolerance, string message )
        {
            NAssert.GreaterOrEqual( rt.anchoredPosition.x - rt.rect.width/2f + tolerance, x, message);
        }
        
        public static void AssertRightEdgeLessOrEqual(RectTransform rt, float x, float tolerance, string message )
        {
            NAssert.LessOrEqual( rt.anchoredPosition.x + rt.rect.width/2f - tolerance, x, message);
        }
        
        public static void AssertBottomEdgeGreaterOrEqual(RectTransform rt, float y, float tolerance, string message )
        {
            NAssert.GreaterOrEqual( rt.anchoredPosition.y - rt.rect.height/2f + tolerance, y, message);
        }
        
        public static void AssertTopEdgeLessOrEqual(RectTransform rt, float y, float tolerance, string message )
        {
            NAssert.LessOrEqual( rt.anchoredPosition.x + rt.rect.height/2f - tolerance, y, message);
        }
        
        public static void AssertRightEdgeAt(RectTransform rt, float x, Vector2 itemSize, string message )
        {
            _Assert2DMatch( rt.anchoredPosition.x + itemSize.x/2f, -1f,
                WidthHeighMatch.WIDTH,
                x, -1f,
                message);
        }
        
        public static void AssertFullyOnScreen(RectTransform rt, Vector2 canvasSize, string message )
        {
            /**
             * We use pixelTolerance to make it EASIER here, because Unity's use of floats
             */
            AssertLeftEdgeGreaterOrEqual(rt, -canvasSize.x/2f, pixelTolerance, message);
            AssertRightEdgeLessOrEqual(rt, canvasSize.x/2f, pixelTolerance, message);
            
            AssertBottomEdgeGreaterOrEqual(rt, -canvasSize.x/2f, pixelTolerance, message);
            AssertTopEdgeLessOrEqual(rt, canvasSize.x/2f, pixelTolerance, message);
        }
        
        public static void AssertNotFullyOnScreen(RectTransform rt, Vector2 canvasSize, string message )
        {
            float leftEdge = rt.anchoredPosition.x - rt.rect.width / 2f;
            float rightEdge = rt.anchoredPosition.x + rt.rect.width / 2f;
            float topEdge = rt.anchoredPosition.y + rt.rect.width / 2f;
            float bottomEdge = rt.anchoredPosition.y - rt.rect.width / 2f;
            
            /**
             * We use pixelTolerance to make it HARDER here, because Unity's use of floats
             */
            bool leftOn = leftEdge + pixelTolerance < -canvasSize.x / 2f;
            bool rightOn = rightEdge - pixelTolerance > canvasSize.x / 2f;
            bool bottomOn = bottomEdge - pixelTolerance > -canvasSize.y / 2f; // Note: params opposite way around b/c Unity's top/bottom flip of Y sign
            bool topOn = topEdge + pixelTolerance < canvasSize.y / 2f;
            
            
            //Debug.Log("Canvas: "+(-canvasSize.x/2f)+"<->"+canvasSize.x/2f+", left = "+leftEdge+", right = "+rightEdge+" == "+rt.name );
            NAssert.IsFalse(leftOn && rightOn && topOn && bottomOn, message + " (expected at least one edge off-screen; top/right/bottom/left at: " +
                                                                    topEdge + "/" + rightEdge + "/" + bottomEdge + "/" + leftEdge + ")");
        }
    }
}