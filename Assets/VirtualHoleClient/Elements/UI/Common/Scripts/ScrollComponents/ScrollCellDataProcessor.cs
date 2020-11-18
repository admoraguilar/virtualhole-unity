using System;

namespace UnityEngine.UI
{
	public abstract class ScrollCellDataProcessor<TObject, TData> : ScrollCellDataProcessor
		where TObject : Component
	{
		public TObject[] prefabs = null;

		public override GameObject prefab => prefabs[0].gameObject;
		public sealed override Type dataType => typeof(TData);

		public sealed override void ProcessData(object instance, object data)
		{
			ProcessData(((GameObject)instance).GetComponent<TObject>(), (TData)data);
		}

		public abstract void ProcessData(TObject instance, TData data);
	}

	public abstract class ScrollCellDataProcessor : ScriptableObject
	{
		public abstract GameObject prefab { get; }
		public abstract Type dataType { get; }

		public abstract void ProcessData(object instance, object data);
	}
}
