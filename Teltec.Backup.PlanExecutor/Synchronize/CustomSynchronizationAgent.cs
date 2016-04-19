using Teltec.Backup.Data.Versioning;
using Teltec.Storage;
using Teltec.Storage.Backend;

namespace Teltec.Backup.PlanExecutor.Synchronize
{
	public class CustomSynchronizationAgent : SynchronizationAgent<CustomVersionedFile>
	{
		public CustomSynchronizationAgent(ITransferAgent agent)
			: base(agent)
		{
		}
	}
}
