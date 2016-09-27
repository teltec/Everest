/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace Teltec.Storage
{
	public static class AsyncHelper
	{
		private static int _SettingsMaxThreadCount;
		public static int SettingsMaxThreadCount
		{
			get
			{
				return _SettingsMaxThreadCount;
			}
			set
			{
				if (value < 1 || value > 256)
					throw new ArgumentException("value must be within 1-256 range", "SettingsMaxThreadCount");

				_SettingsMaxThreadCount = value;

				if (AsyncHelper.TaskSchedulerInstance is IDynamicConcurrencyLevelScheduler)
				{
					IDynamicConcurrencyLevelScheduler scheduler = AsyncHelper.TaskSchedulerInstance as IDynamicConcurrencyLevelScheduler;
					scheduler.UpdateMaximumConcurrencyLevel(_SettingsMaxThreadCount);
				}
			}
		}

		public static TaskScheduler _TaskSchedulerInstance;
		public static TaskScheduler TaskSchedulerInstance
		{
			get
			{
				int threadCount = SettingsMaxThreadCount;
				if (_TaskSchedulerInstance == null)
					//_TaskSchedulerInstance = new System.Threading.Tasks.Schedulers.QueuedTaskScheduler(threadCount, "TaskExecutor");
					_TaskSchedulerInstance = new System.Threading.Tasks.Schedulers.LimitedConcurrencyLevelTaskScheduler(threadCount);
				return _TaskSchedulerInstance;
			}
		}

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
			TaskScheduler scheduler = TaskSchedulerInstance;
			//TaskScheduler scheduler = TaskScheduler.Default;
			return Task.Factory.StartNew(action, token, options, scheduler);
			//return Task.Run(action, token);
		}

		public static Task<T> ExecuteOnBackround<T>(Func<T> action, CancellationToken token)
		{
			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;
			TaskScheduler scheduler = TaskSchedulerInstance;
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
				var exception = t.Exception;
				onFaulted(exception);
			}, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
			return task;
		}
	}
}
