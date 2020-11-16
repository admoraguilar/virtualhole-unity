using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelligentPluginVersioning
{
	public enum LogImportance
	{
		DEFAULT,
		MAJOR,
		MINOR
	}
	public struct LogEntry
	{
		public Version version;
		public string description;
		public LogImportance significance;

		public LogEntry(Version v, string d) : this(v,d,LogImportance.DEFAULT)
		{
		}
		public LogEntry(Version v, string d, LogImportance imp )
		{
			version = v;
			description = d;
			significance = imp;
		}
		
		public bool isMajor
		{
			get { return significance == LogImportance.MAJOR; }
		}
	}

	public struct VersionMetadata
	{
		public Version version;
		public DateTime releaseDate;

		public VersionMetadata(Version v, DateTime dt)
		{
			version = v;
			releaseDate = dt;
		}
	}

	public class VersionLog
	{
		private List<LogEntry> entries = new List<LogEntry>();
		private List<VersionMetadata> releaseDates = new List<VersionMetadata>();

		public void AddVersion(Version v, DateTime releaseDate)
		{
			releaseDates.Add( new VersionMetadata(v, releaseDate) );
		}

		public void Add(Version v, string d, LogImportance li = LogImportance.DEFAULT)
		{
			entries.Add( new LogEntry(v,d,li));
		}

		public IEnumerable<VersionMetadata> AllVersions()
		{
			return releaseDates.OrderBy( d1 => d1.releaseDate );
		}
		
		public IEnumerable<LogEntry> OnlyMajorEntries()
		{
			return entries.Where((r => r.significance == LogImportance.MAJOR));
		}
		
		public IEnumerable<LogEntry> OnlyMajorEntriesAndPatchesFrom( Version templateVersion )
		{
			return entries.Where(r => r.significance == LogImportance.MAJOR || ((r.version.major == templateVersion.major) && (r.version.minor == templateVersion.minor)));
		}
		
		public IEnumerable<LogEntry> OnlyNormalEntriesInVersionsSince( Version templateVersion )
		{
			return entries.Where( r => (r.version >= templateVersion));
		}
		
		public IEnumerable<LogEntry> All()
		{
			return new List<LogEntry>( entries );
		}
	}
}