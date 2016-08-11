using System;
using System.Collections.Generic;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Backend
{
	public delegate void TransferFileProgressHandler(object sender, TransferFileProgressArgs e);
	public delegate void TransferFileExceptionHandler(object sender, TransferFileProgressArgs e);
	public delegate void ListingProgressHandler(object sender, ListingProgressArgs e);
	public delegate void ListingExceptionHandler(object sender, ListingProgressArgs e);
	public delegate void DeleteFileProgressHandler(object sender, DeletionArgs e);
	public delegate void DeleteFileExceptionHandler(object sender, DeletionArgs e);

	public interface ITransferAgent : IDisposable
	{
		// ATTENTION: If an event listener performs changes to the UI, then the provided dispatcher
		//            MUST have been created on the Main thread.
		// The reason is that this implementation raises events through the provided dispatcher,
		// and every change in transfer progress causes an event be raised and propagated.
		// An UI element might be have registered a binding to the event instance, which is unique
		// and reused during the lifetime of a transfer.
		EventDispatcher EventDispatcher { get; set; }

		IPathBuilder PathBuilder { get; set; }

		string LocalRootDir { get; set; }
		string RemoteRootDir { get; set; }

		#region Upload

		event TransferFileProgressHandler UploadFileStarted;
		event TransferFileProgressHandler UploadFileProgress;
		event TransferFileExceptionHandler UploadFileCanceled;
		event TransferFileExceptionHandler UploadFileFailed;
		event TransferFileProgressHandler UploadFileCompleted;

		void UploadVersionedFile(string sourcePath, IFileVersion version, object userData);
		void UploadFile(string sourcePath, string targetPath, object userData);

		#endregion

		#region Download

		event TransferFileProgressHandler DownloadFileStarted;
		event TransferFileProgressHandler DownloadFileProgress;
		event TransferFileExceptionHandler DownloadFileCanceled;
		event TransferFileExceptionHandler DownloadFileFailed;
		event TransferFileProgressHandler DownloadFileCompleted;

		void DownloadVersionedFile(string sourcePath, IFileVersion version, object userData);
		void DownloadFile(string sourcePath, string targetPath, object userData);

		#endregion

		#region Listing

		event ListingProgressHandler ListingStarted;
		event ListingProgressHandler ListingProgress;
		event ListingExceptionHandler ListingCanceled;
		event ListingExceptionHandler ListingFailed;
		event ListingProgressHandler ListingCompleted;

		void List(string prefix, bool recursive, object userData);

		#endregion

		#region Deletion

		event DeleteFileProgressHandler DeleteFileStarted;
		event DeleteFileExceptionHandler DeleteFileCanceled;
		event DeleteFileExceptionHandler DeleteFileFailed;
		event DeleteFileProgressHandler DeleteFileCompleted;

		void DeleteVersionedFile(string sourcePath, IFileVersion version, object userData);
		void DeleteMultipleVersionedFile(List<Tuple<string /*sourcePath*/, IFileVersion /*version*/, object /*userData*/>> files);

		#endregion
	}
}
