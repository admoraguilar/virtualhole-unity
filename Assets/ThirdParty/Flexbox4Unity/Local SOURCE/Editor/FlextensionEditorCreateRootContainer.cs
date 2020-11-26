using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class FlextensionEditorCreateRootContainer
{
	[MenuItem("GameObject/Flexbox/Root Container", false, 1)]
	public static void _Menu_CreateRootFlexContainer(MenuCommand menuCommand)
	{
		GameObject goParent = menuCommand.ContextSensitiveGameObject( new[] {typeof(Canvas), typeof(FlexContainer), typeof(ILayoutElement)} );
		
		RectTransform rt = goParent!=null ? goParent.transform as RectTransform : null;
		FlexContainer newRoot;
		if( rt != null )
			newRoot = rt.AddFlexRootContainer();
		else
			newRoot = FlextensionAddRootContainer.CreateFlexRootContainerWithCanvasEtc();
		
		newRoot.ExpandToFillParent();
		
		Selection.activeGameObject = newRoot.gameObject;
	}
}