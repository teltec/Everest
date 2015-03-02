using System;
using Teltec.Backup.App.Models;

namespace Teltec.Backup.App.DAO
{
	public class AmazonS3AccountRepository : BaseRepository<AmazonS3Account, int?>
	{
	}
	
	public class BackupPlanRepository : BaseRepository<BackupPlan, int?>
	{
	}

	public class BackupPlanSourceEntryRepository : BaseRepository<BackupPlanSourceEntry, Int64?>
	{
	}
}
