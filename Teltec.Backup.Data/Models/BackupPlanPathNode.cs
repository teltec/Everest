using System;
using System.Collections.Generic;
using Teltec.Common.Utils;

namespace Teltec.Backup.Data.Models
{
	public class BackupPlanPathNode : BaseEntity<Int64?>
	{
		public BackupPlanPathNode()
		{
		}

		public BackupPlanPathNode(BackupPlanFile planFile, EntryType type, string name, string path, BackupPlanPathNode parent)
			: this()
		{
			StorageAccountType = planFile.StorageAccountType;
			StorageAccount = planFile.StorageAccount;
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

		#region Account

		private EStorageAccountType _StorageAccountType;
		public virtual EStorageAccountType StorageAccountType
		{
			get { return _StorageAccountType; }
			set { SetField(ref _StorageAccountType, value); }
		}

		//private int _StorageAccountId;
		//public virtual int StorageAccountId
		//{
		//	get { return _StorageAccountId; }
		//	set { SetField(ref _StorageAccountId, value); }
		//}

		//public static ICloudStorageAccount GetStorageAccount(BackupPlan plan, ICloudStorageAccount dao)
		//{
		//	switch (plan.StorageAccountType)
		//	{
		//		default:
		//			throw new ArgumentException("Unhandled StorageAccountType", "plan");
		//		case EStorageAccountType.AmazonS3:
		//			return dao.Get(plan.StorageAccountId);
		//	}
		//}

		private StorageAccount _StorageAccount;
		public virtual StorageAccount StorageAccount
		{
			get { return _StorageAccount; }
			set { SetField(ref _StorageAccount, value); }
		}

		#endregion

		private BackupPlanPathNode _Parent;
		public virtual BackupPlanPathNode Parent
		{
			get { return _Parent; }
			set { SetField(ref _Parent, value); }
		}

		private EntryType _Type;
		public virtual EntryType Type
		{
			get { return _Type; }
			set { SetField(ref _Type, value); }
		}

		public const int NameMaxLen = 255;
		private string _Name;
		public virtual string Name
		{
			get { return _Name; }
			set { SetField(ref _Name, value); }
		}

		public const int PathMaxLen = 1024;
		private string _Path;
		public virtual string Path
		{
			get { return _Path; }
			set { SetField(ref _Path, StringUtils.NormalizeUsingPreferredForm(value)); }
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
