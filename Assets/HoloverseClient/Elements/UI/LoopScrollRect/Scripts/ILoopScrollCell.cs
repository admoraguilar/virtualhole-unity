using System;
using UnityEngine;

namespace Holoverse.Client.UI
{
	public interface ILoopScrollCell
	{
		RectTransform rectTrasform { get; }
		Type cellDataType { get; }

		void UpdateData(object data);
	}
}
