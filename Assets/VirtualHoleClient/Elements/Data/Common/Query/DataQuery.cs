using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VirtualHole.Client.Data
{
	public class DataQueryDTO<TRaw>
	{
		public TRaw raw;

		public DataQueryDTO(TRaw raw)
		{
			this.raw = raw;
		}
	}

	public abstract class DataQuery<
		TRaw, TRawContainer, TRawResult,
		TDTO, TDTOContainer, TDTOResult,
		TQuerySettings>
		where TQuerySettings : new()
	{
		protected TQuerySettings _querySettings = default;

		public DataQuery(TQuerySettings querySettings = default)
		{
			_querySettings = EqualityComparer<TQuerySettings>.Default.Equals(_querySettings, default) ? new TQuerySettings() : querySettings;
		}

		protected TDTOContainer _dtoContainer = default;
		protected TRawContainer _rawContainer = default;

		public abstract Task<TDTOResult> GetDTOAsync(CancellationToken cancellationToken = default);
		public abstract Task<TRawResult> GetRawAsync(CancellationToken cancellationToken = default);
	}
}
