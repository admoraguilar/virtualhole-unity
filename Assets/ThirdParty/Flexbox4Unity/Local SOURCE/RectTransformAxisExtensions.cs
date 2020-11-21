using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * In CSS/flexbox, we very often need to run an algorithm against EITHER x OR y, but interchangeably,
 * while being given a Vector2 that contains both.
 *
 * Since Unity failed to create a method that gives you "X if axis is horizontal, Y if vertical",
 * I wrote one...
 *
 * (and it even supports cases where the incoming Vector2 is potentially null)
 */
public static class RectTransformAxisExtensions
{
	public static float Length(this RectTransform.Axis axis, Vector2 size)
	{
		return (axis == RectTransform.Axis.Vertical) ? size.y : size.x;
	}
	public static float? Length(this RectTransform.Axis axis, Vector2? size)
	{
		return (axis == RectTransform.Axis.Vertical) ? size?.y : size?.x;
	}
	public static float? Length(this RectTransform.Axis axis, VectorNullable2? size)
	{
		return (axis == RectTransform.Axis.Vertical) ? size?.y : size?.x;
	}

	public static RectTransform.Axis Swap(this RectTransform.Axis axis)
	{
		return (axis == RectTransform.Axis.Vertical) ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;
	}
}