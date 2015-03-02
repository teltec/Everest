using System;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Storage.Backend;

namespace Teltec.Storage.Agent
{
	/*
	public class TransferException : Exception
	{
		public TransferFileProgressArgs Args;
		public TransferException(string message, Exception innerException, TransferFileProgressArgs args)
			: base(message, innerException)
		{
			Args = args;
		}
	}

	public class TransferCanceledException : TaskCanceledException
	{
		public TransferFileProgressArgs Args;
		public TransferCanceledException(string message, Exception innerException, TransferFileProgressArgs args)
			: base(message, innerException)
		{
			Args = args;
		}
	}
	*/

	// ATTENTION: If an event listener performs changes to the UI, then the provided dispatcher
	//            MUST have been created on the Main thread.
	// The reason is that this implementation raises events through the provided dispatcher,
	// and every change in transfer progress causes an event be raised and propagated.
	// An UI element might be have registered a binding to the event instance, which is unique
	// and reused during the lifetime of a transfer.
	public abstract class AsyncTransferAgent : IAsyncTransferAgent
	{
		protected StorageBackend Implementation; // IDisposable
		protected CancellationTokenSource CancellationTokenSource; // IDisposable
		protected EventDispatcher EventDispatcher;

		public string LocalRootDir { get; set; }
		public string RemoteRootDir { get; set; }

		public event TransferFileProgressHandler UploadFileStarted;
		public event TransferFileProgressHandler UploadFileProgress;
		public event TransferFileExceptionHandler UploadFileCanceled;
		public event TransferFileExceptionHandler UploadFileFailed;
		public event TransferFileProgressHandler UploadFileCompleted;

		protected AsyncTransferAgent(EventDispatcher dispatcher, StorageBackend impl)
		{
			RenewCancellationToken();

			Implementation = impl;

			// Forward-proxy all events.
			EventDispatcher = dispatcher;
			RegisterDelegates();
		}

		private void RegisterDelegates()
		{
			// We do all this magic because other threads should NOT change the UI.
			// We want the changes made to the reusable `TransferFileProgressArgs` instance
 			// to raise change events on the context of the Main thread.
			Implementation.UploadStarted += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (UploadFileStarted != null)
						UploadFileStarted.Invoke(this, args);
				});
			};
			Implementation.UploadProgressed += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (UploadFileProgress != null)
						UploadFileProgress.Invoke(this, args);
				});
			};
			Implementation.UploadCanceled += (TransferFileProgressArgs args, Exception ex, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (UploadFileCanceled != null)
						UploadFileCanceled.Invoke(this, args, ex);
				});
			};
			Implementation.UploadFailed += (TransferFileProgressArgs args, Exception ex, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (UploadFileFailed != null)
						UploadFileFailed.Invoke(this, args, ex);
				});
			};
			Implementation.UploadCompleted += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (UploadFileCompleted != null)
						UploadFileCompleted.Invoke(this, args);
				});
			};
		}

		abstract public Task UploadFile(string sourcePath);
		abstract public Task UploadFile(string sourcePath, string targetPath);

		abstract public Task DownloadFile(string sourcePath);
		abstract public Task DownloadFile(string sourcePath, string targetPath);

		public void CancelTransfers()
		{
			CancellationTokenSource.Cancel();
		}

		public void RenewCancellationToken()
		{
			bool alreadyUsed = CancellationTokenSource != null && CancellationTokenSource.IsCancellationRequested;
			if (alreadyUsed || CancellationTokenSource == null)
			{
				if (CancellationTokenSource != null)
					CancellationTokenSource.Dispose();

				CancellationTokenSource = new CancellationTokenSource();
			}
		}

		protected Task ExecuteOnBackround(Action action)
		{
			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;
			CancellationToken token = CancellationToken.None;
			TaskScheduler scheduler = new System.Threading.Tasks.Schedulers.QueuedTaskScheduler(Environment.ProcessorCount);
			//TaskScheduler scheduler = TaskScheduler.Default;
			return Task.Factory.StartNew(action, token, options, scheduler);
		}

		#region Dispose Pattern Implementation

		bool _shouldDispose = false;
		bool _isDisposed;

		/// <summary>
		/// Implements the Dispose pattern
		/// </summary>
		/// <param name="disposing">Whether this object is being disposed via a call to Dispose
		/// or garbage collected.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					if (CancellationTokenSource != null)
					{
						CancellationTokenSource.Dispose();
						CancellationTokenSource = null;
					}
				}
				this._isDisposed = true;
			}
		}

		/// <summary>
		/// Disposes of all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
