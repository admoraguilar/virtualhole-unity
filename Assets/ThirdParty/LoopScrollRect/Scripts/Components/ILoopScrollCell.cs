using System;

namespace UnityEngine.UI
{
	public interface ILoopScrollCell
	{
		Type cellDataType { get; }

		void UpdateData(object data);
	}
}
