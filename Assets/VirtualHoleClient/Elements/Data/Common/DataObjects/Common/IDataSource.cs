using System.IO;

namespace VirtualHole.Client.Data
{
	public interface IDataSource
	{
		string rootPath { get; }
		string subPath { get; }
		string filePath { get; }
	}

	public static class DataSourceDefaults
	{
		public static string GetFullPath(this IDataSource dataSource)
		{
			return Path.Combine(
				dataSource.rootPath, dataSource.subPath,
				dataSource.filePath);
		}
	}
}
