/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Teltec.Storage;

namespace Teltec.Everest.PlanExecutor.Synchronize
{
	public sealed class SyncOperationReport : BaseOperationReport
	{
		public SyncResults SyncResults;

		public override void Reset()
		{
			base.Reset();

			SyncResults.Reset();
		}

		public override void AggregateResults()
		{
			base.AggregateResults();

			AddErrorMessages(SyncResults.ErrorMessages);
		}
	}
}
