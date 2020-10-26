using UnityEngine.Assertions;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public abstract class LoopScrollDataSource
    {
        public abstract void ProvideData(Transform transform, int index);
    }

	public interface ILoopScrollIndexReceiver
	{
		void ScrollCellIndex(int index);
	}

    public class LoopScrollSendIndexSource : LoopScrollDataSource
    {
        public static readonly LoopScrollSendIndexSource Instance = new LoopScrollSendIndexSource();
		private static string _noReceiverError => $"{nameof(LoopScrollSendIndexSource)} Loop scroll cell doesn't have an {nameof(ILoopScrollIndexReceiver)}!";

		private Dictionary<int, ILoopScrollIndexReceiver> _indexReceiverLookup = new Dictionary<int, ILoopScrollIndexReceiver>();

        public LoopScrollSendIndexSource() { }

        public override void ProvideData(Transform transform, int index)
        {
			Assert.IsNotNull(transform);

			ILoopScrollIndexReceiver receiver = LoopScrollRectUtilities.GetComponentFromLookup(_indexReceiverLookup, transform);
			if(receiver != null) { receiver.ScrollCellIndex(index); } 
        }
    }

	public interface ILoopScrollContentReceiver
	{
		void ScrollCellContent(object value);
	}

    public class LoopScrollArraySource<T> : LoopScrollDataSource
    {
		private static string _noReceiverError => $"{nameof(LoopScrollArraySource<T>)} Loop scroll cell doesn't have an {nameof(ILoopScrollContentReceiver)}!";

		private Dictionary<int, ILoopScrollContentReceiver> _contentReceiverLookup = new Dictionary<int, ILoopScrollContentReceiver>();
        private T[] objectsToFill;

        public LoopScrollArraySource(T[] objectsToFill)
        {
            this.objectsToFill = objectsToFill;
        }

        public override void ProvideData(Transform transform, int index)
        {
			Assert.IsNotNull(transform);

			ILoopScrollContentReceiver receiver = LoopScrollRectUtilities.GetComponentFromLookup(_contentReceiverLookup, transform);
			if(receiver != null) { receiver.ScrollCellContent(index); }
        }
    }
}