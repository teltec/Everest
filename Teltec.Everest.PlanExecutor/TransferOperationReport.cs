/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Teltec.Everest.PlanExecutor.Versioning;
using Teltec.Storage;

namespace Teltec.Everest.PlanExecutor
{
	public abstract class TransferOperationReport : BaseOperationReport
	{
		public FileVersionerResults VersionerResults = new FileVersionerResults();
		public TransferResults TransferResults = new TransferResults();

		public override void Reset()
		{
			base.Reset();

			VersionerResults.Reset();
			TransferResults.Reset(0);
		}

		public override void AggregateResults()
		{
			base.AggregateResults();

			AddErrorMessages(VersionerResults.ErrorMessages);
			AddErrorMessages(TransferResults.ErrorMessages);

			// TODO(jweyrich): Should aggreatate `VersionerResults.Stats.Failed + TransferResults.Stats.Failed` into a local `Failed` variable.
		}
	}
}
