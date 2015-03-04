using System;
using System.Threading;
using System.Threading.Tasks;

namespace Teltec.Storage
{
	public static class AsyncHelper
	{
		public static Task ExecuteOnBackround(Action action)
		{
			return ExecuteOnBackround(action, CancellationToken.None);
		}

		public static Task ExecuteOnBackround(Action action, CancellationToken token)
		{
			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;
			TaskScheduler scheduler = new System.Threading.Tasks.Schedulers.QueuedTaskScheduler(Environment.ProcessorCount);
			//TaskScheduler scheduler = TaskScheduler.Default;
			return Task.Factory.StartNew(action, token, options, scheduler);
		}
	}
}
