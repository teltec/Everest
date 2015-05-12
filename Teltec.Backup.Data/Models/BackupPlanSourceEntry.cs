using System;

namespace Teltec.Backup.Data.Models
{
	public class BackupPlanSourceEntry : BaseEntity<Int64?>
	{
		//public BackupPlanSourceEntry()
		//{
		//}

		//public BackupPlanSourceEntry(BackupPlan plan, EntryType type, string path) : this()
		//{
		//	BackupPlan = plan;
		//	Type = type;
		//	Path = path;
		//}

		//public BackupPlanSourceEntry(BackupPlan plan, FileSystemTreeNodeTag tag)
		//	: this(plan, tag.ToEntryType(), tag.Path)
		//{
		//}

		private Int64? _Id;
		public virtual Int64? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private BackupPlan _BackupPlan;
		public virtual BackupPlan BackupPlan
		{
			get { return _BackupPlan; }
			set { SetField(ref _BackupPlan, value); }
		}

		private EntryType _Type;
		public virtual EntryType Type
		{
			get { return _Type; }
			set { SetField(ref _Type, value); }
		}

		public const int PathMaxLen = 1024;
		private string _Path;
		public virtual string Path
		{
			get { return _Path; }
			set { SetField(ref _Path, value); }
		}
	}
}
