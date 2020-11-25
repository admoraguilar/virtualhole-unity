using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Midnight.Unity;

namespace VirtualHole.Client.UI
{
	public abstract class UILifecycle : MonoBehaviour, ISimpleCycleAsync
	{
		public event Action<object> OnInitializeStart = delegate { };
		public event Action<Exception, object> OnInitializeError = delegate { };
		public event Action<object> OnInitializeFinish = delegate { };

		public event Action<object> OnLoadStart = delegate { };
		public event Action<Exception, object> OnLoadError = delegate { };
		public event Action<object> OnLoadFinish = delegate { };

		public event Action<object> OnUnloadStart = delegate { };
		public event Action<Exception, object> OnUnloadError = delegate { };
		public event Action<object> OnUnloadFinish = delegate { };

		public bool isInitializing { get; private set; } = false;
		public bool isInitialized { get; private set; } = false;
		public bool isLoading { get; private set; } = false;

		protected CycleLoadParameters _loadingParameters = null;

		private Func<CancellationToken, Task> _dataFactory = null;

		public void SetDataAsyncFactory(Func<CancellationToken, Task> dataFactory)
		{
			_dataFactory = dataFactory;
		}

		public async Task InitializeAsync(CancellationToken cancellationToken = default) 
		{ 
			if(!this.CanInitialize()) { return; }
			isInitializing = true;
			OnInitializeStart(null);

			try {
				if(_dataFactory != null) { await _dataFactory(cancellationToken); }
				await InitializeAsync_Impl(cancellationToken);
			} catch(Exception e) {
				OnInitializeError(e, null);
				throw;
			}

			isInitialized = true;
			await PostInitializeAsync_Impl(cancellationToken);

			OnInitializeFinish(null);
		}

		protected virtual async Task InitializeAsync_Impl(CancellationToken cancellationToken = default) => await Task.CompletedTask;
		protected virtual async Task PostInitializeAsync_Impl(CancellationToken cancellationToken = default) => await Task.CompletedTask;

		public async Task LoadAsync(CancellationToken cancellationToken = default) 
		{ 
			if(!this.CanLoad()) { return; }
			isLoading = true;
			OnLoadStart(_loadingParameters);

			try {
				await LoadAsync_Impl(cancellationToken);
			} catch(Exception e) {
				OnLoadError(e, null);
				throw;
			} finally {
				isLoading = false;
			}

			OnLoadFinish(null);
		}

		protected virtual async Task LoadAsync_Impl(CancellationToken cancellationToken = default) => await Task.CompletedTask;

		public async Task UnloadAsync() 
		{ 
			if(isLoading) { return; }
			OnUnloadStart(null);

			await UnloadAsync_Impl();

			isLoading = false;
			isInitializing = false;
			isInitialized = false;
			OnUnloadFinish(null);
		}

		protected virtual async Task UnloadAsync_Impl() => await Task.CompletedTask;
	}
}
