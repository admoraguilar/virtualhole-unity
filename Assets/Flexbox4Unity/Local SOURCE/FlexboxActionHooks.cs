using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEngine;
using UnityEngine.Events;

/**
 * Global class for debugging Flexbox - you can breakpoint these methods in the debugger to keep track of what's
 * going on, or hook them yourself to extend the behaviour of Flexbox globally (instead of having to edit each
 * individual instance/GameObject one at a time)
 */
public class FlexboxActionHooks
{
 private static FlexboxActionHooks _sharedInstance;

 public static FlexboxActionHooks shared
 {
  get
  {
   if( _sharedInstance == null )
   {
    //Debug.Log("Action hook null, creating new");
    _sharedInstance = new FlexboxActionHooks();
   }

   return _sharedInstance;
  }
 }
 
 [Serializable]
 public class FlexContainerEvent : UnityEvent<FlexContainer>
 {
 }
 [Serializable]
 public class FlexItemEvent : UnityEvent<FlexItem>
 {
 }
 [Serializable]
 public class FlexContainerDirectionEvent : UnityEvent<FlexContainer,FlexDirection>
 {
 }
 [Serializable]
 public class FlexContainerJustifyEvent : UnityEvent<FlexContainer,FlexJustify>
 {
 }
 [Serializable]
 public class FlexContainerAlignEvent : UnityEvent<FlexContainer,AlignItems>
 {
 }
 
 public FlexContainerEvent OnContainerCreated = new FlexContainerEvent();
 public FlexItemEvent OnItemCreated = new FlexItemEvent();
 public FlexItemEvent OnGrowSet = new FlexItemEvent();
 public FlexItemEvent OnShrinkSet = new FlexItemEvent();
 public FlexItemEvent OnPaddingSet = new FlexItemEvent();
 public FlexItemEvent OnMarginsSet = new FlexItemEvent();
 public FlexItemEvent OnConstraintsSet = new FlexItemEvent();
 public FlexItemEvent OnDefaultWidthSet = new FlexItemEvent();
 public FlexItemEvent OnDefaultHeightSet = new FlexItemEvent();
 public FlexItemEvent OnOrderSet = new FlexItemEvent();
 public FlexItemEvent OnExpandChildrenToFitSelfSet = new FlexItemEvent();
 
 public FlexContainerDirectionEvent OnDirectionSet = new FlexContainerDirectionEvent();
 public FlexContainerJustifyEvent OnJustifySet = new FlexContainerJustifyEvent();
 public FlexContainerAlignEvent OnAlignSet = new FlexContainerAlignEvent();
}