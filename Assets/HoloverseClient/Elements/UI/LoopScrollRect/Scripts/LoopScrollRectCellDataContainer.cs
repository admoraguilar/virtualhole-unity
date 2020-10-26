using System;
using System.Collections.Generic;
using UnityEngine;

namespace Holoverse.Client.UI
{
	public class LoopScrollRectCellDataContainer : MonoBehaviour
	{
		public event Action<IReadOnlyList<object>> OnDataUpdated = delegate { };

		public IReadOnlyList<object> data => _data;
		private List<object> _data = new List<object>();

		public void UpdateData(IEnumerable<object> values)
		{
			_data.Clear();
			_data.AddRange(values);

			OnDataUpdated(_data);
		}
	}
}
