using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Midnight.Tasks;

namespace VirtualHole.Client.Data
{
	public class PaginatedQuerySettings<TRaw, TDTO>
		where TRaw : class
		where TDTO : class
	{
		public ICache<TDTO> dtoCache { get; set; } = null;
		public ICache<TRaw> rawCache { get; set; } = null;

		public PaginatedQuerySettings()
		{
			dtoCache = SimpleCache<TDTO>.Get();
			rawCache = SimpleCache<TRaw>.Get();
		}
	}

	public abstract class PaginatedQuery<TRaw, TDTO, TQuerySettings> :
		DataQuery<TRaw, List<TRaw>, IEnumerable<TRaw>,
				  TDTO, List<TDTO>, IEnumerable<TDTO>,
				  TQuerySettings>
		where TRaw : class
		where TDTO : class
		where TQuerySettings : PaginatedQuerySettings<TRaw, TDTO>, new()
	{
		public bool isRunning { get; protected set; } = false;
		public bool isDone { get; protected set; } = false;

		public PaginatedQuery(TQuerySettings querySettings = null) : base(querySettings) 
		{
			_dtoContainer = new List<TDTO>();
			_rawContainer = new List<TRaw>();
		}

		protected abstract string GetCacheKey(TRaw raw);

		public sealed override async Task<IEnumerable<TDTO>> GetDTOAsync(CancellationToken cancellationToken = default)
		{
			// NOTES: Future feature:
			// GetDTO for a certain paged results
			cancellationToken.ThrowIfCancellationRequested();

			if(isRunning) { return default; }
			if(isDone && _dtoContainer.Count > 0) { return _dtoContainer; }

			IEnumerable<TRaw> raws = await GetRawAsync(cancellationToken);
			if(raws == default) { return default; }

			List<TDTO> results = new List<TDTO>();
			List<TDTO> toProcess = new List<TDTO>();

			try {
				await PreProcessDTOAsync(raws, cancellationToken);

				foreach(TRaw raw in raws) {
					TDTO instance = default;
					if(!_querySettings.dtoCache.TryGet(GetCacheKey(raw), out TDTO cached)) {
						instance = FromRawToDTO(raw);

						_querySettings.dtoCache.Upsert(GetCacheKey(raw), instance);
						toProcess.Add(instance);
					} else {
						instance = cached;
					}
					results.Add(instance);
				}

				await Concurrent.ForEachAsync(toProcess, DoProcessDTOAsync, cancellationToken);
			} catch {
				foreach(TRaw raw in raws) {
					_querySettings.dtoCache.Remove(GetCacheKey(raw));
				}
				throw;
			}

			_dtoContainer.AddRange(results);
			return results;

			async Task DoProcessDTOAsync(TDTO dto) => await ProcessDTOAsync(dto, cancellationToken);
		}

		protected virtual async Task PreProcessDTOAsync(IEnumerable<TRaw> raws, CancellationToken cancellationToken = default)
		{
			await Task.CompletedTask;
		}

		protected abstract TDTO FromRawToDTO(TRaw raw);
		protected abstract Task ProcessDTOAsync(TDTO dto, CancellationToken cancellationToken = default);

		public sealed override async Task<IEnumerable<TRaw>> GetRawAsync(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if(isRunning) { return default; }
			if(isDone && _rawContainer.Count > 0) { return _rawContainer; }

			isRunning = true;

			IEnumerable<TRaw> results = default;
			try {
				results = await GetRawAsync_Impl(cancellationToken);
			} finally {
				isRunning = false;
			}

			if(results == default) { return default; }
			foreach(TRaw result in results) {
				_querySettings.rawCache.Upsert(GetCacheKey(result), result);
			}

			_rawContainer.AddRange(results);
			return results;
		}

		protected abstract Task<IEnumerable<TRaw>> GetRawAsync_Impl(CancellationToken cancellationToken = default);

		public virtual void Reset()
		{
			_dtoContainer.Clear();
			_rawContainer.Clear();
			isDone = false;
		}
	}
}
