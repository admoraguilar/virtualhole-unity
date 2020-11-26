using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flexbox4Unity
{
	public enum CSS3LengthType
	{
		PIXELS = 0,
		PERCENT = 1,

		// AUTO = 2 // TODO: not supported yet
		NONE = 3, // equivalent to "ignore; this is not a valid value"
	}

	[System.Serializable]
	public struct CSS3Length
	{
		public CSS3LengthType mode; // should be readonly, but UnityEditor's "SerializedProperty" system crashes on readonly fields
		public float value; // should be readonly, but UnityEditor's "SerializedProperty" system crashes on readonly fields

		public CSS3Length(float f)
		{
			mode = CSS3LengthType.PIXELS;
			value = f;
		}

		public CSS3Length(float f, CSS3LengthType t)
		{
			mode = t;
			value = f;
		}

		public CSS3Length(CSS3LengthType t)
		{
			mode = t;
			value = 0;
		}

		public bool hasValue
		{
			get { return mode != CSS3LengthType.NONE; }
		}

		public static float Clamp( CSS3Length min, CSS3Length max, float length, float? containerLength)
		{
			length = Mathf.Max(length, min.ValueOrNull(containerLength) ?? length);
			length = Mathf.Min(length, max.ValueOrNull(containerLength) ?? length);
			return length;
		}
		
		public float Clamp( CSS3Length min, CSS3Length max, float? containerLength)
		{
			float length = ValueOrNull(containerLength) ?? 0;
			length = Mathf.Max(length, min.ValueOrNull(containerLength) ?? length);
			length = Mathf.Min(length, max.ValueOrNull(containerLength) ?? length);
			return length;
		}
		
		/**
		 * This is the preferred method: all callers should upgrade to this or a variant.
		 *
		 * @return value if possible, or null if EITHER you requested a "percent of null", OR requested "none of (something)"
		 */
		public float? ValueOrNull(float? containerLength)
		{
			return _ResolveUsingMode(containerLength);
		}
		/**
		 * This is the preferred method: all callers should upgrade to this or a variant.
		 *
		 * @return value if possible, or null if you requested "none of (something)"
		 */
		public float? ValueOrNull(float containerLength)
		{
			return _ResolveUsingMode(containerLength);
		}
		
		public float ValueOrZero(float? containerLength) // TODO: remove all methods that call this
		{
			return _ResolveUsingMode(containerLength) ?? 0;
		}

		public float ValueOrZero(float containerLength) // TODO: remove all methods that call this
		{
			return _ResolveUsingMode(containerLength) ?? 0;
		}
		private float? _ResolveUsingMode( float? containerLength )
		{
			switch( mode )
			{
				case CSS3LengthType.PIXELS:
					return value;

				case CSS3LengthType.PERCENT:
					return containerLength.HasValue ? (value / 100f) * containerLength : null;

				case CSS3LengthType.NONE:
					return null;

				default:
					throw new Exception("Impossible CSS3Width enum value");
			}
		}

		public override string ToString()
		{
			switch( mode )
			{
				case CSS3LengthType.PIXELS:
					return "(" + value + " px)";

				case CSS3LengthType.PERCENT:
					return "(" + value + " %)";

				case CSS3LengthType.NONE:
					return "(n/a)";

				default:
					return "ERROR";
			}
		}

		private static readonly CSS3Length _none = new CSS3Length(CSS3LengthType.NONE);

		public static CSS3Length None
		{
			get { return _none; }
		}

		#region operator overloads

		public static bool operator ==(CSS3Length left, CSS3Length right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(CSS3Length left, CSS3Length right)
		{
			return !left.Equals(right);
		}

		public override bool Equals(object obj)
		{
			CSS3Length other = (CSS3Length) obj;

			return other.mode == mode && other.value == value;
		}

		public override int GetHashCode()
		{
			// ReSharper disable once NonReadonlyMemberInGetHashCode -- BUG in UnityEditor: readonly fields WILL BE deleted by Unity!
			return value.GetHashCode();
		}

		#endregion
	}
}