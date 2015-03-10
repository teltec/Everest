using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Backup.App.Versioning;
using Teltec.Storage;
using Teltec.Storage.Agent;

namespace Teltec.Backup.App
{
	public class CustomBackupAgent : BackupAgent<CustomVersionedFile>
	{
		public CustomBackupAgent(IAsyncTransferAgent agent)
			: base(agent)
		{
		}
	}
}
