using Teltec.Backup.Data.Versioning;
using Teltec.Storage;
using Teltec.Storage.Agent;

namespace Teltec.Backup.PlanExecutor.Synchronize
{
	public class CustomSynchronizationAgent : SynchronizationAgent<CustomVersionedFile>
	{
		public CustomSynchronizationAgent(IAsyncTransferAgent agent)
			: base(agent)
		{
		}
	}
}
