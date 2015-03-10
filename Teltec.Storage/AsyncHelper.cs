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
			// IMPORTANT: Using `Task.Factory.StartNew` with `DenyChildAttach` always
			//			  reports `IsCompleted` rather than `IsFaulted` when an exception
			//			  is thrown from inside the task.
			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;
			TaskScheduler scheduler = new System.Threading.Tasks.Schedulers.QueuedTaskScheduler(Environment.ProcessorCount);
			//TaskScheduler scheduler = TaskScheduler.Default;
			return Task.Factory.StartNew(action, token, options, scheduler);
			//return Task.Run(action, token);
		}
		
		public static Task<T> ExecuteOnBackround<T>(Func<T> action, CancellationToken token)
		{
			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;
			TaskScheduler scheduler = new System.Threading.Tasks.Schedulers.QueuedTaskScheduler(Environment.ProcessorCount);
			//TaskScheduler scheduler = TaskScheduler.Default;
			return Task.Factory.StartNew<T>(action, token, options, scheduler);
		}

		public static Task Continue(this Task task, Action action)
		{
			if (!task.IsFaulted)
			{
				task.ContinueWith((t) =>
					action(),
					TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);
			}
			return task;
		}

		public static Task<T> Continue<T>(this Task<T> task, Action<T> action)
		{
			if (!task.IsFaulted)
			{
				task.ContinueWith((t) =>
					action(t.Result),
					TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);
			}
			return task;
		}

		public static Task OnException(this Task task, Action<Exception> onFaulted)
		{
			task.ContinueWith(t =>
			{
				var excetion = t.Exception;
				onFaulted(excetion);
			}, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
			return task;
		}
	}
}
