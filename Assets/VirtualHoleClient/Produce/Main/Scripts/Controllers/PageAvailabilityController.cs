using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Midnight;
using Midnight.Internet;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using Client.ComponentMaps;

	public class PageAvailabilityController : MonoBehaviour
	{
		[Serializable]
		public class NodeCanvasPair
		{
			public Node node = null;
			public Canvas canvas = null;
		}

		[SerializeField]
		private GameObject[] _simpleCyclesObjects = null;

		[SerializeField]
		private PageDisplayMap _pageDisplayMap = null;

		private List<GameObject> _pageDisplays = new List<GameObject>();
		private List<ISimpleCycleAsync> _simpleCycleList = new List<ISimpleCycleAsync>();
		private CycleLoadParameters _loadParameters = null;

		[Space]
		[SerializeField]
		private NodeCanvasPair[] _nodeCanvasPair = null;

		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private List<Action> OnNodeVisit = new List<Action>();
		private List<Action> OnNodeLeave = new List<Action>();

		private bool _isPageInitialized = false;

		private NodeCanvasPair GetNodeCanvasPair(Node node)
		{
			NodeCanvasPair result = null;
			foreach(NodeCanvasPair pair in _nodeCanvasPair) {
				if(pair.node != node) { continue; }
				result = pair;
				break;
			}
			return result;
		}

		private void SetPairActive(Node node, bool value)
		{
			NodeCanvasPair pair = GetNodeCanvasPair(node);
			SetPairActive(pair, value);
		}

		private void SetPairActive(NodeCanvasPair pair, bool value)
		{
			if(pair.canvas == null) { return; }

			pair.canvas.enabled = value;
			pair.canvas.GetComponent<GraphicRaycaster>().enabled = value;
		}

		private void OnCycleInitializeStart(object data)
		{
			_isPageInitialized = false;

			if(!InternetReachability.isReachable) {
				SetDisplayActive(_pageDisplayMap.internetErrorDisplay, true);
			} 
			//else if(!AppManifestState.isCompatible) {
			//	SetDisplayActive(_pageDisplayMap.appIncompatibleErrorDisplay, true);
			//} 
			else {
				SetDisplayActive(_pageDisplayMap.loadingDisplay.gameObject, true);
				_pageDisplayMap.loadingDisplay.SetProgress(.8f);
			}

			SetPairActive(_mainFlowMap.flowTree.current, false);
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
			SetPairActive(_mainFlowMap.flowTree.current, true);
			_isPageInitialized = true;
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

		private void OnCanvasNodePairVisit(NodeCanvasPair pair)
		{
			if(_isPageInitialized) {
				SetPairActive(pair, true);
			}
		}

		private void OnCanvasNodePairLeave(NodeCanvasPair pair)
		{
			SetPairActive(pair, false);
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

			foreach(NodeCanvasPair pair in _nodeCanvasPair) {
				Action onNodeVisit = () => OnCanvasNodePairVisit(pair);
				Action onNodeLeave = () => OnCanvasNodePairLeave(pair);

				pair.node.OnVisit += onNodeVisit;
				pair.node.OnLeave += onNodeLeave;

				OnNodeVisit.Add(onNodeVisit);
				OnNodeLeave.Add(onNodeLeave);
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

			int index = 0;
			foreach(NodeCanvasPair pair in _nodeCanvasPair) {
				pair.node.OnVisit += OnNodeVisit[index];
				pair.node.OnLeave += OnNodeLeave[index];
				index++;
			}
			OnNodeVisit.Clear();
			OnNodeLeave.Clear();
		}

		private void Start()
		{
			foreach(NodeCanvasPair pair in _nodeCanvasPair) {
				SetPairActive(pair, false);
			}
		}
	}
}
