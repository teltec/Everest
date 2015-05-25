using Teltec.Backup.Data.Versioning;
using Teltec.Storage;
using Teltec.Storage.Agent;

namespace Teltec.Backup.PlanExecutor.Backup
{
	public class CustomBackupAgent : BackupAgent<CustomVersionedFile>
	{
		public CustomBackupAgent(IAsyncTransferAgent agent)
			: base(agent)
		{
		}
	}
}
