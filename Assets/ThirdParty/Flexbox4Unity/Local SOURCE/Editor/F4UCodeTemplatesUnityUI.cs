using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class F4UCodeTemplatesUnityUI
{
	[MenuItem("GameObject/Flexbox/Templates/Item with: Text", false, 111)]
	public static void _Menu_CreateFlexItemWithText(MenuCommand menuCommand)
	{
		GameObject goParent = menuCommand.ContextSensitiveGameObject( new[] {typeof(FlexContainer)} );
		if( goParent == null )
			throw new Exception( "This method has to be run on a FlexContainer - select one in Hierarchy window and try again" );

		FlexContainer parentContainer = goParent.GetComponent<FlexContainer>();
		
		// Configure
		FlexItem item = parentContainer.AddFlexTemplatedText( "Flexed text" );
		parentContainer.RelayoutAfterChangingSelfOrChildrenFromScript();

		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(item.gameObject, "Create " + item.name);
		Selection.activeObject = item.gameObject;
	}

	public static FlexItem AddFlexTemplatedText(this FlexContainer container, string textContent )
	{
		return AddFlexTemplatedText(container, out Text t, textContent, -1, false );
	}
	public static FlexItem AddFlexTemplatedText(this FlexContainer container, out Text newText )
	{
		return AddFlexTemplatedText(container, out newText, "[unassigned]", -1, false );
	}
	public static FlexItem AddFlexTemplatedText(this FlexContainer container,
		string textContent, int pointSize = -1, bool bestFit = false, bool shouldGrow = false )
	{
		return AddFlexTemplatedText(container, out Text t, textContent, pointSize, bestFit, shouldGrow);
	}

	public static FlexItem AddFlexTemplatedText( this FlexContainer container,
		out Text newText, string textContent, int pointSize = -1, bool bestFit = false, bool shouldGrow = false,
		string goName = "[Auto] Text", UndoBlock2019 undoBlock = null )
	{
		FlexItem @return; 
		GameObject goNew = new GameObject( goName, typeof(RectTransform));
		if( undoBlock != null )
		{
			undoBlock.RegisterCreatedObject(goNew);
			undoBlock.SetTransformParent(goNew.transform, container.transform, false); // NB: false is required, otherwise Unity will mess up your SCALE
			goNew.transform.localPosition = Vector3.zero; // NB: required, because RegisterCreatedObject will put your z at 0, rather than at canvas's zero
			@return = undoBlock.AddComponent<FlexItem>(goNew);
		}
		else
		{
			goNew.transform.SetParent( container.transform, false);
			@return = goNew.AddComponent<FlexItem>();
		}

		@return.flexGrow = shouldGrow ? 1 : 0;
		
		newText = goNew.AddComponent<Text>();
		newText.text = textContent;
		newText.resizeTextForBestFit = bestFit;
		if( pointSize > 0 ) newText.fontSize = pointSize;
		
		/** Always do this, to make sure Flexbox is controlling the UI element's size fully, by default */
		(newText.transform as RectTransform).ExpandToFillParent();
		
		return @return;
	}
	
}