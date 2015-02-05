using System;
using System.Collections.Generic;
using Teltec.Common;
using System.Linq;
using Teltec.Common.Forms;

namespace Teltec.Backup.Models
{
	public enum EStorageAccountType
	{
		AmazonS3	= 1,
		FileSystem	= 2,
	};

	public class BackupPlan : ObservableObject
	{
		private Guid _Id;
		public Guid Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		#region Name

		public const int NameMaxLen = 128;
		private String _Name;
		public String Name
		{
			get { return _Name; }
			set { SetField(ref _Name, value); }
		}

		#endregion

		#region Accounts

		private EStorageAccountType _StorageAccountType;
		public EStorageAccountType StorageAccountType
		{
			get { return _StorageAccountType; }
			set { SetField(ref _StorageAccountType, value); }
		}

		private Guid _StorageAccountId;
		public Guid StorageAccountId
		{
			get { return _StorageAccountId; }
			set { SetField(ref _StorageAccountId, value); }
		}

		public static ICloudStorageAccount GetStorageAccount(BackupPlan plan, DBContextScope scope)
		{
			switch (plan.StorageAccountType)
			{
				default:
					throw new ArgumentException("Unhandled StorageAccountType", "plan");
				case EStorageAccountType.AmazonS3:
					return scope.AmazonS3Accounts.Get(plan.StorageAccountId);
			}
		}

		private ICloudStorageAccount _StorageAccount;
		public virtual ICloudStorageAccount StorageAccount
		{
			get { return _StorageAccount; }
			set { SetField(ref _StorageAccount, value); }
		}

		#endregion

		#region Sources

		private List<BackupPlanSourceEntry> _SelectedSources = new List<BackupPlanSourceEntry>();
		public virtual List<BackupPlanSourceEntry> SelectedSources
		{
			get { return _SelectedSources; }
			set { SetField(ref _SelectedSources, value); }
		}

		#endregion

		#region Schedule

		public enum ScheduleTypeE
		{
			RunManually = 0,
		}

		private ScheduleTypeE _ScheduleType;
		public ScheduleTypeE ScheduleType
		{
			get { return _ScheduleType; }
			set { SetField(ref _ScheduleType, value); }
		}

		public bool IsRunManually
		{
			get { return ScheduleType == ScheduleTypeE.RunManually;  }
		}

		#endregion

		public Dictionary<string, FileSystemTreeNodeTag> SelectedSourcesAsCheckedDataSource()
		{
			return SelectedSources.ToDictionary(
				e => e.Path,
				e => new FileSystemTreeNodeTag(e.Type.ToInfoType(), e.Path, CheckState.Checked)
			);
		}

	}
}
