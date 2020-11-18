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
 * Flexbox implementation as per CSS3 - this works MANY TIMES better than Unity's HorizontalLayoutGroup, but
 * that's not a surprise: the CSS standard has had many more engineers working on it than Unity's proprietary
 * layout system did :).
 *
 * Note: at runtime, we CANNOT detect when you change the settings of a FlexContainer - Unity refuses to provide
 * a callback for this (They only provide one inside the editor), so that when you make changes to a FlexContainer
 * from a script, at runtime/playmode, you may me need to also call this method to cause them to take effect:
 *
 *    RelayoutAfterMakingChangesFromScript()
 *
 * ...but in many cases, other changes you make at the same time will schedule a relayout anyway, and your newly-added
 * component will automatically get detected during that relayout.
 * 
 * Primary documentation resources:
 *  * https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Flexible_Box_Layout/Basic_Concepts_of_Flexbox
 *  * https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Flexible_Box_Layout/Controlling_Ratios_of_Flex_Items_Along_the_Main_Ax
 *  * Official CSS (very detailed, very technical): https://www.w3.org/TR/css-flexbox-1/#layout-algorithm
 *  * https://css-tricks.com/snippets/css/a-guide-to-flexbox/
 *
 * Current notes:
 *  - when the container is too small in cross-direction to fit the FIXED basis sizes of some elements, those elements
 *     WILL OVERFLOW (this is correct! Confirmed with CSS3). To prevent overflow, CSS has nothing? (TBC/TODO); you can use ASPECT basis
 *     in some cases.
 */
#if UNITY_2018_2 || UNITY_2018_1 || UNITY_2017 || UNITY_5 || UNITY_4
[ExecuteInEditMode]
#else // only works in Unity 2018_3 onwards
 [ExecuteAlways]
#endif

 public class FlexContainer : UIBehaviour, ILayoutGroup, ILayoutElement
 {
#if V3_EXPERIMENTAL_DISABLABLE_RELAYOUT
  protected static bool _suppressingAutoLayout = false;

  public static void DisableAutomaticRelayout( bool suppressWarning = false )
  {
   if( !suppressWarning )
    Debug.LogWarning( "Disabling auto-relayout; you should re-enable this ASAP, otherwise Flexbox will stop working properly" );
   _suppressingAutoLayout = true;
  }

  public static void EnableAutomaticRelayout()
  {
   _suppressingAutoLayout = false;
  }
#endif

  public FlexDirection direction;
  public FlexJustify justifyContent = FlexJustify.START;
#if LITE_VERSION
public FlexWrap wrap { get { return FlexWrap.NOWRAP; } }
#else
  public FlexWrap wrap = FlexWrap.NOWRAP;
#endif
  public AlignItems alignItems = AlignItems.STRETCH;

#if LITE_VERSION
  public bool EXPERIMENTAL_constrainContentToSelf { get { return false; } }
#else
  /**
   * This mainly applies to the outermost FlexContainer in your UI (the one that integrates your UI with the UnityUI layout
   * system, and is not controlled by Flexbox).
   *
   * The main purpose of this flag is to allow you to prevent content from expanding outside your rootContainer. This
   * is often desirable on fixed-size, full-screen UIs, where you want everything to "shrink-to-fit the screen", rather than the
   * default in Flexbox, which is "expand/shrink to the preferred content-size of each item".
   *
   * THIS IS AN EXPERIMENTAL FEATURE, SINCE IT SLIGHTLY DIVERGES FROM THE FLEXBOX/CSS SPECIFICATION: THIS MAY BE REMOVED
   * IN A FUTURE RELEASE, OR REPLACED WITH A DIFFERENT IMPLEMENTATION THAT FITS BETTER WITHIN THE CSS SPEC.
   * 
   * If you place the container inside a UnityUI Scrollview, you definitely want to turn this off! (the unconstrained
   * expansion is what makes ScrollView work properly - and work better in Flexbox than it does in Unity's own UI system!)
   */
  public bool EXPERIMENTAL_constrainContentToSelf = false;
#endif

  public Version createdWithVersion = new Version( 0, 0, 0 ); // defaults to non-existent version for old code that pre-dates the versioning plugin 
  [FormerlySerializedAs( "lastEditedWithVersion" )] public Version upgradedToVersion = new Version( 0, 0, 0 ); // defaults to non-existent version for old code that pre-dates the versioning plugin

  #region Advanced bonus settings

  public bool showDebugMessages = false;
  public bool RelayoutWheneverSelected = false;

  #endregion

  #region UnityUI callbacks that we ignore - our algorithm is much faster than the UnityUI one

  public void CalculateLayoutInputHorizontal()
  {
  }

  public void CalculateLayoutInputVertical()
  {
  }

  public float minWidth { get { return 0f; } }

  public float preferredWidth { get { return 0f; } }

  public float flexibleWidth { get { return 0f; } }

  public float minHeight { get { return 0f; } }

  public float preferredHeight { get { return 0f; } }

  public float flexibleHeight { get { return 0f; } }

  public int layoutPriority { get { return 0; } }

  #endregion

  #region Settings management

  [SerializeField] private Flexbox4UnityProjectSettings _settings;
  public Flexbox4UnityProjectSettings settings
  {
   get
   {
    if( _settings == null )
    {
     if( Application.isEditor )
      if( Application.isPlaying )
       throw new Exception( "(in-Editor): Please run the \"Tools > Flexbox > Auto-discover all RuntimeSettings\" tool! Flexbox settings missing at runtime, should have been pre-configured before entering playmode, for object = " + this );
      else
      {
       _settings = Flexbox4UnityProjectSettings.findProjectSettings;
       
       #if UNITY_EDITOR
       EditorSceneManager.MarkSceneDirty( PrefabStageUtility.GetCurrentPrefabStage() != null ? PrefabStageUtility.GetCurrentPrefabStage().scene : SceneManager.GetActiveScene() );
      #endif
      }
     else // is RUNTIME with no editor
      _settings = Flexbox4UnityProjectSettings.findProjectSettings;
    }

    return _settings;
   }
   
   set { _settings = value; }
  }

  public bool hasSettings { get { return _settings != null; } }

  public void _InternalWipeSettings()
  {
   _settings = null;
  }
 #endregion

#if UNITY_EDITOR
#else
  /**
   * Ideally, this would be enough. However (bug reported to Unity), all versions of Unity currently invoke
   * OnRectTransformDidChange *before* calling Awake(), thereby violating Unity's own contract for Awake (not the
   * first time Unity engineers have done this :().
   *
   * For most situations, this is sufficient. But we also have to additionally do this check inside OnRectTransformDidChange
   */
  protected override void Awake()
  {
   /** Detect runtime load and a missing global FlexboxSettings */
   if( settings == null )
   {
    throw new Exception("Runtime missing FlexboxSettings: this FlexContainer was packaged into the build without a settings object: "+name);
   }
  }
#endif

  /************************************************************
  *
  * This is nasty.
  *
  * One of Unity's programmers made a mistake a few years ago, and it seems to have broken Unity's
  * build-system if you use Reset() or OnValidate() on any UIBehaviour subclass.
  *
  * (The UnityEditor DLL and the UnityEngine DLL have *incompatible* definitions of those methods - which should not
  * be possible!)
  *
  * The workaround we use for now:
  *   1. For source builds, we default to doing it correctly (IF UNITY_EDITOR will always be true)
  *   2. For source builds, when Unity builds the App/Game/Player, we do a HACK that tricks Unity into building, to workaround
  *        Unity's own HACK where they messed-up their own DLL.
  *   3. DLL builds of the project (e.g. the free LITE version) are incapable of processing
  * Reset() or OnValidate() properly. THIS IS A BUG CAUSED BY UNITY and as far as I can tell it is impossible to fix/workaround
  * until Unity fixes their internal source code!
  ************************************************************/
#if UNITY_EDITOR
  protected override void Reset()
#else
  protected void Reset()
#endif
  {
   var vnow = Flexbox4UnityProjectSettings.builtVersion;
   Debug.Log("New component added, setting createdWithVersion & upgadedToVersion = "+vnow);
   
   upgradedToVersion = vnow;
   if( createdWithVersion < new Version( 1, 0, 0 ) )
   {
    createdWithVersion = vnow;

    RelayoutAfterCreatingAtRuntime();
   }
   
   //Debug.Log("Container created, sending callback" );
   FlexboxActionHooks.shared.OnContainerCreated.Invoke( this );
  }

  /**
   * Any changes you make after newly-adding a component will require a new layout - but
   * we can only detect that in the Editor
   */
  public void RelayoutAfterChangingSelfOrChildrenFromScript()
  {
   if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithm v2Algorithm )
    // Trigger a single layout call, to make sure it gets laid-out the first time
    // ... note: we only do self+descendents; if our size changes as a side-effect, the parent will auto layout anyway
    v2Algorithm.ReLayout( this, RefreshLayoutMode.SELF_AND_DESCENDENTS_ONLY );
   else if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithmV3 v3Algorithm )
    v3Algorithm.ReLayout( this );
  }

  /**
   * Only to be used when you are ABSOLUTELY sure that your changes have not changed the size or position of this
   * container within its parent - in that specific case, this increases performance by skipping the recurse up
   * the tree and doing the "safe" relayout (which checks what other things have changed before deciding what to
   * layout)
   *
   * TODO: in 2020/2021 implement the proposed internal layout-cache so that this method is never any faster than the built-in automatically called relayout methods
   */
  public void RelayoutMinimal()
  {
   settings.v3layoutAlgorithm.ReLayoutChildrenOnly( this );
  }

  /**
   * Unity refuses to give us a callback when objects are added at runtime, so although we can auto-detect this in
   * Editor ... for runtime changes, you'll need to call this manually
   */
  public void RelayoutAfterCreatingAtRuntime()
  {
   if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithm v2Algorithm )
    // Trigger a single layout call, to make sure it gets laid-out the first time
    // Note: we go all the way UP the tree to the top, because we could have affected everything!
    settings.v2layoutAlgorithm.ReLayout( this, RefreshLayoutMode.RECURSE_UP_WHOLE_TREE );
   else if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithmV3 v3Algorithm )
   {
    /**
     * The one time the v3 relayout algorithm fails is here, if the size our parent eventually makes us is the same
     * as the size we (coincidentally) were when we were created/added to an MB.
     */
    try
    {
     Vector2 initialSize = rectTransform.rect.size;
     settings.v3layoutAlgorithm.ReLayout(this);
     Vector2 postLayoutSize = rectTransform.rect.size;
     if( Vector2.Distance(initialSize, postLayoutSize) < 1f )
      settings.v3layoutAlgorithm.ReLayoutChildrenOnly(this);
    }
    catch( Exception e )
    {
     Debug.LogWarning("Flexbox: Exception trying to call Relayout after creation, probably you constructed an invalid UnityUI object in script?. Exception = "+e);
    }
   }
  }
  
  /************************************************************
  *
  * This is nasty.
  *
  * One of Unity's programmers made a mistake a few years ago, and it seems to have broken Unity's
  * build-system if you use Reset() or OnValidate() on any UIBehaviour subclass.
  *
  * (The UnityEditor DLL and the UnityEngine DLL have *incompatible* definitions of those methods - which should not
  * be possible!)
  *
  * The workaround we use for now:
  *   1. For source builds, we default to doing it correctly (IF UNITY_EDITOR will always be true)
  *   2. For source builds, when Unity builds the App/Game/Player, we do a HACK that tricks Unity into building, to workaround
  *        Unity's own HACK where they messed-up their own DLL.
  ************************************************************/
#if UNITY_EDITOR
  protected override void OnEnable()
#else
  public void OnEnable()
#endif
  {
#if V3_EXPERIMENTAL_DISABLABLE_RELAYOUT
   if( _suppressingAutoLayout )
    return;
#endif
   /** When it's made visible on screen, Unity will NOT automatically fire a layout event (unlike all other
    * windowing/layout systems since forever). So we need to explicitly demand one.
    */
   if( _settings != null ) 
    RelayoutAfterCreatingAtRuntime();
   else
    Debug.LogWarning( "Unity bug: Unity calls OnEnable inside AddComponent, before it's possible to configure the component. So we have to ignore this OnEnable call because we have no settings object yet." );
  }

  #region Improve Unity's weak design of base MonoBehaviour/Component/Behaviour

  public RectTransform rectTransform
  {
   get
   {
    if( transform is RectTransform rt )
     return rt;
    else
    {
     throw new Exception( "This FlexContainer is attached to a corrupt GameObject that does not have a RectTransform. Due to bugs in Unity (all versions), you must delete this GameObject and re-create it as a UnityUI GameObject: " + hierarchicalName );
    }
   }
  }

  public string hierarchicalName
  {
   get
   {
    string hName = name;

    var parent = transform.parent;
    while( parent != null )
    {
     hName = parent.name + "." + hName;
     parent = parent.parent;
    }

    return hName;
   }
  }

#if POST_3_0_CODE
  public string hierarchicalNameShort( int maxCharsPerSegment = 15, int maxTotalChars = 25 ) 
  {
   List<string> segments = new List<string>();
   
   segments.Add( name );

   int charsUsed = 0;
   
    var parent = transform.parent;
    while( parent != null )
    {
     string nextSegment = parent.name;
     if( maxCharsPerSegment > 0 && nextSegment.Length > maxCharsPerSegment )
     {
      string infix = "...";
      int infixEffectiveLength = 1;
      int maxChars = maxCharsPerSegment - infixEffectiveLength;
      nextSegment = nextSegment.Substring(0, maxChars / 2)
                    + infix
                    + nextSegment.Substring(nextSegment.Length - maxChars / 2);
      charsUsed += maxCharsPerSegment;
     }
     else
      charsUsed += nextSegment.Length; // only do this directly if NOT infixing, otherwise infix's variable length could make us overcount it     

     segments.Insert( 0, nextSegment );
     parent = parent.parent;
    }

    if( charsUsed > maxTotalChars )
    {
     int charsAvailable = maxTotalChars;
     // work out how much we need for showing "..." for each segment, i.e. the max we could compress them
     int charsUsedMinPerSegment = "..".Length;
     charsAvailable -= Math.Max(0,segments.Count - 2) * charsUsedMinPerSegment;

     string firstSegEnding = "..";
     string firstSeg = segments[0];
     string lastSeg = segments[segments.Count - 1];
     if( charsAvailable < firstSeg.Length + lastSeg.Length )
     {
      // preserve the last item as much as possible
      int firstSegCharsAvailable = Math.Max(0, charsAvailable - lastSeg.Length);
      segments[0] = firstSeg.Substring(0, Math.Max( 0, firstSegCharsAvailable-firstSegEnding.Length) ) + firstSegEnding;
      segments[segments.Count - 1] = charsAvailable >= lastSeg.Length
      ? segments[segments.Count - 1]
      : ".. " + lastSeg.Substring( Math.Min( lastSeg.Length, (lastSeg.Length - (charsAvailable - firstSegCharsAvailable - ".. ".Length)) ) );
     }
     
     charsAvailable -= segments[0].Length + segments[segments.Count - 1].Length;

     int charsAllowedPerSegment = Math.Max(0, charsAvailable / (segments.Count - 2) - "..".Length );
     for( int i = 1; i < segments.Count - 1; i++ )
     {
      segments[i] = segments[i].Length < charsAllowedPerSegment
      ? segments[i]
      : ".." + segments[i].Substring( segments[i].Length - ( charsAllowedPerSegment ) );
     }
    }

    return string.Join(" > ",segments);
  }
#endif

  #endregion

  //void ILayoutController.SetLayoutHorizontal()
  public virtual void SetLayoutHorizontal()
  {
#if V3_EXPERIMENTAL_DISABLABLE_RELAYOUT
   if( _suppressingAutoLayout )
    return;
#endif
   if( showDebugMessages
       || settings.debugRefreshTriggers ) Debug.Log( "[" + hierarchicalName + "] START: Unity triggered a re-layout (h)" );

   if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithm v2Algorithm )
   {
    if( settings.debugRefreshTriggers ) Debug.Log( "Unity demanded a SetLayout-H on: " + hierarchicalName );
    settings.v2layoutAlgorithm.Layout( this );
   }
   else if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithmV3 v3Algorithm )
   {
    if( settings.debugRefreshTriggers ) Debug.Log( "Unity demanded a SetLayout-H on: " + hierarchicalName );
    settings.v3layoutAlgorithm.ReLayoutChildrenOnly( this ); // Note: Unity invokes this method on EVERY descendent, so we must avoid recursing up the tree
   }
  }

  #region React to Unity's changes by triggering a relayout

  public bool experimental_DontUseLayoutRebuilder_WhenChildrenAddedRemoved = false;

  protected virtual void OnTransformChildrenChanged()
  {
#if V3_EXPERIMENTAL_DISABLABLE_RELAYOUT
   if( _suppressingAutoLayout )
    return;
#endif
   //if( showDebugMessages ) Debug.Log( "["+name+"] Ignoring: MonoBehaviour.OnTransformChildrenChanged()" );
   if( showDebugMessages
       || settings.debugRefreshTriggers ) Debug.Log( "[" + hierarchicalName + "] MonoBehaviour.OnTransformChildrenChanged()" );

   if( experimental_DontUseLayoutRebuilder_WhenChildrenAddedRemoved )
    RelayoutBecauseChildAddedOrRemoved();
   else
   {
    //Doesn't work, Unity ignores it: RefreshLayout( RefreshLayoutMode.RECURSE_UP_WHOLE_TREE );
    LayoutRebuilder.ForceRebuildLayoutImmediate( transform as RectTransform );
   }
  }
  /*
  /// <summary>
  ///   <para>This callback is called if an associated RectTransform has its dimensions changed.</para>
  /// </summary>
  protected virtual void OnRectTransformDimensionsChange()
  {
  }

  /// <summary>
  ///   <para>See MonoBehaviour.OnBeforeTransformParentChanged.</para>
  /// </summary>
  protected virtual void OnBeforeTransformParentChanged()
  {
  }

  /// <summary>
  ///   <para>See MonoBehaviour.OnRectTransformParentChanged.</para>
  /// </summary>
  protected virtual void OnTransformParentChanged()
  {
  }

  /// <summary>
  ///   <para>See UI.LayoutGroup.OnDidApplyAnimationProperties.</para>
  /// </summary>
  protected virtual void OnDidApplyAnimationProperties()
  {
  }

  /// <summary>
  ///   <para>See MonoBehaviour.OnCanvasGroupChanged.</para>
  /// </summary>
  protected virtual void OnCanvasGroupChanged()
  {
  }

  /// <summary>
  ///   <para>Called when the state of the parent Canvas is changed.</para>
  /// </summary>
  protected virtual void OnCanvasHierarchyChanged()
  {
  }
  */

  /**
   * Proprietary method that we had to invent because Unity hasn't got one...
   */
  public void OnRectTransformPositionChange()
  {
#if V3_EXPERIMENTAL_DISABLABLE_RELAYOUT
   if( _suppressingAutoLayout )
    return;
#endif
   RelayoutBecauseRectTransformChanged();
  }

  public void OnChildRectTransformPositionChange( RectTransform childTransform )
  {
#if V3_EXPERIMENTAL_DISABLABLE_RELAYOUT
   if( _suppressingAutoLayout )
    return;
#endif
   if( settings.isLayoutInProgress )
   {
    // do nothing.
   }
   else
   {
    if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithm v2Algorithm )
    {
     /**
       * no layout in progress, so the user has probably tried to move a child RectTransform, which we want to actively prevent
       *
       *
       * ...but we can probably get away with only layout ourself + children (child being "moved" should by definition NOT
       * change whatever flex layout already existed, since Flexbox doesn't allow manual moving of children)
       */
     settings.v2layoutAlgorithm.ReLayout( this, RefreshLayoutMode.SELF_AND_DESCENDENTS_ONLY );
    }
    else if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithmV3 v3Algorithm )
     settings.v3layoutAlgorithm.ReLayout( this );
   }
  }

  protected override void OnRectTransformDimensionsChange()
  {
#if V3_EXPERIMENTAL_DISABLABLE_RELAYOUT
   if( _suppressingAutoLayout )
    return;
#endif
   //if( showDebugMessages || _settings.debugRefreshTriggers ) Debug.Log("[" + hierarchicalName + "]: OnRectTransformDimensionsChange()");
   RelayoutBecauseRectTransformChanged();
  }

  private void RelayoutBecauseRectTransformChanged()
  {
#if V3_EXPERIMENTAL_DISABLABLE_RELAYOUT
   if( _suppressingAutoLayout )
    return;
#endif

   if( showDebugMessages
       || settings.debugRefreshTriggers ) Debug.Log( "[" + hierarchicalName + "]: relayout because my RectTransform changed" );
/*#if UNITY_2018_2 || UNITY_2018_1 || UNITY_2017 || UNITY_5 || UNITY_4
#else
  Debug.Log("Application.isPlaying?"+Application.IsPlaying(gameObject));
  if( Application.IsPlaying( gameObject ) ) // Unity 2018.3.0+ requires this extra call because ExecuteAlways/ExecuteInEditMode breaks Unity's new nested-prefabs implementation
#endif*/
   {

    if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithm v2Algorithm )
    {
     if( settings.isLayoutInProgress ) // in progress layouts have side-effect of triggering this whole method, once for each resized child and grandchild/etc!
     {
      if( showDebugMessages
          || settings.debugRefreshTriggers ) Debug.Log( "[" + hierarchicalName + "] request to refresh-layout from inside OnRectTransformDimensionsChange. Layout already in progress, so: self+descendents only." );
      /** Layout is in progress; that means our parent has just set a Final Size on us,
       * and we should respond by refreshing our SELF AND CHILDREN ONLY, sizes + positions.
       */
      settings.v2layoutAlgorithm.ReLayout( this, RefreshLayoutMode.SELF_AND_DESCENDENTS_ONLY );
     }
     else
      // Instead of this, weuse our custom layout algorithm which is up to 1000x faster than Unity's built-in algorithm:LayoutRebuilder.MarkLayoutForRebuild( transform as RectTransform );
      settings.v2layoutAlgorithm.ReLayout( this, RefreshLayoutMode.RECURSE_UP_WHOLE_TREE );
    }
    else if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithmV3 v3Algorithm )
    {
     /**
       * Special case: because recttransform changed, we need to react specially even with v3
       */
     if( settings.isLayoutInProgress ) // setsize was AUTHORITATIVE by the already-running alg, so trust it, and only do internal relayout
      settings.v3layoutAlgorithm.ReLayoutChildrenOnly( this );
     else // setsize was RANDOM AND UNTRUSTABLE by the user, by the UnityEditor - who knows? We don't care: trigger an automatic re-layout up the tree
      settings.v3layoutAlgorithm.ReLayout( this );
    }

   }
  }

  private void RelayoutBecauseChildAddedOrRemoved()
  {
#if V3_EXPERIMENTAL_DISABLABLE_RELAYOUT
   if( _suppressingAutoLayout )
    return;
#endif

   if( showDebugMessages
       || settings.debugRefreshTriggers ) Debug.Log( "[" + hierarchicalName + "]: relayout because my RectTransform changed" );
/*#if UNITY_2018_2 || UNITY_2018_1 || UNITY_2017 || UNITY_5 || UNITY_4
#else
  Debug.Log("Application.isPlaying?"+Application.IsPlaying(gameObject));
  if( Application.IsPlaying( gameObject ) ) // Unity 2018.3.0+ requires this extra call because ExecuteAlways/ExecuteInEditMode breaks Unity's new nested-prefabs implementation
#endif*/
   {
    if( settings.isLayoutInProgress ) // in progress layouts have side-effect of triggering this whole method, once for each resized child and grandchild/etc!
    {
     // ignore it
    }
    else
    {
     if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithm v2Algorithm )
      // Instead of this, we use our custom layout algorithm which is up to 1000x faster than Unity's built-in algorithm:LayoutRebuilder.MarkLayoutForRebuild( transform as RectTransform );
      settings.v2layoutAlgorithm.ReLayout( this, RefreshLayoutMode.RECURSE_UP_WHOLE_TREE );
     else if( settings.currentLayoutAlgorithm is IFlexboxLayoutAlgorithmV3 v3Algorithm )
      settings.v3layoutAlgorithm.ReLayout( this );
    }
   }
  }

  #endregion


  public HierarchyObjectType parentObjectType
  {
   get
   {
    if( transform.parent == null )
     return HierarchyObjectType.NULL;

    GameObject goParent = transform.parent.gameObject;
    if( goParent.GetComponent<FlexItem>() != null
        || goParent.GetComponent<FlexContainer>() != null )
     return HierarchyObjectType.FLEXBOX_COMPONENT;
    else if( goParent.transform is RectTransform )
     return HierarchyObjectType.RECTTRANSFORM_ONLY;
    else
     return HierarchyObjectType.TRANSFORM_ONLY;
   }
  }

  private void OnDrawGizmosSelected()
  {
   if( settings.drawFlexHierarchyUsing != GizmosRenderMode.NONE
#if UNITY_EDITOR // Unity engineers made this an editor-only call, even though it is commonly *required* in NON-EDITOR classes (because OnDrawGizmos will ONLY work in non-editor classes).
       && Selection.Contains( gameObject )
#endif
       && UnityGUIHelperMethods.Canvas( this ) != null
   )
#if POST_3_0_CODE
   DrawAsWireframe( DrawWireframe3, 1f, 0f, 0);
#else
    DrawAsWireframe( DrawWireframe, settings.flexHierarchyGizmosInsetAmount, 0f, 0 );
#endif
  }

#if POST_3_0_CODE
  public void DrawMidwayArrow(Vector3[] corners, RectTransform.Axis direction, float headSizePx)
  {
   Vector3 s, e;
   Vector3 headSideways, headBack;
   switch( direction )
   {
    case RectTransform.Axis.Horizontal:
     s = Vector3.Lerp(corners[0], corners[1], 0.5f);
     e = Vector3.Lerp(corners[2], corners[3], 0.5f);
     headSideways = (corners[1] - corners[0]).normalized;
     headBack = (corners[2] - corners[1]).normalized;
     break;
    case RectTransform.Axis.Vertical:
     s = Vector3.Lerp(corners[1], corners[2], 0.5f);
     e = Vector3.Lerp(corners[3], corners[0], 0.5f);
     headSideways = (corners[2] - corners[1]).normalized;
     headBack = (corners[3] - corners[2]).normalized;
     break;
    default:
     throw new Exception("Impossible: 2lkvskl25jk");
   }

   /** Arrow line */
   DrawLineBold( s, e );
   /** Arrow head: s */
   DrawLineBold( s, s + headSizePx * (headSideways + headBack) );
   DrawLineBold( s, s + headSizePx * (headSideways + headBack) );
   DrawLineBold( s, s + headSizePx * (-headSideways + headBack) );
   DrawLineBold( s, s + headSizePx * (-headSideways + headBack) );
   /** Arrow head: e */
   DrawLineBold( e, e + headSizePx * (headSideways - headBack) );
   DrawLineBold( e, e + headSizePx * (headSideways - headBack) );
   DrawLineBold( e, e + headSizePx * (-headSideways - headBack) );
   DrawLineBold( e, e + headSizePx * (-headSideways - headBack) );
  }

  public void DrawLineBold(Vector3 s, Vector3 e)
  {
   Vector3 orth = Vector3.Cross(Vector3.forward, (e - s)).normalized;
   
   /**
    * The insanely-bad design of Unity's Canvas class (shipped broken, never properly architected or fixed; they
    * just stuck some band-aids on with the horrible "CanvasScaler" which is a poor hack to try and un-break the
    * Canvas) means that we have to detect the massively varying canvas "scale" based on canvas "mode"
    */
   float canvasScaleInUnity = this.Canvas().transform.localScale.x;
   
   Gizmos.DrawLine(s,e);
   Gizmos.DrawLine(s+orth*canvasScaleInUnity,e+orth*canvasScaleInUnity);
   Gizmos.DrawLine(s-orth*canvasScaleInUnity,e-orth*canvasScaleInUnity);
  }

  public void DrawWireframe3(Vector3[] corners, Vector2 canvasScaleFactor, int selectionDepth )
  {
   //ExtendedGizmos.DrawWireframeRectangleFractionalSides(corners, 0.1f);

   /**
    * The insanely-bad design of Unity's Canvas class (shipped broken, never properly architected or fixed; they
    * just stuck some band-aids on with the horrible "CanvasScaler" which is a poor hack to try and un-break the
    * Canvas) means that we have to detect the massively varying canvas "scale" based on canvas "mode"
    */
   float canvasScaleInUnity = this.Canvas().transform.localScale.x; 
   
   //Gizmos.color = Color.magenta;
   float colourDepths = 12f;
   Gizmos.color = Color.yellow * ((colourDepths-selectionDepth)/colourDepths);
   DrawMidwayArrow( corners, RectTransform.Axis.Horizontal, 10f * canvasScaleInUnity );
   //Gizmos.color = Color.yellow;
   DrawMidwayArrow( corners, RectTransform.Axis.Vertical, 10f * canvasScaleInUnity );
  }
  
  public void DrawWireframe2(Vector3[] corners, Vector2 canvasScaleFactor, int selectionDepth )
  {
   ExtendedGizmos.DrawWireframeRectangleFractionalSides(corners, 0.1f);

   bool bold = true;
   if( bold )
   {
    corners = ExtendedGizmos.InsetRectanglePixels(corners, canvasScaleFactor * 1f);
    ExtendedGizmos.DrawWireframeRectangleFractionalSides(corners, 0.1f);
    corners = ExtendedGizmos.InsetRectanglePixels(corners, canvasScaleFactor * 1f);
    ExtendedGizmos.DrawWireframeRectangleFractionalSides(corners, 0.1f);
    corners = ExtendedGizmos.InsetRectanglePixels(corners, canvasScaleFactor * 1f);
    ExtendedGizmos.DrawWireframeRectangleFractionalSides(corners, 0.1f);
   }
  }
#else
  public void DrawWireframe( Vector3[] corners, Vector2 canvasScaleFactor, int selectionDepth )
  {
   ExtendedGizmos.DrawWireframeRectangleFractionalSides( corners, 0.25f );

   corners = ExtendedGizmos.InsetRectanglePixels( corners, canvasScaleFactor * 1f );
   ExtendedGizmos.DrawWireframeRectangleFractionalSides( corners, 0.25f );
   corners = ExtendedGizmos.InsetRectanglePixels( corners, canvasScaleFactor * 1f );
   ExtendedGizmos.DrawWireframeRectangleFractionalSides( corners, 0.25f );
   corners = ExtendedGizmos.InsetRectanglePixels( corners, canvasScaleFactor * 1f );
   ExtendedGizmos.DrawWireframeRectangleFractionalSides( corners, 0.25f );
  }
#endif

  /**
   * InsetAmount is in pixels, but in a World-Space canvas, Unity will treat
   * it as meters.
   *
   * So we need to pre-multiply it by the canvas's scale every time we use it.
   */
  public void DrawAsWireframe( Action<Vector3[], Vector2, int> renderer, float childInset, float insetAmount = 0f, int selectionDepth = 0 )
  {
   RectTransform rt = (transform as RectTransform);
   Vector3[] corners = new Vector3[4];
   rt.GetWorldCorners( corners );
   Canvas canvas = this.Canvas();
   Vector2 canvasScaleFactor = new Vector2( canvas.transform.localScale.x, canvas.transform.localScale.y );
   corners = ExtendedGizmos.InsetRectanglePixels( corners, canvasScaleFactor * insetAmount );
   //Debug.Log("canvasScaleFactor = "+canvasScaleFactor.x);

   Color cornersColour = selectionDepth == 0 ? Color.green : new Color( 0, 0.75f, 0 );
   Gizmos.color = cornersColour;
#if POST_3_0_CODE
    renderer.Invoke( corners, canvasScaleFactor, selectionDepth );
    if( selectionDepth == 0 )
    {
     //renderer.Invoke( corners, canvasScaleFactor, selectionDepth );
     //Color c = new Color(1f,1f,1f,0.75f);
     //Color c = new Color(0f,0f,0f,0.75f);
     Color c = new Color(0f,0f,0f,0.5f);
     Texture2D texture; // = EditorGUIUtility.whiteTexture;
     texture = new Texture2D(1,1);
     texture.SetPixel(0,0, c);
     texture.Apply();

     Rect cornersRect = UnityRectExtensions.Rect(corners, transform as RectTransform);
     float scaleX = canvas.transform.localScale.x;
     float scaleY = canvas.transform.localScale.y;
     Rect rScaled = new Rect( new Vector2( cornersRect.x / scaleX, cornersRect.y/scaleY),
      new Vector2( cornersRect.width/scaleX, cornersRect.height/scaleY));
     //Debug.Log("rect = "+cornersRect+", scaled = "+rScaled+", position = "+transform.position);

     //cornersRect.x = 0;
     //Gizmos.DrawGUITexture( new Rect( cornersRect.position - (new Vector2(scaleX*transform.position.x, scaleY*transform.position.y)), cornersRect.size), texture );
     //Gizmos.DrawGUITexture( new Rect( cornersRect.position - (new Vector2(0.5f/scaleX, scaleY*transform.position.y)), cornersRect.size), texture );
     Gizmos.DrawGUITexture( cornersRect, texture );
     
    
     FlexItem flexItem = gameObject.GetComponent<FlexItem>();
     string text = name + "\n" + (flexItem != null ? "Basis:" + flexItem.flexBasis.mode : "Direction: " + direction);
     Handles.Label( transform.position + 0*(corners[2]-corners[0])/2f, text, new GUIStyle() { fontStyle = FontStyle.Bold, normal = new GUIStyleState() { textColor = Gizmos.color }} );
    }
    else
     Handles.Label( transform.position + 0*(corners[2]-corners[0])/2f, hierarchicalNameShort(), new GUIStyle() { fontStyle = FontStyle.Bold, normal = new GUIStyleState() { textColor = Gizmos.color }} );
#else
   DrawWireframe( corners, canvasScaleFactor, selectionDepth );
#endif

   foreach( Transform t in transform )
   {
    if( t.gameObject.activeSelf )
    {
     FlexItem flexChild = t.gameObject.GetComponent<FlexItem>();
     FlexContainer flexChildAsContainer = t.gameObject.GetComponent<FlexContainer>();

     if( flexChildAsContainer != null )
      flexChildAsContainer.DrawAsWireframe( renderer, childInset, insetAmount + childInset, selectionDepth + 1 );
     else if( flexChild != null )
      flexChild.DrawAsWireframe( insetAmount + childInset, selectionDepth + 1 );
    }
   }
  }

  void ILayoutController.SetLayoutVertical()
  {
   if( showDebugMessages ) Debug.Log( "[" + name + "] ignoring: UnityUI request to layout vertical (this callback is never needed, UnityUI always sends both callbacks)" );
  }

 }
}