
using System;
using System.Threading;

namespace Teltec.Storage.Backend
{
	public delegate void TransferStartedDelegate(TransferFileProgressArgs e, Action action = null);
	public delegate void TransferProgressedDelegate(TransferFileProgressArgs e, Action action = null);
	public delegate void TransferFailedDelegate(TransferFileProgressArgs e, Exception ex, Action action = null);
	public delegate void TransferCanceledDelegate(TransferFileProgressArgs e, Exception ex, Action action = null);
	public delegate void TransferCompletedDelegate(TransferFileProgressArgs e, Action action = null);

	public abstract class StorageBackend : IStorageBackend
	{
		#region Upload 

		public TransferStartedDelegate UploadStarted;
		public TransferProgressedDelegate UploadProgressed;
		public TransferFailedDelegate UploadFailed;
		public TransferCanceledDelegate UploadCanceled;
		public TransferCompletedDelegate UploadCompleted;

		public abstract void UploadFile(string filePath, string keyName, CancellationToken cancellationToken);

		#endregion

		#region Download

		public TransferStartedDelegate DownloadStarted;
		public TransferProgressedDelegate DownloadProgressed;
		public TransferFailedDelegate DownloadFailed;
		public TransferCanceledDelegate DownloadCanceled;
		public TransferCompletedDelegate DownloadCompleted;

		public abstract void DownloadFile(string filePath, string keyName, CancellationToken cancellationToken);

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
					//if (obj != null)
					//{
					//	obj.Dispose();
					//	obj = null;
					//}
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
