using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Flexbox4Unity
{
	public enum HierarchyObjectType
	{
		NULL,
		TRANSFORM_ONLY,
		RECTTRANSFORM_ONLY,
		FLEXBOX_COMPONENT
	}
	
	public static class UnityGUIHelperMethods
	{
		public static Canvas Canvas(this FlexContainer item)
		{
			return Canvas(item.gameObject);
		}
		public static Canvas Canvas(this FlexItem item)
		{
			return Canvas(item.gameObject);
		}

		public static Canvas Canvas(this RectTransform item)
		{
			return Canvas(item.gameObject);
		}

		private static Canvas Canvas(GameObject go )
		{
			Canvas c = go.GetComponent<Canvas>();
			while( c == null && go.transform.parent != null )
			{
				go = go.transform.parent.gameObject;
				c = go.GetComponent<Canvas>();
			}

			return c;
		}
		
#if ALLOW_FLEXCONTAINERS_WITHOUT_FLEXITEMS_AND_FLEXITEMS_WITHOUT_CONTAINER_PARENTS
		public static FlexItem AddChildFlexItem( this GameObject go, string title )
		{
			GameObject goNew = new GameObject(title, typeof(RectTransform));
			goNew.transform.SetParent(go.transform, false);
			FlexItem fi = goNew.AddComponent<FlexItem>();
			return fi;
		}
		
		public static FlexContainer AddChildFlexContainer( this FlexContainer fc, string title )
		{
			GameObject goNew = new GameObject(title, typeof(RectTransform));
			goNew.transform.SetParent(fc.transform, false);
			FlexContainer fcNew = goNew.AddComponent<FlexContainer>();
			fcNew.settings = fc.settings;
			return fcNew;
		}
    #endif
	
		public static FlexContainer AddChildFlexContainerAndFlexItem( this FlexContainer fc, string title, out FlexItem newFlexItem, UndoBlock2019 undoBlock = null )
		{
			GameObject goNew = new GameObject(title, typeof(RectTransform));
			if( undoBlock != null )
			{
				undoBlock.RegisterCreatedObject(goNew);
				undoBlock.SetTransformParent(goNew.transform, fc.gameObject.transform, false); // NB: false is required, otherwise Unity will mess up your SCALE
				goNew.transform.localPosition = Vector3.zero; // NB: required, because RegisterCreatedObject will put your z at 0, rather than at canvas's zero
				FlexContainer fcNew = undoBlock.AddComponent<FlexContainer>(goNew);
				newFlexItem = undoBlock.AddComponent<FlexItem>(goNew);
				fcNew.settings = fc.settings;
				newFlexItem.settings = fc.settings;
				return fcNew;
			}
			else
			{
				goNew.transform.SetParent( fc.gameObject.transform, false);
				FlexContainer fcNew = goNew.AddComponent<FlexContainer>();
				newFlexItem = goNew.AddComponent<FlexItem>();
				fcNew.settings = fc.settings;
				newFlexItem.settings = fc.settings;
				return fcNew;
			}
		}
	
		public static FlexItem AddChildFlexItem( this FlexContainer fc, string title )
		{
			GameObject goNew = new GameObject(title, typeof(RectTransform));
			goNew.transform.SetParent(fc.transform, false);
			FlexItem fiNew = goNew.AddComponent<FlexItem>();
			fiNew.settings = fc.settings;
			return fiNew;
		}
		
		/**
		 * NB: due to bugs in Unity's design of Destroy, it isn't possible to safely
		 * invoke this method from Editor (you can only do it when you have source-code
		 * access, which isn't guaranteed, and uses Unity's own hacks to workaround the bugs
		 * in Unity.Object).
		 *
		 * TODO: re-implement this using Reflection, to workaround the UnityEditor bug
		 */
		public static void ClearTransform_PlayModeOnly(Transform t)
		{
		
			foreach( Transform child in t )
			{
				//Debug.Log("Will delete: " + child.name);
				Object.Destroy( child.gameObject );
			}
			//Debug.Log("Will detach all from: " + t.name);
			t.DetachChildren();
		}
		
		public static void ExpandToFillParent(this FlexItem item)
		{
			_ExpandToFillParent(item.gameObject);
		}

		public static bool CouldExpandToFillParent(this RectTransform unityUIItem )
		{
			RectTransform thisRect = unityUIItem as RectTransform;
			
			RectTransform parentRect = unityUIItem.parent as RectTransform;

			return (parentRect != null)
			       &&
			       (thisRect.anchorMin != Vector2.zero
			        || thisRect.anchorMax != Vector2.one
			        || thisRect.anchoredPosition != Vector2.zero
			        || thisRect.rect.size != parentRect.rect.size);
		}
		
		public static bool CouldExpandToFillParent(this FlexContainer container)
		{
			RectTransform thisRect = container.transform as RectTransform;
			
			RectTransform parentRect = container.transform.parent as RectTransform;

			return (parentRect != null)
			       &&
			       (thisRect.anchorMin != Vector2.zero
			       || thisRect.anchorMax != Vector2.one
			       || thisRect.anchoredPosition != Vector2.zero
			       || thisRect.rect.size != parentRect.rect.size);
		}
		public static void ExpandToFillParent(this FlexContainer container)
		{
			_ExpandToFillParent(container.gameObject);
		}
		public static void ExpandToFillParent(this RectTransform rt )
		{
			_ExpandToFillParent(rt.gameObject);
		}

		private static void _ExpandToFillParent(GameObject go)
		{
			RectTransform thisRect = go.transform as RectTransform;
			RectTransform parentRect = go.transform.parent as RectTransform;

			if( parentRect != null )
			{
				thisRect.anchorMin = Vector2.zero;
				thisRect.anchorMax = Vector2.one;
				thisRect.anchoredPosition = Vector2.zero;

				thisRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentRect.rect.width);
				thisRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentRect.rect.height);
			}
			else
			{
				Debug.LogWarning("Cannot expand to fill parent - parent is not a Unity GUI object (has no RectTransform), parent = " + go.transform.parent);
			}

		}
	}
}