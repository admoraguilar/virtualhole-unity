using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class FlextensionAddRootContainer
{
	/**
	 * Convenience method that auto-creates a Canvas in your project if none exists, then creates the main root flexcontainer
	 * and adds it
	 */
	public static FlexContainer CreateFlexRootContainerWithCanvasEtc()
	{
		// Find or create a Canvas in this project
		Canvas c = GameObject.FindObjectOfType<Canvas>();
		GameObject goCanvas = null;
		if( c == null )
		{
			goCanvas = new GameObject("Canvas");
			c = goCanvas.AddComponent<Canvas>();
			c.renderMode = RenderMode.ScreenSpaceOverlay; // has side-effect of setting width and heighto match the screen
			float cw = (c.transform as RectTransform).rect.width;
			float ch = (c.transform as RectTransform).rect.height;

			var cs = goCanvas.AddComponent<CanvasScaler>();
			cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			cs.scaleFactor = 1;
			cs.referencePixelsPerUnit = 100;

			var rc = goCanvas.AddComponent<GraphicRaycaster>();
			rc.ignoreReversedGraphics = true;
			rc.blockingObjects = GraphicRaycaster.BlockingObjects.None;

			/** Canvas is fundamentally broken without an EventSystem in the Hierarchy,
			 *
			 * but ... Unity bug with ALL VERSIONS of Unity: there is no error or warning given if the ES is missing.
			 *
			 * MUST auto-create one here if we auto-create a Canvas (Unity will ONLY auto-create the ES if it created the Canvas itself too)
			 */
			EventSystem es = GameObject.FindObjectOfType<EventSystem>();
			if( es == null )
			{
				GameObject goEventSystem = new GameObject( "EventSystem" );
				es = goEventSystem.AddComponent<EventSystem>();

				var im = goEventSystem.AddComponent<StandaloneInputModule>();
			}
		}
		else
			goCanvas = c.gameObject;

		/** Create and add the root FlexContainer */
		FlexContainer container = (goCanvas.transform as RectTransform).AddFlexRootContainer( c );
		
		return container;
	}
	
	/**
	 * Can be added anywhere in the GUI hierarchy
	 */
	public static FlexContainer AddFlexRootContainer( this RectTransform rt, Canvas rootCanvas = null )
	{
		if( rootCanvas == null ) 
			rootCanvas = rt.Canvas();

		if( rootCanvas == null )
			return CreateFlexRootContainerWithCanvasEtc();
		else
		{
			/** Create and add the root FlexContainer */
			GameObject goNew = new GameObject("Root FlexContainer", typeof(RectTransform));
			goNew.transform.SetParent(rt.gameObject.transform, false);
			FlexContainer fc = goNew.AddComponent<FlexContainer>();
			fc.settings = Flexbox4UnityProjectSettings.findProjectSettings;
			
			fc.ExpandToFillParent();
			return fc;
		}
	}
}