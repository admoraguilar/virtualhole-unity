#define V3_EXPERIMENTAL_DISABLABLE_RELAYOUT // Temporary feature, may change in future versions.
//#define POST_3_0_CODE

using System;
using UnityEngine;
using IntelligentPluginTools;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
#endif

using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Version = IntelligentPluginVersioning.Version;

namespace Flexbox4Unity
{
 /**
  * This special magic-case version of FlexContainer is required to handle one specific edge-case where UnityUI's core
  * design fails: scrollviews. Unity designed UnityUI so that in all cases your root custom-UI-object should allow UnityUI
  * to size it - except for Scrollviews, where they diverge from that general rule, and implemented their own UI poorly
  * (famously Scrollview doesn't automatically resize inside UnityUI, frustrating everyone who's tried to use them).
  *
  * The workaround is that we have this "magic" FlexContainer subclass that can override all of UnityUI's SIZING rules
  * and algorithm, and size itself by peeking into the Flexbox4Unity hierarchy inside itself and choosing an appropriate
  * size, then deleting the UnityUI size and replacing it with an appropriate size.
  *
  */
#if UNITY_2018_2 || UNITY_2018_1 || UNITY_2017 || UNITY_5 || UNITY_4
[ExecuteInEditMode]
#else // only works in Unity 2018_3 onwards
 [ExecuteAlways]
#endif

 public class RootFlexContainer : FlexContainer /* : UIBehaviour, ILayoutGroup, ILayoutElement */
 {
  private bool _isRespondingToInternalSetSize = false;

  public void SetSizeDontRelayout( Vector2 newSize )
  {
   _isRespondingToInternalSetSize = true;
   rectTransform.SetSize( newSize );
   _isRespondingToInternalSetSize = false;
  }
  public override void SetLayoutHorizontal()
  {
   Debug.Log("SetLayoutHorizontal()" );
   if( !_isRespondingToInternalSetSize ) 
    base.SetLayoutHorizontal();
  }

  protected override void OnTransformChildrenChanged()
  {
   Debug.Log( "OnTransformChildrenChanged()" );
   if( !_isRespondingToInternalSetSize )
   base.OnTransformChildrenChanged();
  }
  
  protected override void OnRectTransformDimensionsChange()
  {
   Debug.Log( "OnRectTransformDimensionsChange()" );
   if( !_isRespondingToInternalSetSize )
    base.OnRectTransformDimensionsChange();
  }
 }
}