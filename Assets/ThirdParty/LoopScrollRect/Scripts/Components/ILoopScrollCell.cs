using System;

namespace UnityEngine.UI
{
	public interface ILoopScrollCell
	{
		Type cellDataType { get; }

		RectTransform rectTrasform { get; }
		LayoutElement layoutElement { get; }
		
		void UpdateData(object data);
	}
}
