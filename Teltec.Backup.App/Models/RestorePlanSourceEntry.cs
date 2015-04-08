using System;

namespace Teltec.Backup.App.Models
{
	public class RestorePlanSourceEntry : BaseEntity<Int64?>
	{
		private Int64? _Id;
		public virtual Int64? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private RestorePlan _RestorePlan;
		public virtual RestorePlan RestorePlan
		{
			get { return _RestorePlan; }
			set { SetField(ref _RestorePlan, value); }
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

		private BackupPlanPathNode _PathNode;
		public virtual BackupPlanPathNode PathNode
		{
			get { return _PathNode; }
			set { SetField(ref _PathNode, value); }
		}

		public const int VersionMaxLen = 10;
		private string _Version;
		public virtual string Version
		{
			get { return _Version; }
			set { SetField(ref _Version, value); }
		}
	}
}
