using Teltec.Storage;
using Teltec.Storage.Backend;

namespace DemoTransferS3
{
	public class CustomBackupAgent : BackupAgent<CustomVersionedFile>
	{
		public CustomBackupAgent(ITransferAgent agent)
			: base(agent)
		{
		}
	}
}
