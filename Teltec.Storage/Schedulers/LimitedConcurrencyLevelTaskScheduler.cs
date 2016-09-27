//
// ORIGINAL CODE FROM https://code.msdn.microsoft.com/windowsdesktop/Samples-for-Parallel-b4b76364/sourcecode?fileId=44488&pathId=462437453
// LICENSE: MS-LPL
//

//--------------------------------------------------------------------------
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File: LimitedConcurrencyLevelTaskScheduler.cs
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Monitor = System.Threading.Monitor;

namespace System.Threading.Tasks.Schedulers
{
	/// <summary>
	/// Provides a task scheduler that ensures a maximum concurrency level while
	/// running on top of the ThreadPool.
	/// </summary>
	public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler, IDynamicConcurrencyLevelScheduler
	{
		// Indicates whether the current thread is processing work items.
		[ThreadStatic]
		private static bool _currentThreadIsProcessingItems;

		// The list of tasks to be executed
		private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

		// The maximum concurrency level allowed by this scheduler.
		private long _maxDegreeOfParallelism;

		// Indicates whether the scheduler is currently processing work items.
		private int _delegatesQueuedOrRunning = 0;

		/// <summary>
		/// Initializes an instance of the LimitedConcurrencyLevelTaskScheduler class with the
		/// specified degree of parallelism.
		/// </summary>
		/// <param name="maxDegreeOfParallelism">The maximum degree of parallelism provided by this scheduler.</param>
		public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
		{
			UpdateMaximumConcurrencyLevel(maxDegreeOfParallelism);
		}

		/// <summary>Queues a task to the scheduler.</summary>
		/// <param name="task">The task to be queued.</param>
		protected sealed override void QueueTask(Task task)
		{
			// Add the task to the list of tasks to be processed.  If there aren't enough
			// delegates currently queued or running to process tasks, schedule another.
			lock (_tasks)
			{
				_tasks.AddLast(task);

				if (_delegatesQueuedOrRunning < MaximumConcurrencyLevel)
				{
					++_delegatesQueuedOrRunning;
					NotifyThreadPoolOfPendingWork();
				}
			}
		}

		public void RemovePendingTasks()
		{
			lock (_tasks)
			{
				while (true)
				{
					// When there are no more items to be processed,
					// note that we're done processing, and get out.
					if (_tasks.Count == 0)
						break;

					// Get the next item from the queue
					Task task = _tasks.First.Value;
					switch (task.Status)
					{
						// The task has been initialized but has not yet been scheduled.
						case TaskStatus.Created:
						// The task is waiting to be activated and scheduled internally.
						case TaskStatus.WaitingForActivation:
						// The task has been scheduled for execution but has not yet begun executing.
						case TaskStatus.WaitingToRun:
							_tasks.RemoveFirst();
							break;
						case TaskStatus.Running:
						case TaskStatus.RanToCompletion:
						case TaskStatus.Faulted:
						case TaskStatus.Canceled:
						case TaskStatus.WaitingForChildrenToComplete:
							// Do nothing. The task execution just finishes or is underway.
							break;
					}
				}
			}
		}

		/// <summary>
		/// Informs the ThreadPool that there's work to be executed for this scheduler.
		/// </summary>
		private void NotifyThreadPoolOfPendingWork()
		{
			ThreadPool.UnsafeQueueUserWorkItem(_ =>
			{
				// Note that the current thread is now processing work items.
				// This is necessary to enable inlining of tasks into this thread.
				_currentThreadIsProcessingItems = true;
				try
				{
					// Process all available items in the queue.
					while (true)
					{
						Task item;
						lock (_tasks)
						{
							// When there are no more items to be processed,
							// note that we're done processing, and get out.
							if (_tasks.Count == 0)
							{
								--_delegatesQueuedOrRunning;
								break;
							}

							// Get the next item from the queue
							item = _tasks.First.Value;
							_tasks.RemoveFirst();
						}

						// Execute the task we pulled out of the queue
						base.TryExecuteTask(item);
					}
				}
				// We're done processing items on the current thread
				finally
				{
					_currentThreadIsProcessingItems = false;
				}
			}, null);
		}

		/// <summary>Attempts to execute the specified task on the current thread.</summary>
		/// <param name="task">The task to be executed.</param>
		/// <param name="taskWasPreviouslyQueued"></param>
		/// <returns>Whether the task could be executed on the current thread.</returns>
		protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			// If this thread isn't already processing a task, we don't support inlining
			if (!_currentThreadIsProcessingItems)
				return false;

			// If the task was previously queued, remove it from the queue
			if (taskWasPreviouslyQueued)
			{
				// Try to run the task.
				if (TryDequeue(task))
					return base.TryExecuteTask(task);
				else
					return false;
			}
			else
			{
				return base.TryExecuteTask(task);
			}
		}

		/// <summary>Attempts to remove a previously scheduled task from the scheduler.</summary>
		/// <param name="task">The task to be removed.</param>
		/// <returns>Whether the task could be found and removed.</returns>
		protected sealed override bool TryDequeue(Task task)
		{
			lock (_tasks)
				return _tasks.Remove(task);
		}

		/// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
		public sealed override int MaximumConcurrencyLevel
		{
			get
			{
				return (int)Interlocked.Read(ref _maxDegreeOfParallelism);
			}
		}

		public void UpdateMaximumConcurrencyLevel(int value)
		{
			if (value < 1)
				throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
			Interlocked.Exchange(ref _maxDegreeOfParallelism, value);
		}

		/// <summary>Gets an enumerable of the tasks currently scheduled on this scheduler.</summary>
		/// <returns>An enumerable of the tasks currently scheduled.</returns>
		protected sealed override IEnumerable<Task> GetScheduledTasks()
		{
			bool lockTaken = false;
			try
			{
				Monitor.TryEnter(_tasks, ref lockTaken);
				if (lockTaken)
					return _tasks;
				else
					throw new NotSupportedException();
			}
			finally
			{
				if (lockTaken)
					Monitor.Exit(_tasks);
			}
		}
	}
}
