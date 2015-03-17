using System;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Agent
{
	public abstract class AsyncTransferAgent : IAsyncTransferAgent
	{
		protected AsyncTransferAgent(StorageBackend impl)
		{
			RenewCancellationToken();

			Implementation = impl;

			// Forward-proxy all events.
			EventDispatcher = new EventDispatcher();
			RegisterDelegates();
		}

		#region Delegates

		private void RegisterDelegates()
		{
			RegisterUploadDelegates();
			RegisterDownloadDelegates();
		}

		private void RegisterUploadDelegates()
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

		private void RegisterDownloadDelegates()
		{
			// We do all this magic because other threads should NOT change the UI.
			// We want the changes made to the reusable `TransferFileProgressArgs` instance
 			// to raise change events on the context of the Main thread.
			Implementation.DownloadStarted += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DownloadFileStarted != null)
						DownloadFileStarted.Invoke(this, args);
				});
			};
			Implementation.DownloadProgressed += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DownloadFileProgress != null)
						DownloadFileProgress.Invoke(this, args);
				});
			};
			Implementation.DownloadCanceled += (TransferFileProgressArgs args, Exception ex, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DownloadFileCanceled != null)
						DownloadFileCanceled.Invoke(this, args, ex);
				});
			};
			Implementation.DownloadFailed += (TransferFileProgressArgs args, Exception ex, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DownloadFileFailed != null)
						DownloadFileFailed.Invoke(this, args, ex);
				});
			};
			Implementation.DownloadCompleted += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DownloadFileCompleted != null)
						DownloadFileCompleted.Invoke(this, args);
				});
			};
		}

		#endregion

		protected Task ExecuteOnBackround(Action action)
		{
			return AsyncHelper.ExecuteOnBackround(action);
		}

		protected StorageBackend Implementation; // IDisposable
		protected CancellationTokenSource CancellationTokenSource; // IDisposable

		#region IAsyncTransferAgent

		public EventDispatcher EventDispatcher { get; set; }

		public IPathBuilder PathBuilder { get; set; }

		public string LocalRootDir { get; set; }

		private string _RemoteRootDir;
		public string RemoteRootDir
		{
			get { return _RemoteRootDir;  }
			set
			{
				_RemoteRootDir = value;
				if (PathBuilder != null)
					// The `RootDirectory` of the path builder should always
					// be our `RemoteRootDir` (not `LocalRootDir`) because
					// versioning only makes sense on the remote repository.
					PathBuilder.RootDirectory = RemoteRootDir;
			}
		}

		#region Upload

		public event TransferFileProgressHandler UploadFileStarted;
		public event TransferFileProgressHandler UploadFileProgress;
		public event TransferFileExceptionHandler UploadFileCanceled;
		public event TransferFileExceptionHandler UploadFileFailed;
		public event TransferFileProgressHandler UploadFileCompleted;

		abstract public Task UploadVersionedFile(string sourcePath, IFileVersion version);
		abstract public Task UploadFile(string sourcePath, string targetPath);

		#endregion

		#region Download

		public event TransferFileProgressHandler DownloadFileStarted;
		public event TransferFileProgressHandler DownloadFileProgress;
		public event TransferFileExceptionHandler DownloadFileCanceled;
		public event TransferFileExceptionHandler DownloadFileFailed;
		public event TransferFileProgressHandler DownloadFileCompleted;

		abstract public Task DownloadVersionedFile(string sourcePath, IFileVersion version);
		abstract public Task DownloadFile(string sourcePath, string targetPath);

		#endregion

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

		#endregion

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
}
