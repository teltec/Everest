using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Common;

namespace Teltec.Backup.Models
{
	public class BackupPlan : ObservableObject
	{
		public enum EStorageAccountType
		{
			AmazonS3,
			FileSystem
		};

		private Guid _Id;
		public Guid Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		public const int NAME_MAX_LEN = 128;
		private String _Name;
		public String Name
		{
			get { return _Name; }
			set { SetField(ref _Name, value); }
		}

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

	}
}
