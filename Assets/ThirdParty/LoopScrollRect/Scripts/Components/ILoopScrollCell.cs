using System;

namespace UnityEngine.UI
{
	public interface ILoopScrollCell
	{
		Type dataType { get; }

		void SetData(object content);
	}
}
