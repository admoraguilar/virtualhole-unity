using System;

namespace VirtualHole.APIWrapper
{
	public static class UriUtilities
	{
		public static Uri CreateUri(string domain, string path, string slug)
		{
			return new Uri(new Uri(domain), $"{path}/{slug}");
		}
	}
}
