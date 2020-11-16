using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityRectExtensions
{
	/**
	 * Workaround for annoying bugs in Unity RectTransform API: their own API for on-screen coords works in v3-array, but
	 * their own Rect API won't allow you to create a Rect from v3-array. They were just too lazy to add the missing methods
	 * when they created RectTransform.
	 */
	public static Rect Rect(Vector3[] corners, RectTransform transform)
	{
		Rect r = new Rect(corners[0].x, corners[0].y, corners[2].x-corners[0].x, corners[2].y-corners[0].y);
		//r.center = r.center + (Vector2)transform.position;
		return r;
	}
}