using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Midnight.Internet;
using Midnight.Concurrency;

namespace Midnight.Pages
{
	public class Page : MonoBehaviour
	{
		public event Action OnLoadStart = delegate { };
		public event Action OnLoadFinish = delegate { };
		public event Action OnUnloadStart = delegate { };
		public event Action OnUnloadFinish = delegate { };

		public new Transform transform => this.GetComponent(ref _transform, () => base.transform);
		private Transform _transform = null;

		public async Task RefreshAsync(CancellationToken cancellationToken = default)
		{
			MLog.Log(nameof(Page), $"Refresh triggered: {name}");
			await UnloadAsync();
			await LoadAsync(cancellationToken);
		}

		public async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try {
				OnLoadStart();
				foreach(Section section in GetSections<Section>()) {
					await section.LoadAsync(cancellationToken);
				}
				OnLoadFinish();
			} catch(OperationCanceledException) {
				await UnloadAsync();
			}
		}

		public async Task UnloadAsync()
		{
			OnUnloadStart();
			foreach(Section section in GetSections<Section>()) {
				await section.UnloadAsync();
			}
			OnUnloadFinish();
		}

		public T GetSection<T>() where T : Section
		{
			return GetComponentInChildren<T>();
		}

		public T[] GetSections<T>() where T : Section
		{
			return GetComponentsInChildren<T>();
		}

		private void OnInternetError(string message)
		{
			TaskExt.FireForget(RefreshAsync());
		}

		private void OnInternetErrorResolved()
		{
			TaskExt.FireForget(RefreshAsync());
		}

		private void OnEnable()
		{
			InternetReachability.OnError += OnInternetError;
			InternetReachability.OnErrorResolved += OnInternetErrorResolved;
		}

		private void OnDisable()
		{
			InternetReachability.OnError -= OnInternetError;
			InternetReachability.OnErrorResolved -= OnInternetErrorResolved;
		}
	}
}
