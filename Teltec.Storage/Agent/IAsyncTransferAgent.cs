using System;
using System.Threading.Tasks;

namespace Teltec.Storage.Agent
{
	public delegate void TransferFileProgressHandler(object sender, TransferFileProgressArgs e);
	public delegate void TransferFileExceptionHandler(object sender, TransferFileProgressArgs e, Exception ex);

	public interface IAsyncTransferAgent : IDisposable
	{
		string LocalRootDir { get; set; }
		string RemoteRootDir { get; set; }

		Task UploadFile(string sourcePath);
		Task UploadFile(string sourcePath, string targetPath);

		Task DownloadFile(string sourcePath);
		Task DownloadFile(string sourcePath, string targetPath);

		void CancelTransfers();
		void RenewCancellationToken();

		event TransferFileProgressHandler UploadFileStarted;
		event TransferFileProgressHandler UploadFileProgress;
		event TransferFileExceptionHandler UploadFileCanceled;
		event TransferFileExceptionHandler UploadFileFailed;
		event TransferFileProgressHandler UploadFileCompleted;
	}
}
