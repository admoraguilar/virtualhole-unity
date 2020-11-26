using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A Vector2, with EITHER of its fields independently nullable
 */
public struct VectorNullable2
{
 private static readonly VectorNullable2 zeroVector = new VectorNullable2(0.0f, 0.0f);
 public static VectorNullable2 zero
 {
  get
  {
   return VectorNullable2.zeroVector;
  }
 }
 
 public float? x;
 public float? y;
 
 public VectorNullable2(float? x, float? y)
 {
  this.x = x;
  this.y = y;
 }

 public VectorNullable2(Vector2 v) : this(v.x,v.y)
 {
 }

 public override string ToString()
 {
  return "{" + (x?.ToString() ?? "null") + "," + (y?.ToString() ?? "null") + "}";
 }
}