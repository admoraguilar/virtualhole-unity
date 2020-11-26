using System;
using UnityEngine;

namespace IntelligentPluginVersioning
{
/**
 * IntelligentPluginVersioning - plugin-management system for Unity Asset Store publishers
 *
 * Last updated: 2020-March-28th
 */
	
	[System.Serializable]
	public struct Version
	{
		public int major;
		public int minor;
		public int patch;

		public Version( int m )
		{
			major = m;
			minor = 0;
			patch = 0;
		}

		public Version( int maj, int min )
		{
			major = maj;
			minor = min;
			patch = 0;
		}

		public Version( int maj, int min, int p )
		{
			major = maj;
			minor = min;
			patch = p;
		}

		public Version( string s )
		{
			if( s == null || s.Length < 1 )
				major = minor = patch = 0;
			else
			{
				var ns = s.Split( '.' );
				if( ns.Length == 0 )
					major = minor = patch = 0;
				else
				{
					if( ns.Length > 2 )
						patch = int.Parse( ns[2] );
					else
						patch = 0;

					if( ns.Length > 1 )
						minor = int.Parse( ns[1] );
					else
						minor = 0;

					major = int.Parse( ns[0] );
				}
			}
		}

		public Version Major()
		{
			return new Version( major, 0, 0 );
		}
		public Version MajorMinor()
		{
			return new Version( major, minor, 0 );
		}

		private static readonly Version zeroVersion = new Version(0,0,0);
		public static Version zero // doing it Unity-style, in 4 lines instead of C#6.0-style in 1 line
		{
			get { return zeroVersion; }
		}

		public override bool Equals (object obj)
		{
			Version other = (Version) obj;

			return other.major == major && other.minor == minor && other.patch == patch;
		}

		public override int GetHashCode ()
		{
			return (major*10000000+minor*1000+patch).GetHashCode ();
		}

		#region operator overloads
		public static bool operator ==(Version left, Version right)
		{
			return left.Equals( right );
		}

		public static bool operator !=(Version left, Version right )
		{
			return ! left.Equals( right );
		}

		public static bool operator <(Version left, Version right)
		{
			return left.major < right.major || (left.major == right.major && left.minor < right.minor ) || (left.major == right.major && left.minor == right.minor && left.patch < right.patch );
		}
		public static bool operator <=(Version left, Version right)
		{
			return left.major < right.major || (left.major == right.major && left.minor < right.minor ) || (left.major == right.major && left.minor == right.minor && left.patch <= right.patch );
		}

		public static bool operator >(Version left, Version right)
		{
			return left.major > right.major || (left.major == right.major && left.minor > right.minor ) || (left.major == right.major && left.minor == right.minor && left.patch > right.patch );
		}
		public static bool operator >=(Version left, Version right)
		{
			return left.major > right.major || (left.major == right.major && left.minor > right.minor ) || (left.major == right.major && left.minor == right.minor && left.patch >= right.patch );
		}

		public static Version operator -(Version left, Version right)
		{
			return new Version( left.major-right.major, left.minor-right.minor, left.patch-right.patch);
		}
		#endregion

		public string StringMajorMinorOnly()
		{
			return major + "." + minor;
		}

		public string ToStringFull()
		{
			return major + "." + minor + "." + patch;
		}
		public override string ToString ()
		{
			return major+"."+minor+ (patch == 0 ? "" : "p"+patch);
		}
	}
}