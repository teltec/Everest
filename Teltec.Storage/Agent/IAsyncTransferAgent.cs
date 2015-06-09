using System;
using System.Threading.Tasks;
using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage.Agent
{
	public delegate void TransferFileProgressHandler(object sender, TransferFileProgressArgs e);
	public delegate void TransferFileExceptionHandler(object sender, TransferFileProgressArgs e, Exception ex);

	public interface IAsyncTransferAgent : IDisposable
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

		Task UploadVersionedFile(string sourcePath, IFileVersion version);
		Task UploadFile(string sourcePath, string targetPath);

		#endregion

		#region Download

		event TransferFileProgressHandler DownloadFileStarted;
		event TransferFileProgressHandler DownloadFileProgress;
		event TransferFileExceptionHandler DownloadFileCanceled;
		event TransferFileExceptionHandler DownloadFileFailed;
		event TransferFileProgressHandler DownloadFileCompleted;

		Task DownloadVersionedFile(string sourcePath, IFileVersion version);
		Task DownloadFile(string sourcePath, string targetPath);

		#endregion

		void CancelTransfers();
		void RenewCancellationToken();
	}
}
