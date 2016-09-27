/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Teltec.Common.Utils;

namespace Teltec.Everest.PlanExecutor.Restore
{
	public sealed class RestoreOperationReportSender : BaseOperationReportSender<RestoreOperationReport>
	{
		public RestoreOperationReportSender(RestoreOperationReport report) : base(report) { }

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
