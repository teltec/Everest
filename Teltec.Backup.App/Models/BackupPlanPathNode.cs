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

		public BackupPlanPathNode(BackupPlan plan, EntryType type, string name, string path, BackupPlanPathNode parent)
			: this()
		{
			BackupPlan = plan;
			Type = type;
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

		/*
		private string FormattedName(EntryType type, string name)
		{
			switch (type)
			{
				default:
					throw new ArgumentException("Unhandled type in switch-case", "type");
				case EntryType.FOLDER:
					return name
						+ System.IO.Path.DirectorySeparatorChar.ToString();
				case EntryType.FILE:
					return name;
				case EntryType.FILE_VERSION:
					return name;
				case EntryType.DRIVE:
					return name
						+ System.IO.Path.VolumeSeparatorChar.ToString()
						+ System.IO.Path.DirectorySeparatorChar.ToString();
			}
		}

		public virtual string Path
		{
			get
			{
				List<string> parts = new List<string>();
				
				// Add itself
				parts.Add(FormattedName(Type, Name));
				
				// Iterate over parents
				BackupPlanPathNode parent = Parent;
				while (parent != null)
				{
					// Add parent
					parts.Add(FormattedName(parent.Type, parent.Name));
					parent = parent.Parent;
				}

				parts.Reverse();

				return string.Join("", parts);
			}
		}
		*/

		private IList<BackupPlanPathNode> _SubNodes = new List<BackupPlanPathNode>();
		public virtual IList<BackupPlanPathNode> SubNodes
		{
			get { return _SubNodes; }
			protected set { SetField(ref _SubNodes, value); }
		}
	}
}
