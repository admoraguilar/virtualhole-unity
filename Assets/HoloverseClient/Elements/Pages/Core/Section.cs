using Midnight.Internet;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using DnsClient.Protocol;

namespace Midnight.Pages
{
	public abstract class Section : MonoBehaviour
	{
		[Flags]
		public enum Requirement
		{
			None,
			Internet,
			AppCompatible
		};

		public enum DisplayType
		{
			Content,
			LoadingIndicator,
			GenericError,
			InternetError,
			AppIncompatibleError
		}

		public virtual Requirement requirements => Requirement.None;

		private Dictionary<DisplayType, GameObject> _displays = new Dictionary<DisplayType, GameObject>();

		[Header("Displays")]
		[SerializeField]
		private GameObject _contentDisplay = null;

		[SerializeField]
		private GameObject _loadingIndicatorDisplay = null;

		[SerializeField]
		private GameObject _genericErrorDisplay = null;

		[SerializeField]
		private GameObject _internetErrorDisplay = null;

		[SerializeField]
		private GameObject _appIncompatibleErrorDisplay = null;

		public async Task RefreshAsync(CancellationToken cancellationToken = default)
		{
			MLog.Log(nameof(Section), $"Refresh triggered: {name}");
			await UnloadAsync();
			await LoadAsync(cancellationToken);
		}

		public async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			if(!InternetReachability.isReachable && requirements.HasFlag(Requirement.Internet)) {
				SetDisplayActive(GetDisplay(DisplayType.InternetError));
			} else if(!AppManifestState.isCompatible && requirements.HasFlag(Requirement.AppCompatible)) {
				SetDisplayActive(GetDisplay(DisplayType.AppIncompatibleError));
			} else {
				SetDisplayActive(GetDisplay(DisplayType.LoadingIndicator));		
				try {
					await LoadContentAsync(cancellationToken);
					SetDisplayActive(GetDisplay(DisplayType.Content));
				} catch(Exception e) {
					if(!(e is OperationCanceledException)) {
						SetDisplayActive(GetDisplay(DisplayType.GenericError));
					}
					throw;
				}
			}
		}

		public async Task UnloadAsync()
		{
			await UnloadContentAsync();
		}

		protected abstract Task LoadContentAsync(CancellationToken cancellationToken = default);
		protected abstract Task UnloadContentAsync();

		private void SetDisplayActive(GameObject display)
		{
			foreach(GameObject d in _displays.Values) {
				if(d == display) { continue; }
				d.SetActive(false);
			}

			display.transform.SetAsLastSibling();
			display.SetActive(true);
		}

		private GameObject GetDisplay(DisplayType type)
		{
			if(!_displays.TryGetValue(type, out GameObject display)) {
				if(type == DisplayType.Content) { display = _contentDisplay; } 
				else if(type == DisplayType.LoadingIndicator) { display = GetDefaultsIfNull(_loadingIndicatorDisplay, PagesSettings.defaultLoadingIndicatorDisplay); } 
				else if(type == DisplayType.GenericError) { display = GetDefaultsIfNull(_genericErrorDisplay, PagesSettings.defaultGenericErrorDisplay); }
				else if(type == DisplayType.InternetError) { display = GetDefaultsIfNull(_internetErrorDisplay, PagesSettings.defaultInternetErrorDisplay); } 
				else if(type == DisplayType.AppIncompatibleError) { display = GetDefaultsIfNull(_appIncompatibleErrorDisplay, PagesSettings.defaultAppIncompatibleErrorDisplay); }

				_displays[type] = display;
			}

			return display;

			GameObject GetDefaultsIfNull(GameObject instance, GameObject prefab)
			{
				if(instance != null && !instance.IsPrefab()) { return instance; }
				else if(instance != null && instance.IsPrefab()) { return Instantiate(instance, transform); }
				else if(instance == null) { return Instantiate(prefab, transform); }
				return null;
			}
		}
	}
}
