using System;
using Teltec.Common;

namespace DemoTransferS3
{
	public class BackupPlanSourceEntry : ObservableObject
	{
		public enum EntryType
		{
			DRIVE = 1,
			FOLDER = 2,
			FILE = 3,
		}

		private Int64? _Id;
		public virtual Int64? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
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
