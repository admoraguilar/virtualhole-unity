using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
 public struct ElementToRefresh
 {
  public FlexContainer element;
  public RefreshLayoutMode refreshType;
 

  public ElementToRefresh(FlexContainer e)
  {
   element = e;
   refreshType = RefreshLayoutMode.RECURSE_UP_WHOLE_TREE;
  }

  public ElementToRefresh(FlexContainer e, RefreshLayoutMode m)
  {
   element = e;
   refreshType = m;
  }
 }

/**
 * This class was originally created in 2019 to workaround the UnityEngine bug (unfixed for several years)
 * where Unity's internal classes spam the Console with a false warning about "SendMessage",
 * which Unity's tech support has confirmed is an internal Unity bug.
 *
 * Because Unity won't fix/backport their bug, and because they don't allow you to block their Console
 * messages, AND because their console messages cost up to 20ms per message (!), this destroys
 * performance in-editor.
 *
 * The only solution is to create a custom system to sidestep the buggy Unity code and delay
 * all layout calls by a single in-Editor frame. That's what this class does. I feel dirty
 * having to write this, but with 3 separate Unity bugs that they refuse to fix (fixing
 * ANY ONE of their bugs would make this problem vanish), and apparently no intention for them
 * to backport to all affected versions of Unity (2017, 2018, 2019), there's no other way...
 */
 public class GlobalFlexboxLayoutTicker
 {
#if UNITY_EDITOR
  public static bool isTicking = false;
  private static HashSet<ElementToRefresh> queueToRefresh;

  private static readonly object _lockAdding = new object(), _lockQueue = new object();

  public static void AddRefreshOnNextTick(FlexContainer element, RefreshLayoutMode refreshType)
  {
   lock( _lockAdding ) // Ensures that Ticker doesn't get stopped when new AddRefresh is pending
   {
    lock( _lockQueue ) // Ensures that Ticker doesn't iterate while I'm adding to set
    {
     if( queueToRefresh == null )
      queueToRefresh = new HashSet<ElementToRefresh>();

     queueToRefresh.Add(new ElementToRefresh(element, refreshType));
    }

    if( !isTicking )
    {
     isTicking = true;
     EditorApplication.update += Tick;
    }
   }
  }

  #region Settings management since Unity still - in 2020 - has zero support for project-wide settings
  private static Flexbox4UnityProjectSettings _cachedProjectSettings;
  private static Flexbox4UnityProjectSettings _projectSettings
  {
   get
   {
    if( _cachedProjectSettings == null )
     _cachedProjectSettings = EditorProjectSettings.requireProjectSettings;

    return _cachedProjectSettings;
   }
  }
  #endregion
  
  public static void Tick()
  {
   lock( _lockAdding )
   {
    lock( _lockQueue )
    {
     try
     {
      foreach( var item in queueToRefresh )
      {
       if( _projectSettings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithm v2Algorithm )
        switch( item.refreshType )
        {
         case RefreshLayoutMode.RECURSE_UP_WHOLE_TREE:
         case RefreshLayoutMode.SELF_AND_DESCENDENTS_ONLY:
          v2Algorithm.ReLayout( item.element, item.refreshType );
          break;

         // TODO: 2020 / Flexbox4Unity v2.x: remove the need for this special-case mode by integrating more deeply into Unity's internal (private) layout mechanisms
         case RefreshLayoutMode.FORCE_RECURSE_UP_THEN_SELF_DOWN:
          v2Algorithm.ReLayout( item.element, RefreshLayoutMode.RECURSE_UP_WHOLE_TREE );
          v2Algorithm.ReLayout( item.element, RefreshLayoutMode.SELF_AND_DESCENDENTS_ONLY );
          break;
        }
       else if( _projectSettings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithmV3 v3Algorithm )
        v3Algorithm.ReLayout( item.element );

      }
     }
     catch( Exception e )
     {
      Debug.LogWarning("Exception inside Editor-Coroutine; killing coroutine queue and self-recovering. E = "+e );
      queueToRefresh.Clear();
     }

     queueToRefresh.Clear();
    }

    isTicking = false;
    EditorApplication.update -= Tick;
   }
  }
#endif
 }
}