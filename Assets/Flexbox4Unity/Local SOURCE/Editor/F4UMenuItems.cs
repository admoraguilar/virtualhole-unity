using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Flexbox4Unity
{
	public class F4UMenuItems : MonoBehaviour
	{
		
#if UNITY_EDITOR
	[MenuItem("GameObject/Flexbox/Flex Item", false, 1)]
	public static void _Menu_CreateFlexItem(MenuCommand menuCommand)
	{
		GameObject goParent = menuCommand.ContextSensitiveGameObject( new[] {typeof(FlexContainer)} );
		if( goParent == null )
			throw new Exception( "This method has to be run on a FlexContainer - first create a RootContainer, or else select any FlexContainer in Hierarchy window and try again" );

		FlexContainer parentContainer = goParent.GetComponent<FlexContainer>();
		FlexItem item = parentContainer.AddChildFlexItem( "Flex Item" );
		
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(item.gameObject, "Create " + item.name);
		Selection.activeObject = item.gameObject;
	}

	[MenuItem("GameObject/Flexbox/Flex Container", false, 1)]
	public static void _Menu_CreateFlexContainer(MenuCommand menuCommand)
	{
		GameObject goParent = menuCommand.ContextSensitiveGameObject( new[] {typeof(FlexContainer)} );
		if( goParent == null )
			throw new Exception( "This method has to be run on a FlexContainer - first create a RootContainer, or else select any FlexContainer in Hierarchy window and try again" );

		FlexContainer parentContainer = goParent.GetComponent<FlexContainer>();
		
		/** When adding to the parent, if the parent was non-null and had a FlexContainer, automatically create a FlexItem too */
		FlexContainer	container = parentContainer.AddChildFlexContainerAndFlexItem( "Flex Container", out FlexItem discard );

		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(container.gameObject, "Create " + container.name);
		Selection.activeObject = container.gameObject;
	}

#if LITE_VERSION
	[MenuItem("GameObject/Flexbox/Templates/Flex Scrollview")]
		public static void _Menu_CreateFlexContainerWithScrollview(MenuCommand menuCommand)
		{
			EditorUtility.DisplayDialog("Automatic Scrollview creation", "Not available in LITE version; please see Help to upgrade, or Window > Flexbox > About", "OK");
		}
	#else
		[MenuItem("GameObject/Flexbox/Templates/Flex Scrollview", false, 111)]
		public static void _Menu_CreateFlexContainerWithScrollview(MenuCommand menuCommand)
		{
			GameObject goParent = menuCommand.ContextSensitiveGameObject( new[] {typeof(Canvas), typeof(ILayoutElement)} );
			if( goParent == null )
				throw new Exception( "This method has to be run on a Canvas, or any UnityUI layoutable item - select one in Hierarchy window and try again" );
			
			GameObject go = new GameObject( "Flex Scroll View" );
			GameObjectUtility.SetParentAndAlign(go, goParent );
			
			GameObject goViewPort = new GameObject("Viewport", typeof(RectTransform) );
			goViewPort.transform.SetParent(go.transform, false );
			GameObject goFlexContentHolder = new GameObject("Content-Holder (Flex)", typeof(RectTransform) );
			goFlexContentHolder.transform.SetParent(goViewPort.transform, false);
			
			GameObject goScrollbar = new GameObject( "Scrollbar", typeof(RectTransform) );
			goScrollbar.transform.SetParent(go.transform, false);
			GameObject goSlidingArea = new GameObject("Sliding Area", typeof(RectTransform) );
			goSlidingArea.transform.SetParent(goScrollbar.transform,false);
			GameObject goHandle = new GameObject("Handle", typeof(RectTransform) );
			goHandle.transform.SetParent(goSlidingArea.transform, false);

			float verticalScrollbarWidth = 24f;
			(goViewPort.transform as RectTransform).anchorMin = Vector2.zero;
			(goViewPort.transform as RectTransform).anchorMax = Vector2.one;
			(goViewPort.transform as RectTransform).offsetMin = /** Unity official: left,bottom */ new Vector2(0, 0);
			(goViewPort.transform as RectTransform).offsetMax = /** Unity official: MINUS right, MINUS top */ -1f * new Vector2(verticalScrollbarWidth, 0);
			(goScrollbar.transform as RectTransform).anchorMin = new Vector2(1f,0f);
			(goScrollbar.transform as RectTransform).anchorMax = new Vector2(1f,1f);
			(goScrollbar.transform as RectTransform).SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, verticalScrollbarWidth );
			(goScrollbar.transform as RectTransform).offsetMin = /** Unity official: left, bottom */ new Vector2(-verticalScrollbarWidth, 0);
			(goScrollbar.transform as RectTransform).offsetMax = /** Unity official: MINUS right, MINUS top */ -1f * new Vector2(0, 0);
			

			(goSlidingArea.transform as RectTransform).anchorMin = Vector2.zero;
			(goSlidingArea.transform as RectTransform).anchorMax = Vector2.one;
			(goSlidingArea.transform as RectTransform).offsetMin = /** Unity official: left, bottom */ new Vector2(10f, 10f);
			(goSlidingArea.transform as RectTransform).offsetMax = /** Unity official: MINUS right, MINUS top */ -1f * new Vector2(10f, 10f);
			
			(goHandle.transform as RectTransform).offsetMin = /** Unity official: left, bottom */ new Vector2(-10f, -10f);
			(goHandle.transform as RectTransform).offsetMax = /** Unity official: MINUS right, MINUS top */ -1f * new Vector2(-10f, -10f);
			
			ScrollRect scrollRect = go.AddComponent<ScrollRect>();
			Image scrollbarImage = goScrollbar.AddComponent<Image>();
			Scrollbar scrollbar = goScrollbar.AddComponent<Scrollbar>();
			Image handleImage = goHandle.AddComponent<Image>();

			Mask viewportMask = goViewPort.AddComponent<Mask>();
			Image viewportMaskImage = goViewPort.AddComponent<Image>();
			FlexContainer viewportFlexContainer = goViewPort.AddComponent<FlexContainer>();

			FlexContainer rootFlexContainer = goFlexContentHolder.AddComponent<FlexContainer>();
			FlexItem contentHolderFlexItem = goFlexContentHolder.AddComponent<FlexItem>();

			scrollbar.interactable = true;
			scrollbar.targetGraphic = handleImage;
			scrollbar.handleRect = (goHandle.transform as RectTransform);
			scrollbar.direction = Scrollbar.Direction.BottomToTop;
			
			scrollbarImage.type = Image.Type.Sliced;
			scrollbarImage.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

			handleImage.type = Image.Type.Sliced;
			handleImage.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

			viewportMaskImage.type = Image.Type.Sliced;
			viewportMaskImage.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
			
			scrollRect.content = goFlexContentHolder.transform as RectTransform;
			scrollRect.vertical = true;
			scrollRect.horizontal = false;
			scrollRect.viewport = goViewPort.transform as RectTransform;
			scrollRect.verticalScrollbar = scrollbar;


			viewportFlexContainer.direction = FlexDirection.COLUMN; // doesn't really matter: it will be constrained to the visible part of scrollview anyway
			viewportFlexContainer.justifyContent = FlexJustify.START;
			viewportFlexContainer.alignItems = AlignItems.STRETCH;

			rootFlexContainer.direction = FlexDirection.COLUMN; // DOES matter: this defines whether the scroll area is vertical (needs vertical-scrollbar) or ...
			rootFlexContainer.justifyContent = FlexJustify.START;
			rootFlexContainer.alignItems = AlignItems.STRETCH;
			
			contentHolderFlexItem.flexBasis = FlexboxBasis.Content; // Critical: required to make the scroll contents expand/contract, causing Unity to update the scrollbar(s)!
			contentHolderFlexItem.flexGrow = 1;
			contentHolderFlexItem.flexShrink = 0; // Critical: required to make the scroll contents overflow the area, so that Unity has something to scroll across!
			
			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
			Selection.activeObject = go;
		}
	#endif
#endif
	}
}