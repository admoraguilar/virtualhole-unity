using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Midnight
{
	public class UnityWebRequestAwaiter : INotifyCompletion
	{
		public bool IsCompleted { get { return _asyncOp.isDone; } }
		private UnityWebRequestAsyncOperation _asyncOp;

		private Action _continuation;

		public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
		{
			_asyncOp = asyncOp;
			_asyncOp.completed += OnRequestCompleted;
		}
		
		public void GetResult() { }

		public void OnCompleted(Action continuation)
		{
			_continuation = continuation;
		}

		private void OnRequestCompleted(AsyncOperation obj)
		{
			_continuation();
		}
	}
}
