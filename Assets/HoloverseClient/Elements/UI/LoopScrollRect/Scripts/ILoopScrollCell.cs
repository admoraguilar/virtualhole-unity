using System;
using UnityEngine;
using UnityEngine.UI;

namespace Holoverse.Client.UI
{
	public interface ILoopScrollCell
	{
		RectTransform rectTrasform { get; }
		LayoutElement layoutElement { get; }
		Type cellDataType { get; }

		void UpdateData(object data);
	}
}
