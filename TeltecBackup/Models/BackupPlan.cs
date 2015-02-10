using System;
using System.Collections.Generic;
using Teltec.Common;
using System.Linq;
using Teltec.Common.Forms;
using Teltec.Backup.DAO;

namespace Teltec.Backup.Models
{
	public class BackupPlan : ObservableObject, IEntity<int>
	{
		private int _Id;
		public virtual int Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		#region Name

		public const int NameMaxLen = 128;
		private String _Name;
		public virtual String Name
		{
			get { return _Name; }
			set { SetField(ref _Name, value); }
		}

		#endregion

		#region Accounts

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

		#region Sources

		private IList<BackupPlanSourceEntry> _SelectedSources = new List<BackupPlanSourceEntry>();
		public virtual IList<BackupPlanSourceEntry> SelectedSources
		{
			get { return _SelectedSources; }
			set { SetField(ref _SelectedSources, value); }
		}

		#endregion

		#region Schedule

		public enum EScheduleType
		{
			RunManually = 0,
		}

		private EScheduleType _ScheduleType;
		public virtual EScheduleType ScheduleType
		{
			get { return _ScheduleType; }
			set { SetField(ref _ScheduleType, value); }
		}

		public virtual bool IsRunManually
		{
			get { return ScheduleType == EScheduleType.RunManually;  }
		}

		#endregion

		public virtual Dictionary<string, FileSystemTreeNodeTag> SelectedSourcesAsCheckedDataSource()
		{
			return SelectedSources.ToDictionary(
				e => e.Path,
				e => new FileSystemTreeNodeTag(e.Type.ToInfoType(), e.Path, CheckState.Checked)
			);
		}
	}
}
