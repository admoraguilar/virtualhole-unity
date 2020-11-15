using System;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Internet;

namespace VirtualHole.Client.Controllers
{
	using Client.ComponentMaps;

	public class PageDisplayController : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] _simpleCyclesObjects = null;

		[SerializeField]
		private PageDisplayMap _pageDisplayMap = null;

		private List<GameObject> _pageDisplays = null;
		private List<ISimpleCycleAsync> _simpleCycleList = null;
		private CycleLoadParameters _loadParameters = null;

		private void OnCycleInitializeStart(object data)
		{
			if(!InternetReachability.isReachable) {
				SetDisplayActive(_pageDisplayMap.internetErrorDisplay, true);
			} else if(!AppManifestState.isCompatible) {
				SetDisplayActive(_pageDisplayMap.appIncompatibleErrorDisplay, true);
			} else {
				SetDisplayActive(_pageDisplayMap.loadingDisplay.gameObject, true);
				_pageDisplayMap.loadingDisplay.SetProgress(.8f);
			}
		}

		private void OnCycleInitializeError(Exception e, object data)
		{
			if(!(e is OperationCanceledException)) {
				SetDisplayActive(_pageDisplayMap.genericErrorDisplay, true);
			}
		}

		private void OnCycleInitializeFinish(object data)
		{
			SetDisplayActive(null, false);
		}

		private void OnCycleLoadStart(object data)
		{
			if(data is CycleLoadParameters loadParams) {
				_loadParameters = loadParams;
				if(_loadParameters.isShowLoadingIndicator) {
					SetDisplayActive(_pageDisplayMap.loadingDisplay.gameObject, true);
					_pageDisplayMap.loadingDisplay?.SetProgress(.8f);
				}
			} else {
				_loadParameters = null;
			}
		}

		private void OnCycleLoadError(Exception e, object data)
		{

		}

		private void OnCycleLoadFinish(object data)
		{
			SetDisplayActive(null, false);
		}

		private void OnCycleUnloadStart(object data)
		{

		}

		private void OnCycleUnloadError(Exception e, object data)
		{

		}

		private void OnCycleUnloadFinish(object data)
		{
			
		}

		private void SetDisplayActive(GameObject display, bool value)
		{
			foreach(GameObject d in _pageDisplays) {
				if(d == display) { continue; }
				d.SetActive(false);
			}

			if(display != null) {
				display.SetActive(value);
			}
		}

		private void Awake()
		{
			_pageDisplays.Add(_pageDisplayMap.loadingDisplay.gameObject);
			_pageDisplays.Add(_pageDisplayMap.internetErrorDisplay);
			_pageDisplays.Add(_pageDisplayMap.appIncompatibleErrorDisplay);
			_pageDisplays.Add(_pageDisplayMap.genericErrorDisplay);

			foreach(GameObject simpleCycleObject in _simpleCyclesObjects) {
				ISimpleCycleAsync simpleCycle = simpleCycleObject.GetComponent<ISimpleCycleAsync>();
				_simpleCycleList.Add(simpleCycle);
			}
			
		}

		private void OnEnable()
		{
			foreach(ISimpleCycleAsync simpleCycle in _simpleCycleList) {
				simpleCycle.OnInitializeStart += OnCycleInitializeStart;
				simpleCycle.OnInitializeError += OnCycleInitializeError;
				simpleCycle.OnInitializeFinish += OnCycleInitializeFinish;
				simpleCycle.OnLoadStart += OnCycleLoadStart;
				simpleCycle.OnLoadError += OnCycleLoadError;
				simpleCycle.OnLoadFinish += OnCycleLoadFinish;
				simpleCycle.OnUnloadStart += OnCycleUnloadStart;
				simpleCycle.OnUnloadError += OnCycleUnloadError;
				simpleCycle.OnUnloadFinish += OnCycleUnloadFinish;
			}
		}

		private void OnDisable()
		{
			foreach(ISimpleCycleAsync simpleCycle in _simpleCycleList) {
				simpleCycle.OnInitializeStart -= OnCycleInitializeStart;
				simpleCycle.OnInitializeError -= OnCycleInitializeError;
				simpleCycle.OnInitializeFinish -= OnCycleInitializeFinish;
				simpleCycle.OnLoadStart -= OnCycleLoadStart;
				simpleCycle.OnLoadError -= OnCycleLoadError;
				simpleCycle.OnLoadFinish -= OnCycleLoadFinish;
				simpleCycle.OnUnloadStart -= OnCycleUnloadStart;
				simpleCycle.OnUnloadError -= OnCycleUnloadError;
				simpleCycle.OnUnloadFinish -= OnCycleUnloadFinish;
			}
		}
	}
}
