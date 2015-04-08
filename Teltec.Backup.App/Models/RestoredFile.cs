using System;
using Teltec.Storage;

namespace Teltec.Backup.App.Models
{
	public class RestoredFile : BaseEntity<Int64?>
	{
		public RestoredFile()
		{
		}

		public RestoredFile(Restore restore, RestorePlanFile file)
			: this()
		{
			Restore = restore;
			File = file;
		}

		private Int64? _Id;
		public virtual Int64? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private Restore _Restore;
		public virtual Restore Restore
		{
			get { return _Restore; }
			protected set { _Restore = value; }
		}

		private RestorePlanFile _File;
		public virtual RestorePlanFile File
		{
			get { return _File; }
			protected set { _File = value; }
		}

		//private RestoreFileStatus _FileStatus;
		//public virtual RestoreFileStatus FileStatus
		//{
		//	get { return _FileStatus; }
		//	set { SetField(ref _FileStatus, value); }
		//}

		private TransferStatus _TransferStatus;
		public virtual TransferStatus TransferStatus
		{
			get { return _TransferStatus; }
			set { _TransferStatus = value; }
		}

		private DateTime _UpdatedAt;
		public virtual DateTime UpdatedAt
		{
			get { return _UpdatedAt; }
			set { _UpdatedAt = value; }
		}
	}
}
