using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Midnight;
using UnityEditorInternal;

namespace Holoverse.Client.UI
{
	public class SupportView : MonoBehaviour, ISimpleCycleAsync
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

		[SerializeField]
		private InfoButton _infoButtonPrefab = null;

		[SerializeField]
		private RectTransform _contentContainer = null;

		public bool isInitializing { get; private set; } = false;
		public bool isInitialized { get; private set; } = false;
		public bool isLoading { get; private set; } = false;

		private Func<CancellationToken, Task> _dataFactory = null;
		private IEnumerable<InfoButtonData> _data = null;
		private List<InfoButton> _instances = new List<InfoButton>();

		public void SetData(Func<CancellationToken, Task> dataFactory)
		{
			_dataFactory = dataFactory;
		}

		public void SetData(IEnumerable<InfoButtonData> data)
		{
			_data = data;
		}

		public async Task InitializeAsync(CancellationToken cancellationToken = default)
		{
			if(!this.CanInitialize()) { return; }
			isInitializing = true;
			OnInitializeStart(null);

			try {
				if(_dataFactory != null) {
					await _dataFactory(cancellationToken);
				}

				foreach(InfoButtonData data in _data) {
					InfoButton button = Instantiate(_infoButtonPrefab, _contentContainer, false);
					button.name = data.header;
					button.SetData(data);
				}

			} catch(Exception e) {
				OnInitializeError(e, null);
				throw;
			}

			OnInitializeFinish(null);
			isInitialized = true;
		}

		public async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			if(!this.CanLoad()) { return; }
			await Task.CompletedTask;
		}

		public async Task UnloadAsync()
		{
			await Task.CompletedTask;
			if(!this.CanUnload()) { return; }
			OnUnloadStart(null);

			foreach(InfoButton instance in _instances) {
				Destroy(instance.gameObject);
			}
			_instances.Clear();

			isLoading = false;
			isInitializing = false;
			isInitialized = false;
			OnUnloadFinish(null);
		}
	}
}
