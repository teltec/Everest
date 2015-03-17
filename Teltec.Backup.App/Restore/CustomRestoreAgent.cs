using Teltec.Backup.App.Versioning;
using Teltec.Storage;
using Teltec.Storage.Agent;

namespace Teltec.Backup.App.Restore
{
	public class CustomRestoreAgent : RestoreAgent<CustomVersionedFile>
	{
		public CustomRestoreAgent(IAsyncTransferAgent agent)
			: base(agent)
		{
		}
	}
}
