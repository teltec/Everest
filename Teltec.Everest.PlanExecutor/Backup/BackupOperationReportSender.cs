using Teltec.Common.Utils;

namespace Teltec.Everest.PlanExecutor.Backup
{
	public sealed class BackupOperationReportSender : BaseOperationReportSender<BackupOperationReport>
	{
		public BackupOperationReportSender(BackupOperationReport report) : base(report) { }

		protected override void BuildRequestBody()
		{
			// Status
			RequestBody.Add("Status", Report.OperationStatus.ToString());

			// Transfers
			RequestBody.Add("Total", Report.TransferResults.Stats.Total);
			RequestBody.Add("Pending", Report.TransferResults.Stats.Pending);
			RequestBody.Add("Running", Report.TransferResults.Stats.Running);
			RequestBody.Add("Failed", Report.VersionerResults.Stats.Failed + Report.TransferResults.Stats.Failed);
			RequestBody.Add("Canceled", Report.TransferResults.Stats.Canceled);
			RequestBody.Add("Completed", Report.TransferResults.Stats.Completed);

			// Sizes
			RequestBody.Add("TotalSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesTotal));
			RequestBody.Add("PendingSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesPending));
			RequestBody.Add("FailedSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesFailed));
			RequestBody.Add("CanceledSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesCanceled));
			RequestBody.Add("CompletedSize", FileSizeUtils.FileSizeToString(Report.TransferResults.Stats.BytesCompleted));
		}
	}
}
