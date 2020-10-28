﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.SOM;
using Midnight.Mobile;
using Midnight.FlowTree;

namespace Holoverse.Client.Controllers
{
	using Client.UI;
	using Client.SOM;

	public class MainFlowNavigator : MonoBehaviour
	{
		[Serializable]
		public class NavigationItem
		{
			public Sprite sprite = null;
			public string text = null;
			public Node node = null;
		}

		[Header("Navigations")]
		[SerializeField]
		private NavigationItem[] _navigationItems = null;

		private SceneObjectModel _som = null;
		private FlowTree _flowTree = null;
		private NavigationBar _navigationBar = null;

		private void OnBackButtonClicked()
		{
			if(_flowTree.isLessThanOrOneNode) { MobileApplication.Suspend(); } 
			else { _flowTree.Backward(); }
		}

		private void Awake()
		{
			_som = SceneObjectModel.Get(this);
			_flowTree = _som.GetCachedComponent<MainFlowMap>().flowTree;
			_navigationBar = _som.GetCachedComponent<ControlsMap>().navigationBar;
		}

		private void Start()
		{
			List<NavigationBar.ItemData> itemData = new List<NavigationBar.ItemData>();
			foreach(NavigationItem item in _navigationItems) {
				itemData.Add(new NavigationBar.ItemData {
					sprite = item.sprite,
					text = item.text,
					onClick = item.node.Set
				});
			}
			_navigationBar.UpdateEntries(itemData);
		}

		private void Update()
		{
			// We hide the back button when there's no history on iOS
			// because there's no such thing as manual app suspension in
			// iOS besides pressing the home button:
			// https://answers.unity.com/questions/42608/exit-function-for-iphone-or-android-app.html
			// https://docs.unity3d.com/ScriptReference/Application.Quit.html
#if !UNITY_EDITOR && UNITY_IOS
			if(_flowTree.isLessThanOrOneNode) {
				if(_navigationBar.backButton.gameObject.activeSelf) {
					_navigationBar.backButton.gameObject.SetActive(false);
				}
			} else {
				if(!_navigationBar.backButton.gameObject.activeSelf) {
					_navigationBar.backButton.gameObject.SetActive(true);
				}
			}
#endif
		}

		private void OnEnable()
		{
			_navigationBar.backButton.onClick.AddListener(OnBackButtonClicked);
		}

		private void OnDisable()
		{
			_navigationBar.backButton.onClick.RemoveListener(OnBackButtonClicked);
		}
	}
}
