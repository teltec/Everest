/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

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
