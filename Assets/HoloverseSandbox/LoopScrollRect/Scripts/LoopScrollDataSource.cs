using UnityEngine.Assertions;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public abstract class LoopScrollDataSource
    {
        public abstract void ProvideData(Transform transform, int index);

		protected T GetCellInstanceFromLookup<T>(Dictionary<int, T> lookup, Transform transform)
		{
			int instanceId = transform.GetInstanceID();
			if(!lookup.TryGetValue(instanceId, out T value)) {
				if(transform.TryGetComponent(out value)) {
					lookup[instanceId] = value;
				}
			}
			return value;
		}
    }

	public interface ILoopScrollIndexReceiver
	{
		void ScrollCellIndex(int index);
	}

    public class LoopScrollSendIndexSource : LoopScrollDataSource
    {
        public static readonly LoopScrollSendIndexSource Instance = new LoopScrollSendIndexSource();
		private static string _noReceiverError => $"{nameof(LoopScrollSendIndexSource)} Loop scroll cell doesn't have an {nameof(ILoopScrollIndexReceiver)}!";

		private Dictionary<int, ILoopScrollIndexReceiver> _loopIndexReceiverLookup = new Dictionary<int, ILoopScrollIndexReceiver>();

        public LoopScrollSendIndexSource() { }

        public override void ProvideData(Transform transform, int index)
        {
			Assert.IsNotNull(transform);

			ILoopScrollIndexReceiver receiver = GetCellInstanceFromLookup(_loopIndexReceiverLookup, transform);
			if(receiver != null) { receiver.ScrollCellIndex(index); } 
			else { Debug.LogError(_noReceiverError); }
        }
    }

	public interface ILoopScrollContentReceiver
	{
		void ScrollCellContent(object value);
	}

    public class LoopScrollArraySource<T> : LoopScrollDataSource
    {
		private static string _noReceiverError => $"{nameof(LoopScrollArraySource<T>)} Loop scroll cell doesn't have an {nameof(ILoopScrollContentReceiver)}!";

		private Dictionary<int, ILoopScrollContentReceiver> _loopContentReceiverLookup = new Dictionary<int, ILoopScrollContentReceiver>();
        private T[] objectsToFill;

        public LoopScrollArraySource(T[] objectsToFill)
        {
            this.objectsToFill = objectsToFill;
        }

        public override void ProvideData(Transform transform, int index)
        {
			Assert.IsNotNull(transform);

			ILoopScrollContentReceiver receiver = GetCellInstanceFromLookup(_loopContentReceiverLookup, transform);
			if(receiver != null) { receiver.ScrollCellContent(index); }
			else { Debug.LogError(_noReceiverError); }
        }
    }
}