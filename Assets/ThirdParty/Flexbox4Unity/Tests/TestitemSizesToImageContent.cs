#define DEBUG_BREAK
using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static Tests.FlexboxTestHelpers;
using static Tests.WidthHeighMatch;
using Assert = UnityEngine.Assertions.Assert;
using NAssert = NUnit.Framework.Assert;
using Object = UnityEngine.Object;

namespace Tests
{
	public class TestitemSizesToImageContent
	{
		private Vector2 iSize;
		private float cw, ch;
		private FlexContainer fCanvas;
		private FlexContainer rootContainer;
		private FlexItem rootItem;
		private FlexItem childItem;

		[SetUp]
		public void PerTestSetup()
		{
			iSize = 100f * Vector2.one;
			FlexCanvasFullSizeForTesting(out cw, out ch, out fCanvas, new[] {iSize.x});
			NAssert.Greater(cw, iSize.x * 3, "Need the canvas to be wide enough to hold three items with space to spare");
			rootContainer = fCanvas.AddChildFlexContainerAndFlexItem("rootContainer", out rootItem);
			rootContainer.RelayoutAfterCreatingAtRuntime(); // required, or else the container itself won't get sized by its parent

			/** Three items, all smaller than the canvas */
			childItem = rootContainer.AddChildFlexItem("item");

			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // causes all the new childitems to be laid-out in one pass
		}

		[TearDown]
		public void PerTestCleanup()
		{
			var guiElements = Object.FindObjectsOfType<RectTransform>();
			foreach( var e in guiElements )
				Object.DestroyImmediate(e.gameObject);
		}



		[Test]
		public void TestAutoSizesUsing_Image_Attached_STRETCH()
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			childItem.flexBasis = FlexboxBasis.Auto;
			childItem.flexGrow = childItem.flexShrink = 0;

			/** add an Image */
			Image im = childItem.gameObject.AddComponent<Image>();
			Texture2D texForSprite = new Texture2D(200, 200);
			Sprite imSprite = Sprite.Create(texForSprite, new Rect(0, 0, texForSprite.width, texForSprite.height), 0.5f * Vector2.one);
			im.sprite = imSprite;

			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** Check that the size is now correct */
			AssertSize(childItem, BOTH, 200f, ch, "Item should be the sprite pixel width, with canvas height");
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, ch, "Item shouldn't be the sprite pixel width any more");
			childItem.flexGrow = 0; // reset it
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
		}
		
		[Test]
		public void TestAutoSizesUsing_Image_Attached_CENTER()
		{
			/** Now without stretch (test both axes are fixed this time) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.CENTER;

			childItem.flexBasis = FlexboxBasis.Auto;
			childItem.flexGrow = childItem.flexShrink = 0;

			/** add an Image */
			Image im = childItem.gameObject.AddComponent<Image>();
			Texture2D texForSprite = new Texture2D(200, 200);
			Sprite imSprite = Sprite.Create(texForSprite, new Rect(0, 0, texForSprite.width, texForSprite.height), 0.5f * Vector2.one);
			im.sprite = imSprite;

			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** Check that the size is now correct */
			AssertSize(childItem, BOTH, 200f, 200f, "Item should be the sprite pixel width AND height" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, 200f, "Item shouldn't be the sprite pixel width any more");
			childItem.flexGrow = 0; // reset it
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
		}

		[Test]
		public void TestAutoSizesUsing_Image_Child_STRETCH()
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			childItem.flexBasis = FlexboxBasis.Auto;
			childItem.flexGrow = childItem.flexShrink = 0;

			/** add an Image ... as CHILD*/
			Image subChildImage = childItem.gameObject.AddGameObjectWith<Image>();
			Texture2D texForSprite = new Texture2D(200, 200);
			Sprite imSprite = Sprite.Create(texForSprite, new Rect(0, 0, texForSprite.width, texForSprite.height), 0.5f * Vector2.one);
			subChildImage.sprite = imSprite;

			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** Check that the size is now correct */
			AssertSize(childItem, BOTH, 200f, ch, "Enclosing outer Item should be the sprite pixel width, with canvas height");
			AssertSize(subChildImage.rectTransform, NEITHER, 200f, ch, "uGUI is very badly written, and Image should be ignoring the sprite size at first (hasn't been forced to expand to fit parent yet)" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, ch, "Enclosing outer Item shouldn't be the sprite pixel width any more");
			AssertSize(subChildImage.rectTransform, NEITHER, 200f, ch, "uGUI is very badly written, and Image should be ignoring the sprite size at first (hasn't been forced to expand to fit parent yet)" );
			childItem.flexGrow = 0; // reset it
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			
			/** Expand the UI child to fit its Flex parent, and check its resizing */
			subChildImage.rectTransform.ExpandToFillParent();
			AssertSize(childItem, BOTH, 200f, ch, "Enclosing outer Item should be the sprite pixel width, with canvas height");
			AssertSize(subChildImage.rectTransform, BOTH, 200f, ch, "Image should be the sprite pixel width, with canvas height" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, ch, "Enclosing outer Item shouldn't be the sprite pixel width any more");
			AssertSize(subChildImage.rectTransform, HEIGHT_ONLY, 200f, ch, "Image shouldn't be using sprite width or height" );
			childItem.flexGrow = 0; // reset it
		}
		
		[Test]
		public void TestAutoSizesUsing_Image_Child_CENTER()
		{
			/** Without stretch (both axes should now be controlled by the Image/sprite size at all times unless GROW is on) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.CENTER;

			childItem.flexBasis = FlexboxBasis.Auto;
			childItem.flexGrow = childItem.flexShrink = 0;

			/** add an Image ... as CHILD*/
			Image subChildImage = childItem.gameObject.AddGameObjectWith<Image>();
			Texture2D texForSprite = new Texture2D(200, 200);
			Sprite imSprite = Sprite.Create(texForSprite, new Rect(0, 0, texForSprite.width, texForSprite.height), 0.5f * Vector2.one);
			subChildImage.sprite = imSprite;

			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** Check that the size is now correct */
			AssertSize(childItem, BOTH, 200f, 200f, "Enclosing outer Item should be the sprite pixel size");
			AssertSize(subChildImage.rectTransform, NEITHER, 200f, ch, "uGUI is very badly written, and Image should be ignoring the sprite size at first (hasn't been forced to expand to fit parent yet)" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, NEITHER, 200f, ch, "Enclosing outer Item shouldn't be the sprite pixel width, nor should it match the canvas height");
			AssertSize(subChildImage.rectTransform, NEITHER, 200f, 200f, "uGUI is very badly written, and Image should be ignoring the sprite size at first (hasn't been forced to expand to fit parent yet)" );
			childItem.flexGrow = 0; // reset it
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			
			/** Expand the UI child to fit its Flex parent, and check its resizing */
			subChildImage.rectTransform.ExpandToFillParent();
			AssertSize(childItem, BOTH, 200f, 200f, "Enclosing outer Item should be the sprite pixel size");
			AssertSize(subChildImage.rectTransform, BOTH, 200f, 200f, "Image should be the sprite pixel size" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, 200f, "Enclosing outer Item shouldn't be the sprite pixel width any more, but still uses cross (sprite height)" );
			AssertSize(subChildImage.rectTransform, HEIGHT_ONLY, 200f, 200f, "Image shouldn't be using sprite width, but still uses cross (sprite height)" );
			childItem.flexGrow = 0; // reset it
		}
		
		/**********************************
		 *
		 *
		 * RAW Image class
		 *
		 * 
		 */

		[Test]
		public void TestAutoSizesUsing_RawImage_Attached_STRETCH()
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			childItem.flexBasis = FlexboxBasis.Auto;
			childItem.flexGrow = childItem.flexShrink = 0;

			/** add an Image */
			RawImage im = childItem.gameObject.AddComponent<RawImage>();
			Texture tex = new Texture2D(200, 200);
			im.texture = tex;

			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** Check that the size is now correct */
			AssertSize(childItem, BOTH, 200f, ch, "Item should be the sprite pixel width, with canvas height");
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, ch, "Item shouldn't be the sprite pixel width any more");
			childItem.flexGrow = 0; // reset it
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
		}
		
		#if DEBUG_BREAK
		[UnityTest]
		public IEnumerator TestAutoSizesUsing_RawImage_Attached_CENTER()
		#else
		[Test]
		public void TestAutoSizesUsing_RawImage_Attached_CENTER()
		#endif
		{
			/** Now without stretch (test both axes are fixed this time) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.CENTER;

			childItem.flexBasis = FlexboxBasis.Auto;
			childItem.flexGrow = childItem.flexShrink = 0;

			/** add an Image */
			RawImage im = childItem.gameObject.AddComponent<RawImage>();
			Texture tex = new Texture2D(200, 200);
			im.texture = tex;

			rootContainer.showDebugMessages = true;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            
			rootContainer.showDebugMessages = false;
			
			#if DEBUG_BREAK
			Debug.Break();
			yield return null;
			#endif
			
			/** Check that the size is now correct */
			AssertSize(childItem, BOTH, 200f, 200f, "Item should be the sprite pixel width AND height" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, 200f, "Item shouldn't be the sprite pixel width any more");
			childItem.flexGrow = 0; // reset it
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
		}

		[Test]
		public void TestAutoSizesUsing_RawImage_Child_STRETCH()
		{
			/** First with stretch (only the length axis will be fixed, cross will be container-cross) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.STRETCH;

			childItem.flexBasis = FlexboxBasis.Auto;
			childItem.flexGrow = childItem.flexShrink = 0;

			/** add an Image ... as CHILD*/
			RawImage subChildImage = childItem.gameObject.AddGameObjectWith<RawImage>();
			Texture tex = new Texture2D(200, 200);
			subChildImage.texture = tex;

			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** Check that the size is now correct */
			AssertSize(childItem, BOTH, 200f, ch, "Enclosing outer Item should be the sprite pixel width, with canvas height");
			AssertSize(subChildImage.rectTransform, NEITHER, 200f, ch, "uGUI is very badly written, and Image should be ignoring the sprite size at first (hasn't been forced to expand to fit parent yet)" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, ch, "Enclosing outer Item shouldn't be the sprite pixel width any more");
			AssertSize(subChildImage.rectTransform, NEITHER, 200f, ch, "uGUI is very badly written, and Image should be ignoring the sprite size at first (hasn't been forced to expand to fit parent yet)" );
			childItem.flexGrow = 0; // reset it
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			
			/** Expand the UI child to fit its Flex parent, and check its resizing */
			subChildImage.rectTransform.ExpandToFillParent();
			AssertSize(childItem, BOTH, 200f, ch, "Enclosing outer Item should be the sprite pixel width, with canvas height");
			AssertSize(subChildImage.rectTransform, BOTH, 200f, ch, "Image should be the sprite pixel width, with canvas height" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, ch, "Enclosing outer Item shouldn't be the sprite pixel width any more");
			AssertSize(subChildImage.rectTransform, HEIGHT_ONLY, 200f, ch, "Image shouldn't be using sprite width or height" );
			childItem.flexGrow = 0; // reset it
		}
		
		[Test]
		public void TestAutoSizesUsing_RawImage_Child_CENTER()
		{
			/** Without stretch (both axes should now be controlled by the Image/sprite size at all times unless GROW is on) */
			rootContainer.justifyContent = FlexJustify.START;
			rootContainer.alignItems = AlignItems.CENTER;

			childItem.flexBasis = FlexboxBasis.Auto;
			childItem.flexGrow = childItem.flexShrink = 0;

			/** add an Image ... as CHILD*/
			RawImage subChildImage = childItem.gameObject.AddGameObjectWith<RawImage>();
			Texture tex = new Texture2D(200, 200);
			subChildImage.texture = tex;

			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

			/** Check that the size is now correct */
			AssertSize(childItem, BOTH, 200f, 200f, "Enclosing outer Item should be the sprite pixel size");
			AssertSize(subChildImage.rectTransform, NEITHER, 200f, ch, "uGUI is very badly written, and Image should be ignoring the sprite size at first (hasn't been forced to expand to fit parent yet)" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, NEITHER, 200f, ch, "Enclosing outer Item shouldn't be the sprite pixel width, nor should it match the canvas height");
			AssertSize(subChildImage.rectTransform, NEITHER, 200f, 200f, "uGUI is very badly written, and Image should be ignoring the sprite size at first (hasn't been forced to expand to fit parent yet)" );
			childItem.flexGrow = 0; // reset it
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			
			/** Expand the UI child to fit its Flex parent, and check its resizing */
			subChildImage.rectTransform.ExpandToFillParent();
			AssertSize(childItem, BOTH, 200f, 200f, "Enclosing outer Item should be the sprite pixel size");
			AssertSize(subChildImage.rectTransform, BOTH, 200f, 200f, "Image should be the sprite pixel size" );
			/** ... but is it resizing? Lets briefly GROW the flex and see if the child-Image object resized too */
			childItem.flexGrow = 1;
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript();
			AssertSize(childItem, HEIGHT_ONLY, 200f, 200f, "Enclosing outer Item shouldn't be the sprite pixel width any more, but still uses cross (sprite height)" );
			AssertSize(subChildImage.rectTransform, HEIGHT_ONLY, 200f, 200f, "Image shouldn't be using sprite width, but still uses cross (sprite height)" );
			childItem.flexGrow = 0; // reset it
		}
	}
}