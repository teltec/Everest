using System;
using Teltec.Storage;

namespace Teltec.Backup.App.Models
{
	public class BackupedFile : BaseEntity<Int64?>
	{
		public BackupedFile()
		{
		}

		public BackupedFile(Backup backup, BackupPlanFile file)
			: this()
		{
			Backup = backup;
			File = file;
		}

		private Int64? _Id;
		public virtual Int64? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private Backup _Backup;
		public virtual Backup Backup
		{
			get { return _Backup; }
			protected set { _Backup = value; }
		}

		private BackupPlanFile _File;
		public virtual BackupPlanFile File
		{
			get { return _File; }
			protected set { _File = value; }
		}

		private BackupStatus _Status;
		public virtual BackupStatus Status
		{
			get { return _Status; }
			set { _Status = value; }
		}

		private DateTime _UpdatedAt;
		public virtual DateTime UpdatedAt
		{
			get { return _UpdatedAt; }
			set { _UpdatedAt = value; }
		}
	}
}
