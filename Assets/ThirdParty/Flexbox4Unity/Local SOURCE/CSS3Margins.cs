using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flexbox4Unity
{
	[System.Serializable]
	public struct CSS3Margins
	{
		private static readonly CSS3Margins _none = new CSS3Margins(false);
		public static CSS3Margins None
		{
			get { return _none; }
		}
		
		public bool isActive;
		public CSS3Length left, top, right, bottom; // should be readonly, but UnityEditor's "SerializedProperty" system crashes on readonly fields

		/** C# has a horrible feature that it doesn't allow zero-arg constructors, so we have
		 * to introduce a fake argument merely to create this constructor
		 *
		 * @param enableByDefault should always be false, marking the struct as state = UNSPECIFIED. All other constructors mark the state = SPECIFIED 
		 */
		public CSS3Margins(bool enableByDefault)
		{
			isActive = enableByDefault;
			top = bottom = left = right = new CSS3Length(0f);
		}

		public CSS3Margins(float all)
		{
			isActive = true;
			top = bottom = left = right = new CSS3Length(all);
		}

		public CSS3Margins(float topBottom, float leftRight)
		{
			isActive = true;
			top = bottom = new CSS3Length(topBottom);
			left = right = new CSS3Length(leftRight);
		}

		public void Set(float value)
		{
			left = top = right = bottom = new CSS3Length(value);
		}

		public void Set(float topBottom, float leftRight)
		{
			top = bottom = new CSS3Length(topBottom);
			left = right = new CSS3Length(leftRight);
		}

		public bool requiresParentSizeToResolve
		{
			get { return isActive && (top.mode == CSS3LengthType.PERCENT || left.mode == CSS3LengthType.PERCENT || right.mode == CSS3LengthType.PERCENT || bottom.mode == CSS3LengthType.PERCENT); }
		}

		public float ValueOrZero(RectTransform.Axis axis, Vector2 containerSize)
		{
			return axis == RectTransform.Axis.Horizontal ? HorizontalValueOrZero(containerSize) : VerticalValueOrZero(containerSize );
		}
		public float? ValueOrNull(RectTransform.Axis axis, Vector2? containerSize)
		{
			return axis == RectTransform.Axis.Horizontal ? HorizontalValueOrNull(containerSize) : VerticalValueOrNull(containerSize );
		}
		public float? ValueOrNull(RectTransform.Axis axis, VectorNullable2 containerSize)
		{
			return axis == RectTransform.Axis.Horizontal ? HorizontalValueOrNull(containerSize) : VerticalValueOrNull(containerSize );
		}

		public float LeftOrZero(Vector2 containerSize)
		{
			return isActive ? left.ValueOrZero(containerSize.x) : 0;
		}
		public float RightOrZero(Vector2 containerSize)
		{
			return isActive ? right.ValueOrZero(containerSize.x) : 0;
		}
		public float TopOrZero(Vector2 containerSize)
		{
			return isActive ? top.ValueOrZero(containerSize.y) : 0;
		}
		public float BottomOrZero(Vector2 containerSize)
		{
			return isActive ? bottom.ValueOrZero(containerSize.y) : 0;
		}
		
		public float? LeftOrNull(Vector2? containerSize)
		{
			return isActive ? left.ValueOrNull(containerSize?.x) : null;
		}
		public float? RightOrNull(Vector2? containerSize)
		{
			return isActive ? right.ValueOrNull(containerSize?.x) : null;
		}
		public float? TopOrNull(Vector2? containerSize)
		{
			return isActive ? top.ValueOrNull(containerSize?.y) : null;
		}
		public float? BottomOrNull(Vector2? containerSize)
		{
			return isActive ? bottom.ValueOrNull(containerSize?.y) : null;
		}

		public float VerticalValueOrZero(Vector2 containerSize)
		{
			return isActive ? top.ValueOrZero(containerSize.y) + bottom.ValueOrZero(containerSize.y) : 0f;
		}
		
		public float? VerticalValueOrNull(Vector2? containerSize)
		{
			return isActive ? top.ValueOrNull(containerSize?.y) + bottom.ValueOrNull(containerSize?.y) : 0f;
		}
		public float? VerticalValueOrNull(VectorNullable2 containerSize)
		{
			return isActive ? top.ValueOrNull(containerSize.y) + bottom.ValueOrNull(containerSize.y) : 0f;
		}

		public float HorizontalValueOrZero(Vector2 containerSize)
		{
			return isActive ? left.ValueOrZero(containerSize.x) + right.ValueOrZero(containerSize.x) : 0f;
		}
		
		public float? HorizontalValueOrNull(Vector2? containerSize)
		{
			return isActive ? left.ValueOrNull(containerSize?.x) + right.ValueOrNull(containerSize?.x) : 0f;
		}
		public float? HorizontalValueOrNull(VectorNullable2 containerSize)
		{
			return isActive ? left.ValueOrNull(containerSize.x) + right.ValueOrNull(containerSize.x) : 0f;
		}

		#region operator overloads

		public static bool operator ==(CSS3Margins left, CSS3Margins right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(CSS3Margins left, CSS3Margins right)
		{
			return !left.Equals(right);
		}

		public override bool Equals(object obj)
		{
			CSS3Margins other = (CSS3Margins) obj;

			return other.isActive == isActive && other.left == left && other.top == top && other.right == right && other.bottom == bottom;
		}

		public override int GetHashCode()
		{
			// ReSharper disable once NonReadonlyMemberInGetHashCode -- BUG in UnityEditor: readonly fields WILL BE deleted by Unity!
			return (left.value + top.value * 113 + right.value * 1074 + bottom.value * 30397).GetHashCode();
		}

		#endregion
	}
}