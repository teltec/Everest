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
