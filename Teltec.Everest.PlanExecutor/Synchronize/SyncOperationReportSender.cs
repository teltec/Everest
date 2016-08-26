using Teltec.Common.Utils;

namespace Teltec.Everest.PlanExecutor.Synchronize
{
	public sealed class SyncOperationReportSender : BaseOperationReportSender<SyncOperationReport>
	{
		public SyncOperationReportSender(SyncOperationReport report) : base(report) { }

		protected override void BuildRequestBody()
		{
			// Sync
			RequestBody.Add("FileCount", Report.SyncResults.Stats.FileCount);
			RequestBody.Add("SavedFileCount", Report.SyncResults.Stats.SavedFileCount);

			// Sizes
			RequestBody.Add("TotalSize", FileSizeUtils.FileSizeToString(Report.SyncResults.Stats.TotalSize));
		}
	}
}
