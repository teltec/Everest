using System;

namespace Teltec.Backup.Data.Models
{
	// TODO(jweyrich: Improvement - We could move these properties to PlanConfig, and replace all uses of `ISchedulablePlan` by `SchedulablePlan`.
	public interface ISchedulablePlan
	{
		PlanConfig Config { get; }
		DateTime UpdatedAt { get; set; }
		Int32 ScheduleParamId { get; }
		string ScheduleParamName { get; }
		ScheduleTypeEnum ScheduleType { get; set; }
		PlanSchedule Schedule { get; set; }
		bool IsRunManually { get; }
		bool IsSpecific { get; }
		bool IsRecurring { get; }
	}

	public abstract class SchedulablePlan<ConcretePlanType> : BaseEntity<Int32?>, ISchedulablePlan
	{
		public static readonly string TaskNamePrefix = "TeltecCloudBackup-";

		public abstract Type GetVirtualType();

		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		#region ISchedulablePlan

		private PlanConfig _Config = new PlanConfig();
		public virtual PlanConfig Config
		{
			get { return _Config; }
			set { SetField(ref _Config, value); }
		}

		#region Schedule

		private DateTime _UpdatedAt; // Last date this entity was updated.
		public virtual DateTime UpdatedAt
		{
			get { return _UpdatedAt; }
			set { SetField(ref _UpdatedAt, value); }
		}

		public virtual Int32 ScheduleParamId
		{
			get { return this.Id.Value; }
		}

		public virtual string ScheduleParamName
		{
			get
			{
				return string.Format("{0}{1}#{2}",
					TaskNamePrefix,
					GetVirtualType().Name,
					this.Id.HasValue ? this.Id.Value.ToString() : string.Empty);
			}
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

		#endregion

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

		private bool _IsDeleted = false;
		public virtual bool IsDeleted
		{
			get { return _IsDeleted; }
			set { SetField(ref _IsDeleted, value); }
		}
	}
}
