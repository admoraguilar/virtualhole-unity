using UnityEngine;

namespace Midnight.Unity
{
	public struct ColorHSV
	{
		public float h;
		public float s;
		public float v;
		public float a;
		public bool isHdr;

		public ColorHSV(float h, float s, float v) : this(h, s, v, 1f, false) { }

		public ColorHSV(float h, float s, float v, float a) : this(h, s, v, a, false) { }

		public ColorHSV(float h, float s, float v, float a, bool isHdr)
		{
			this.h = h;
			this.s = s;
			this.v = v;
			this.a = a;
			this.isHdr = isHdr;
		}

		public static implicit operator Color(ColorHSV colorHSV)
		{
			Color color = Color.HSVToRGB(colorHSV.h, colorHSV.s, colorHSV.v);
			color.a = colorHSV.a;
			return color;
		}

		public static implicit operator ColorHSV(Color color)
		{
			float h, s, v = 0f;
			Color.RGBToHSV(color, out h, out s, out v);
			return new ColorHSV(h, s, v, color.a);
		}
	}
}
