using Teltec.Everest.Data.Versioning;
using Teltec.Storage;
using Teltec.Storage.Backend;

namespace Teltec.Everest.PlanExecutor.Synchronize
{
	public class CustomSynchronizationAgent : SynchronizationAgent<CustomVersionedFile>
	{
		public CustomSynchronizationAgent(ITransferAgent agent)
			: base(agent)
		{
		}
	}
}
