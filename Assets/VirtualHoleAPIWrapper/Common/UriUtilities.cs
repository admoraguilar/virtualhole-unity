using System;

namespace VirtualHole.APIWrapper
{
	public static class UriUtilities
	{
		public static Uri CreateUri(
			string domain, string version, 
			string path, string slug)
		{
			string relativeUri = $"{path}/{slug}";
			if(!string.IsNullOrEmpty(version)) { relativeUri = relativeUri.Insert(0, $"{version}/"); }
			return new Uri(new Uri(domain), relativeUri);
		}
	}
}
