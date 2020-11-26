using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Flexbox4Unity
{
	public class F4UMenuItemsHTMLIntegration
	{

#if UNITY_EDITOR
		[MenuItem("GameObject/Flexbox/Flextensions/Output as HTML")]
		public static void _Menu_CreateFlexItem(MenuCommand menuCommand)
		{
			#if UNITY_2019_2_OR_NEWER
			if( menuCommand.context is GameObject && (menuCommand.context as GameObject).TryGetComponent<FlexContainer>( out FlexContainer fc ) )
			#else
				GameObject go = menuCommand.context as GameObject;
				FlexContainer fc = go != null ? go.GetComponent<FlexContainer>() : null;
				if( fc != null )
			#endif
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("<html><body>");
				
				OutputFlexContainerInHTML( fc, sb );
				
				sb.AppendLine("</body></html>");
				Debug.Log("HTML: "+sb);
			}
		}

		private static void OutputFlexContainerInHTML(FlexContainer fc, StringBuilder sb, string indent = "" )
		{
			sb.Append(indent+"<div style=\"");
			
			sb.Append(CSS("display","flex"));
			
			if( fc.wrap == FlexWrap.WRAP )
				sb.Append(CSS("flex-wrap", "wrap"));


			string fdirection = null;
			switch( fc.direction )
			{
				case FlexDirection.ROW:
					fdirection = "row";
					break;
				
				case FlexDirection.COLUMN:
					fdirection = "column";
					break;
				
				case FlexDirection.ROW_REVERSED:
					fdirection = "row-reverse";
					break;
				
				case FlexDirection.COLUMN_REVERSED:
					fdirection = "column-reverse";
					break;
			}
			sb.Append(CSS("flex-direction", fdirection));

#if UNITY_2019_2_OR_NEWER
			if( fc.TryGetComponent<Image>(out Image im) )
#else
			Image im = fc.GetComponent<Image>();
			if( im == null )
			{
				// Check if it has a single child FlexItem with an Image
				/** Can't use GetComponentsInChildren because Unity broke that method and it has never worked as described, sigh */
				int matches = 0;
				foreach( Transform childTransform in fc.transform )
				{
					im = childTransform.GetComponent<Image>();
					if( im != null )
						matches++;
				}

				/** If more than one found, don't use it - we can't know which one it was supposed to be */
				if( matches != 1 )
					im = null;
			}

			if( im != null )
#endif
			{
				sb.Append(CSS("background", im.color));
			}
#if UNITY_2019_2_OR_NEWER
			if( fc.TryGetComponent<FlexItem>(out FlexItem item) )
#else
			FlexItem item = fc.GetComponent<FlexItem>();
			if( item != null )
#endif
			{
				OutputFlexItemStyleInHTML( item, sb, indent);
			}
			else /** Only happens for the root Container! */
			{
				RectTransform rt = fc.transform as RectTransform;

				/** Only for the root Container: width and height controlled by UNITY, not by FLEXBOX */
				sb.Append(CSS("width", rt.rect.width));
				sb.Append(CSS("height", rt.rect.height));
			}

			sb.AppendLine("\">");

			foreach( Transform childTransform in fc.transform )
			{
				/** Everything is guaranteed to be a FlexItem, and MIGHT ALSO be a FlexContainer */
#if UNITY_2019_2_OR_NEWER
				if( childTransform.gameObject.TryGetComponent<FlexItem>( out FlexItem childFlexItem ) )
#else
				FlexItem childFlexItem = childTransform.gameObject.GetComponent<FlexItem>();
				if( childFlexItem != null )
#endif
				{
#if UNITY_2019_2_OR_NEWER
					if( childTransform.TryGetComponent<FlexContainer>( out FlexContainer childContainer ))
#else
					FlexContainer childContainer = childTransform.GetComponent<FlexContainer>();
					if( childContainer != null )
#endif
						OutputFlexContainerInHTML( childContainer, sb, indent+" " );
					else
						OutputFlexItemInHTML(childFlexItem, sb, indent+" ");
				}
			}

			sb.AppendLine(indent+"</div>");
		}

		private static void OutputFlexItemInHTML(FlexItem item, StringBuilder sb, string indent = "")
		{
			sb.Append(indent + "<div style=\"");
#if UNITY_2019_2_OR_NEWER
			if( item.TryGetComponent<Image>(out Image im) )
#else
			Image im = item.GetComponent<Image>();
			if( im != null )
#endif
			{
				sb.Append(CSS("background", im.color));
			}
			OutputFlexItemStyleInHTML(item, sb, indent);
			sb.AppendLine("\">");
			sb.AppendLine(indent + "</div>");
		}

		private static void OutputFlexItemStyleInHTML(FlexItem item, StringBuilder sb, string indent = "")
		{
			sb.Append(CSS("flex-basis", item.flexBasis.ToString()));
			if( item.cssDefaultWidth != CSS3Length.None )
				sb.Append( CSSLengthToCSSString(item.cssDefaultWidth, "width"));
			if( item.cssDefaultHeight != CSS3Length.None )
				sb.Append( CSSLengthToCSSString(item.cssDefaultHeight, "height"));
			if( item.cssMinWidth != CSS3Length.None )
				sb.Append( CSSLengthToCSSString(item.cssMinWidth, "min-width"));
			if( item.cssMinHeight != CSS3Length.None )
				sb.Append( CSSLengthToCSSString(item.cssMinHeight, "min-height"));

			sb.Append( CSS("flex-grow", item.flexGrow) );
			sb.Append( CSS( "flex-shrink",  item.flexShrink) );
			if( item.boxSizing == BoxSizing.BORDER_BOX ) // CONTENT-BOX is default so not needed
				sb.Append( CSS("box-sizing", "border-box" ));

#if UNITY_2019_2_OR_NEWER
			if( item.TryGetComponent<Image>(out Image im) )
#else
			Image im = item.GetComponent<Image>();
			if( im != null )
#endif
			{
				sb.Append(CSS("background", im.color));
			}
		}

		private static string CSS(string name, string value)
		{
			return name + ": " + value + "; ";
		}
		private static string CSS(string name, int value)
		{
			return name + ": " + value + "; ";
		}
		private static string CSS(string name, float value)
		{
			return name + ": " + value + "; ";
		}
		private static string CSS(string name, Color value)
		{
			return name + ": " + "#" + (value.a < 1f ? ColorUtility.ToHtmlStringRGBA(value) : ColorUtility.ToHtmlStringRGB(value)) + "; ";
		}
		

		private static string CSSLengthToCSSString(CSS3Length length, string name)
		{
			switch( length.mode )
			{
				case CSS3LengthType.PIXELS:
					return CSS(name, length.value + "px");
				case CSS3LengthType.PERCENT:
					return CSS(name, length.value + "%");
				case CSS3LengthType.NONE:
					return "";
					
				default:
					throw new Exception("Unsupported CSS3LengthType = "+length.mode);
			}
		}
		#endif
	}
}