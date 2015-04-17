using System;
using System.Collections.Generic;
using Teltec.Storage;

namespace Teltec.Backup.App.Models
{
	public class BackupPlanPathNode : BaseEntity<Int64?>
	{
		public BackupPlanPathNode()
		{
		}

		public BackupPlanPathNode(BackupPlanFile planFile, EntryType type, string name, string path, BackupPlanPathNode parent)
			: this()
		{
			BackupPlan = planFile.BackupPlan;
			Type = type;
			// Only assign `PlanFile` if this is for a node that represents a FILE.
			if (Type == EntryType.FILE)
				PlanFile = planFile;
			Name = name;
			Path = path;
			Parent = parent;
		}

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
			set { _BackupPlan = value; }
		}

		private BackupPlanPathNode _Parent;
		public virtual BackupPlanPathNode Parent
		{
			get { return _Parent; }
			set { _Parent = value; }
		}

		private EntryType _Type;
		public virtual EntryType Type
		{
			get { return _Type; }
			set { _Type = value; }
		}

		public const int NameMaxLen = 255;
		private string _Name;
		public virtual string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		public const int PathMaxLen = 1024;
		private string _Path;
		public virtual string Path
		{
			get { return _Path; }
			set { _Path = value; }
		}

		private IList<BackupPlanPathNode> _SubNodes = new List<BackupPlanPathNode>();
		public virtual IList<BackupPlanPathNode> SubNodes
		{
			get { return _SubNodes; }
			protected set { SetField(ref _SubNodes, value); }
		}

		private BackupPlanFile _PlanFile;
		public virtual BackupPlanFile PlanFile
		{
			get { return _PlanFile; }
			protected set { SetField(ref _PlanFile, value); }
		}
	}
}
