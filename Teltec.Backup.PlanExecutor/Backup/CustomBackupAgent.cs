using Teltec.Backup.Data.Versioning;
using Teltec.Storage;
using Teltec.Storage.Backend;

namespace Teltec.Backup.PlanExecutor.Backup
{
	public class CustomBackupAgent : BackupAgent<CustomVersionedFile>
	{
		public CustomBackupAgent(ITransferAgent agent)
			: base(agent)
		{
		}
	}
}
