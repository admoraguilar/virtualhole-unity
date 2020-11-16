using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Flexbox4Unity
{
	/**
	 * TODO: Add UnityEditor-safe (i.e. works with Unity's new, incompatible, Undo system) implementations of: CreateFlexContainer, CreateFlexContainerAndFlexItem, CreateFlexItem
	 */
	public static class UnityEditorHelperMethods
	{
		public static GameObject ContextSensitiveGameObject( this MenuCommand command, Type[] parentableSelectionTypes = null )
		{
			GameObject _candidate;
			if( command != null && command.context != null )
				_candidate = (command.context as GameObject);
			else if( Selection.activeGameObject != null )
				_candidate = Selection.activeGameObject;
			else
				_candidate = null;

			if( parentableSelectionTypes == null )
				return _candidate;
			else if( _candidate == null )
				return null;
			else
			{
				foreach( var type in parentableSelectionTypes )
					if( _candidate.GetComponent( type ) != null )
					{
						return _candidate;
					}

				return null;
			}
		}
	}
}