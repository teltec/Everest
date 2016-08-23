using Teltec.Everest.Data.Versioning;
using Teltec.Storage;
using Teltec.Storage.Backend;

namespace Teltec.Everest.PlanExecutor.Backup
{
	public class CustomBackupAgent : BackupAgent<CustomVersionedFile>
	{
		public CustomBackupAgent(ITransferAgent agent)
			: base(agent)
		{
		}
	}
}
