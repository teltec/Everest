using Teltec.Backup.Data.Versioning;
using Teltec.Storage;
using Teltec.Storage.Backend;

namespace Teltec.Backup.PlanExecutor.Restore
{
	public class CustomRestoreAgent : RestoreAgent<CustomVersionedFile>
	{
		public CustomRestoreAgent(ITransferAgent agent)
			: base(agent)
		{
		}
	}
}
