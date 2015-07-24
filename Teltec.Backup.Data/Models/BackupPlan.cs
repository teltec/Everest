using System;
using System.Collections.Generic;
using Teltec.Common.Extensions;

namespace Teltec.Backup.Data.Models
{
	public class BackupPlan : BaseEntity<Int32?>, ISchedulablePlan
	{
		private Int32? _Id;
		public virtual Int32? Id
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

		#region Sources

		private IList<BackupPlanSourceEntry> _SelectedSources = new List<BackupPlanSourceEntry>();
		public virtual IList<BackupPlanSourceEntry> SelectedSources
		{
			get { return _SelectedSources; }
			protected set { SetField(ref _SelectedSources, value); InvalidateCachedSelectedSourcesAsDelimitedString(); }
		}

		private string _CachedSelectedSourcesAsDelimitedString;
		public virtual string SelectedSourcesAsDelimitedString(string delimiter, int maxLength, string trail)
		{
			if (_CachedSelectedSourcesAsDelimitedString == null)
				_CachedSelectedSourcesAsDelimitedString = SelectedSources.AsDelimitedString(p => p.Path,
					"No selected sources", delimiter, maxLength, trail);
			return _CachedSelectedSourcesAsDelimitedString;
		}

		public virtual void InvalidateCachedSelectedSourcesAsDelimitedString()
		{
			_CachedSelectedSourcesAsDelimitedString = null;
		}

		#endregion

		#region Files

		private IList<BackupPlanFile> _Files = new List<BackupPlanFile>();
		public virtual IList<BackupPlanFile> Files
		{
			get { return _Files; }
			protected set { SetField(ref _Files, value); }
		}

		#endregion

		#region Backups

		private IList<Backup> _Backups = new List<Backup>();
		public virtual IList<Backup> Backups
		{
			get { return _Backups; }
			protected set { SetField(ref _Backups, value); }
		}

		#endregion

		#region Schedule

		public virtual string ScheduleParamId
		{
			get { return this.Id.HasValue ? this.Id.Value.ToString() : string.Empty; }
		}

		public virtual string ScheduleParamName
		{
			get { return string.Format("{0}#{1}", this.GetType().Name, this.Id.HasValue ? this.Id.Value.ToString() : string.Empty); }
		}

		private ScheduleTypeEnum _ScheduleType;
		public virtual ScheduleTypeEnum ScheduleType
		{
			get { return _ScheduleType; }
			set { SetField(ref _ScheduleType, value); }
		}

		private PlanSchedule _Schedule = new PlanSchedule();
		public virtual PlanSchedule Schedule
		{
			get { return _Schedule; }
			set
			{
				if (value == null)
					value = new PlanSchedule();
				SetField(ref _Schedule, value);
			}
		}

		public virtual bool IsRunManually
		{
			get { return ScheduleType == ScheduleTypeEnum.RUN_MANUALLY; }
		}

		public virtual bool IsSpecific
		{
			get { return ScheduleType == ScheduleTypeEnum.SPECIFIC; }
		}

		public virtual bool IsRecurring
		{
			get { return ScheduleType == ScheduleTypeEnum.RECURRING; }
		}

		#endregion

		private DateTime? _LastRunAt;
		public virtual DateTime? LastRunAt
		{
			get { return _LastRunAt; }
			set { SetField(ref _LastRunAt, value); }
		}

		private DateTime? _LastSuccessfulRunAt;
		public virtual DateTime? LastSuccessfulRunAt
		{
			get { return _LastSuccessfulRunAt; }
			set { SetField(ref _LastSuccessfulRunAt, value); }
		}
	}
}
