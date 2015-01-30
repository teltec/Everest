using System;
using System.Collections.Generic;
using Teltec.Common;
using Teltec.Common.Forms;

namespace Teltec.Backup.Models
{
	public enum EStorageAccountType
	{
		AmazonS3,
		FileSystem
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

		private ICloudStorageAccount _StorageAccount;
		public ICloudStorageAccount StorageAccount
		{
			get { return _StorageAccount; }
			set { SetField(ref _StorageAccount, value); }
		}

		#endregion

		#region Sources

		private IList<BackupPlanSourceEntry> _SelectedSources;
		public IList<BackupPlanSourceEntry> SelectedSources
		{
			get { return _SelectedSources; }
			set { SetField(ref _SelectedSources, value); }
		}

		#endregion

		#region Schedule

		private bool _RunManually;
		public bool RunManually
		{
			get { return _RunManually; }
			set { SetField(ref _RunManually, value); }
		}

		#endregion

	}
}
