using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Unity2018FixMissingTryGetComponent
{
#if UNITY_2019_2_OR_NEWER
 // Not needed: Unity has this method built in from here onwards
#else
	/** This core method is missing from Unity 2018.4 LTS and Unity 2019.1, and Unity hasn't backported it,
	 * despite the huge value of doing so (all codebases should be using it, but cannot, because 2018 LTS only went
	 * live in early 2020, so it will be at least 2022 before that code can be retired!)
	 *
	 * The official one in Unity2019 onwards is a better implementation, this is just a patch to maintain codebase compatibility
	 * until every game written in Unity2018 has finally shipped, sold all copies, and then been removed from the market...
	 */
	public static bool TryGetComponent<T>( this GameObject go, out T component) where T : Object
	{
		component = go.GetComponent<T>();
		return component != null; 
	}
	
	/** This core method is missing from Unity 2018.4 LTS and Unity 2019.1, and Unity hasn't backported it,
	 * despite the huge value of doing so (all codebases should be using it, but cannot, because 2018 LTS only went
	 * live in early 2020, so it will be at least 2022 before that code can be retired!)
	 *
	 * The official one in Unity2019 onwards is a better implementation, this is just a patch to maintain codebase compatibility
	 * until every game written in Unity2018 has finally shipped, sold all copies, and then been removed from the market...
	 */
	public static bool TryGetComponent<T>( this MonoBehaviour mb, out T component) where T : Object
	{
		component = mb.GetComponent<T>();
		return component != null; 
	}
	
	/** This core method is missing from Unity 2018.4 LTS and Unity 2019.1, and Unity hasn't backported it,
	 * despite the huge value of doing so (all codebases should be using it, but cannot, because 2018 LTS only went
	 * live in early 2020, so it will be at least 2022 before that code can be retired!)
	 *
	 * The official one in Unity2019 onwards is a better implementation, this is just a patch to maintain codebase compatibility
	 * until every game written in Unity2018 has finally shipped, sold all copies, and then been removed from the market...
	 */
	public static bool TryGetComponent<T>( this Transform t, out T component) where T : Object
	{
		component = t.GetComponent<T>();
		return component != null; 
	}
#endif
}