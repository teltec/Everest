using System;
using System.Collections.Generic;
using System.Threading;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Backend
{
	public abstract class TransferAgent : ITransferAgent
	{
		protected TransferAgentOptions Options { get; private set; }

		protected TransferAgent(TransferAgentOptions options, StorageBackend impl, CancellationToken cancellationToken)
		{
			_shouldDispose = false;

			Options = options;
			Implementation = impl; // This should be disposed by classe that inherit this class.

			CancellationToken = cancellationToken;

			// Forward-proxy all events.
			EventDispatcher = new EventDispatcher();
			RegisterDelegates();
		}

		#region Delegates

		private void RegisterDelegates()
		{
			RegisterUploadDelegates();
			RegisterDownloadDelegates();
			RegisterListingDelegates();
			RegisterDeletionDelegates();
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
			Implementation.UploadCanceled += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (UploadFileCanceled != null)
						UploadFileCanceled.Invoke(this, args);
				});
			};
			Implementation.UploadFailed += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (UploadFileFailed != null)
						UploadFileFailed.Invoke(this, args);
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
			Implementation.DownloadCanceled += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DownloadFileCanceled != null)
						DownloadFileCanceled.Invoke(this, args);
				});
			};
			Implementation.DownloadFailed += (TransferFileProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DownloadFileFailed != null)
						DownloadFileFailed.Invoke(this, args);
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

		private void RegisterListingDelegates()
		{
			// We do all this magic because other threads should NOT change the UI.
			// We want the changes made to the reusable `ListingProgressArgs` instance
			// to raise change events on the context of the Main thread.
			Implementation.ListingStarted += (ListingProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (ListingStarted != null)
						ListingStarted.Invoke(this, args);
				});
			};
			Implementation.ListingProgressed += (ListingProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (ListingProgress != null)
						ListingProgress.Invoke(this, args);
				});
			};
			Implementation.ListingCanceled += (ListingProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (ListingCanceled != null)
						ListingCanceled.Invoke(this, args);
				});
			};
			Implementation.ListingFailed += (ListingProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (ListingFailed != null)
						ListingFailed.Invoke(this, args);
				});
			};
			Implementation.ListingCompleted += (ListingProgressArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (ListingCompleted != null)
						ListingCompleted.Invoke(this, args);
				});
			};
		}

		private void RegisterDeletionDelegates()
		{
			// We do all this magic because other threads should NOT change the UI.
			// We want the changes made to the reusable `DeletionArgs` instance
			// to raise change events on the context of the Main thread.
			Implementation.DeletionStarted += (DeletionArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DeleteFileStarted != null)
						DeleteFileStarted.Invoke(this, args);
				});
			};
			Implementation.DeletionCanceled += (DeletionArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DeleteFileCanceled != null)
						DeleteFileCanceled.Invoke(this, args);
				});
			};
			Implementation.DeletionFailed += (DeletionArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DeleteFileFailed != null)
						DeleteFileFailed.Invoke(this, args);
				});
			};
			Implementation.DeletionCompleted += (DeletionArgs args, Action action) =>
			{
				EventDispatcher.Invoke(() =>
				{
					if (action != null)
						action.Invoke();
					if (DeleteFileCompleted != null)
						DeleteFileCompleted.Invoke(this, args);
				});
			};
		}

		#endregion

		protected StorageBackend Implementation; // IDisposable
		protected readonly CancellationToken CancellationToken;

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
					PathBuilder.RemoteRootDirectory = RemoteRootDir;
			}
		}

		#region Upload

		public event TransferFileProgressHandler UploadFileStarted;
		public event TransferFileProgressHandler UploadFileProgress;
		public event TransferFileExceptionHandler UploadFileCanceled;
		public event TransferFileExceptionHandler UploadFileFailed;
		public event TransferFileProgressHandler UploadFileCompleted;

		abstract public void UploadVersionedFile(string sourcePath, IFileVersion version, object userData);
		abstract public void UploadFile(string sourcePath, string targetPath, object userData);

		#endregion

		#region Download

		public event TransferFileProgressHandler DownloadFileStarted;
		public event TransferFileProgressHandler DownloadFileProgress;
		public event TransferFileExceptionHandler DownloadFileCanceled;
		public event TransferFileExceptionHandler DownloadFileFailed;
		public event TransferFileProgressHandler DownloadFileCompleted;

		abstract public void DownloadVersionedFile(string sourcePath, IFileVersion version, object userData);
		abstract public void DownloadFile(string sourcePath, string targetPath, object userData);

		#endregion

		#region Listing

		public event ListingProgressHandler ListingStarted;
		public event ListingProgressHandler ListingProgress;
		public event ListingExceptionHandler ListingCanceled;
		public event ListingExceptionHandler ListingFailed;
		public event ListingProgressHandler ListingCompleted;

		abstract public void List(string prefix, bool recursive, object userData);

		#endregion

		#region Deletion

		public event DeleteFileProgressHandler DeleteFileStarted;
		public event DeleteFileExceptionHandler DeleteFileCanceled;
		public event DeleteFileExceptionHandler DeleteFileFailed;
		public event DeleteFileProgressHandler DeleteFileCompleted;

		abstract public void DeleteVersionedFile(string sourcePath, IFileVersion version, object userData);
		abstract public void DeleteMultipleVersionedFile(List<Tuple<string /*sourcePath*/, IFileVersion /*version*/, object /*userData*/>> files);

		#endregion

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
					// Do nothing.
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
