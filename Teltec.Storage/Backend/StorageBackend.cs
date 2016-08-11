using System;
using System.Collections.Generic;
using System.Threading;

namespace Teltec.Storage.Backend
{
	public delegate void TransferStartedDelegate(TransferFileProgressArgs e, Action action = null);
	public delegate void TransferProgressedDelegate(TransferFileProgressArgs e, Action action = null);
	public delegate void TransferFailedDelegate(TransferFileProgressArgs e, Action action = null);
	public delegate void TransferCanceledDelegate(TransferFileProgressArgs e, Action action = null);
	public delegate void TransferCompletedDelegate(TransferFileProgressArgs e, Action action = null);

	public delegate void ListingStartedDelegate(ListingProgressArgs e, Action action = null);
	public delegate void ListingProgressedDelegate(ListingProgressArgs e, Action action = null);
	public delegate void ListingFailedDelegate(ListingProgressArgs e, Action action = null);
	public delegate void ListingCanceledDelegate(ListingProgressArgs e, Action action = null);
	public delegate void ListingCompletedDelegate(ListingProgressArgs e, Action action = null);

	public delegate void DeletionStartedDelegate(DeletionArgs e, Action action = null);
	public delegate void DeletionFailedDelegate(DeletionArgs e, Action action = null);
	public delegate void DeletionCanceledDelegate(DeletionArgs e, Action action = null);
	public delegate void DeletionCompletedDelegate(DeletionArgs e, Action action = null);

	public abstract class StorageBackend : IStorageBackend
	{
		#region Upload

		public TransferStartedDelegate UploadStarted;
		public TransferProgressedDelegate UploadProgressed;
		public TransferFailedDelegate UploadFailed;
		public TransferCanceledDelegate UploadCanceled;
		public TransferCompletedDelegate UploadCompleted;

		public abstract void UploadFile(string filePath, string keyName, object userData, CancellationToken cancellationToken);

		#endregion

		#region Download

		public TransferStartedDelegate DownloadStarted;
		public TransferProgressedDelegate DownloadProgressed;
		public TransferFailedDelegate DownloadFailed;
		public TransferCanceledDelegate DownloadCanceled;
		public TransferCompletedDelegate DownloadCompleted;

		public abstract void DownloadFile(string filePath, string keyName, object userData, CancellationToken cancellationToken);

		#endregion

		#region Listing

		public ListingStartedDelegate ListingStarted;
		public ListingProgressedDelegate ListingProgressed;
		public ListingFailedDelegate ListingFailed;
		public ListingCanceledDelegate ListingCanceled;
		public ListingCompletedDelegate ListingCompleted;

		public abstract void List(string prefix, bool recursive, object userData, CancellationToken cancellationToken);

		#endregion

		#region Deletion

		public DeletionStartedDelegate DeletionStarted;
		public DeletionFailedDelegate DeletionFailed;
		public DeletionCanceledDelegate DeletionCanceled;
		public DeletionCompletedDelegate DeletionCompleted;

		public abstract void DeleteFile(string keyName, object userData, CancellationToken cancellationToken);
		public abstract void DeleteMultipleFiles(List<Tuple<string, object>> keyNamesAndUserData, CancellationToken cancellationToken);

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
