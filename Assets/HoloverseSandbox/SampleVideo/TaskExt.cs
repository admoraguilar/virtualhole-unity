using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Midnight
{
	public static class TaskExt
	{
		private static readonly TaskFactory _taskFactory = new TaskFactory(
			CancellationToken.None,
			TaskCreationOptions.None,
			TaskContinuationOptions.None,
			TaskScheduler.Default
		);

		public static async void FireForget(
			Task task, Action<Exception> onException = null, 
			bool isContinueOnCapturedContext = true)
		{
			try {
				await task.ConfigureAwait(isContinueOnCapturedContext);
			} catch(Exception e) {
				Debug.LogError(e);
			}
		}

		public static void RunSync(Func<Task> task)
		{
			_taskFactory.StartNew(task).Unwrap().GetAwaiter().GetResult();
		}

		public static TResult RunSync<TResult>(Func<Task<TResult>> task)
		{
			return _taskFactory.StartNew(task).Unwrap().GetAwaiter().GetResult();
		}
	}
}
