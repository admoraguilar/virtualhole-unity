#define PROPRIETARY_ASPECT_FLEXBASIS // This is not in CSS-3, added specifically for Unity and game-developers
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flexbox4Unity
{
	public enum FlexBasis
	{
		LENGTH = 0,
		PERCENT = 1,
		CONTENT = 2,
		#if PROPRIETARY_ASPECT_FLEXBASIS
		/* for testing: #if LITE_VERSION
		#else*/
		ASPECT_FIT = 3,
		/*#endif*/
		#endif
		AUTO = 4,
	}

	[System.Serializable]
	public struct FlexboxBasis
	{
		public FlexBasis mode; // should be readonly, but UnityEditor's "SerializedProperty" system crashes on readonly fields
		public float value; // should be readonly, but UnityEditor's "SerializedProperty" system crashes on readonly fields

		public FlexboxBasis(float v, FlexBasis m = FlexBasis.LENGTH)
		{
			mode = m;
			value = v;
		}

		public FlexboxBasis(FlexBasis mode)
		{
			this.mode = mode;
			value = 0f;
		}

		private static readonly FlexboxBasis _content = new FlexboxBasis(FlexBasis.CONTENT);

		/** Shorthand for creating a struct set to basis = CONTENT */
		public static FlexboxBasis Content
		{
			get { return _content; }
		}

		private static readonly FlexboxBasis _auto = new FlexboxBasis(FlexBasis.AUTO);

		/** Shorthand for creating a struct set to basis = AUTO */
		public static FlexboxBasis Auto
		{
			get { return _auto; }
		}

		public override string ToString()
		{
			switch( mode )
			{
				case FlexBasis.AUTO:
					return "auto";
				case FlexBasis.LENGTH:
					return value + "px";
				case FlexBasis.CONTENT:
					return "content";
				case FlexBasis.PERCENT:
					return value + "%";
				case FlexBasis.ASPECT_FIT:
					return "Flexbox4Unity.ASPECT_FIT";
					
				default:
					throw new Exception("Impossible: 235kljsdf");
			}
		}
	}
}