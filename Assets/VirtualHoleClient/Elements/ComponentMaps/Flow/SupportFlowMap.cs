﻿using UnityEngine;
using VirtualHole.Client.UI;

namespace VirtualHole.Client.ComponentMaps
{
	public class SupportFlowMap : MonoBehaviour
	{
		public SupportView supportView => _supportView;
		[SerializeField]
		private SupportView _supportView = null;
	}
}
