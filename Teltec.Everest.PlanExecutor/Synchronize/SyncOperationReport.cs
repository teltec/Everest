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
