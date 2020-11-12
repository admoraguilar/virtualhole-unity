using System;
using System.IO;

namespace VirtualHole.Client.Data
{
	//public interface ILocatableFile
	//{
	//	string rootPath { get; }
	//	string subPath { get; }
	//	string filePath { get; }
	//}

	//public static class LocatableDataDefaults
	//{
	//	public static string GetFullPath(this ILocatableFile locatableData)
	//	{
	//		if(Uri.IsWellFormedUriString(locatableData.rootPath, UriKind.RelativeOrAbsolute)) {
	//			return UriUtilities.CombineUri(
	//				locatableData.rootPath,
	//				Path.Combine(locatableData.subPath, locatableData.filePath)).AbsoluteUri;
	//		} else {
	//			return Path.Combine(
	//				locatableData.rootPath, locatableData.subPath,
	//				locatableData.filePath);
	//		}
	//	}
	//}
}
