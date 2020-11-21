using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEditor;
using UnityEngine.UI;

public static class Flextension3ColumnLayout
{
	[MenuItem("GameObject/Flexbox/Templates/3-Column layout", false, 10)]
	public static void _Menu_Create3Cols(MenuCommand menuCommand)
	{
		GameObject goParent = menuCommand.ContextSensitiveGameObject( new[] {typeof(FlexContainer)} );
		if( goParent == null )
			throw new Exception( "This method has to be run on a Canvas, any UnityUI layoutable item, or FlexContainer - select one in Hierarchy window and try again" );

		if( !goParent.TryGetComponent<FlexContainer>( out FlexContainer parentContainer ) )
		{
			throw new Exception( "This is impossible: 234lksdflkjs23" );
		}
		else
		{
			FlexContainer container = parentContainer.AddChildFlexContainerAndFlexItem( "Flex Container (3 Columns)", out FlexItem discard );

			// Configure
			container.direction = FlexDirection.ROW;
			int numCols = 3;
			for( int i = 0; i < numCols; i++ )
			{
				FlexContainer col = container.AddChildFlexContainerAndFlexItem( "Col-" + (i + 1), out FlexItem colItem );
				col.direction = FlexDirection.COLUMN;
			}
			//Probably only needed for when adding FlexItems - Unity only fails to layout those: container.RelayoutAfterCreatingAtRuntime();

			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo( container.gameObject, "Create " + container.name );
			Selection.activeObject = container.gameObject;
		}
	}	
}