using NUnit.Framework;
using System;
using System.Collections.Generic;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;

namespace Teltec.Backup.Data.Models
{
	public enum ScheduleTypeEnum
	{
		UNDEFINED = 0,
		RUN_MANUALLY = 1,
		SPECIFIC = 2,
		RECURRING = 3,
	}

	public enum DailyFrequencyTypeEnum
	{
		UNDEFINED = 0,
		SPECIFIC = 1,
		EVERY = 2,
	}

	public enum FrequencyTypeEnum
	{
		UNDEFINED = 0,
		DAILY = 1,
		WEEKLY = 2,
		MONTHLY = 3,
		DAY_OF_MONTH = 4,
	}

	public enum TimeUnitEnum
	{
		UNDEFINED = 0,
		HOURS = 1,
		MINUTES = 2,
	}

	public enum MonthlyOccurrenceTypeEnum
	{
		UNDEFINED = 0,
		FIRST = 1,
		SECOND = 2,
		THIRD = 3,
		FOURTH = 4,
		PENULTIMATE = 5,
		LAST = 6,
	}

	public class PlanScheduleDayOfWeek : BaseEntity<Int32?>
	{
		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private PlanSchedule _Schedule;
		public virtual PlanSchedule Schedule
		{
			get { return _Schedule; }
			set { SetField(ref _Schedule, value); }
		}

		private DayOfWeek _DayOfWeek;
		public virtual DayOfWeek DayOfWeek
		{
			get { return _DayOfWeek; }
			set { SetField(ref _DayOfWeek, value); }
		}
	}

	public class PlanSchedule : BaseEntity<Int32?>
	{
		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private ScheduleTypeEnum _ScheduleType = ScheduleTypeEnum.RUN_MANUALLY;
		public virtual ScheduleTypeEnum ScheduleType
		{
			get { return _ScheduleType; }
			set { SetField(ref _ScheduleType, value); }
		}

		#region ScheduleType == ScheduleTypeEnum.SPECIFIC

		private DateTime? _OccursSpecificallyAt;
		public virtual DateTime? OccursSpecificallyAt
		{
			get { return _OccursSpecificallyAt; }
			set
			{
				SetField(ref _OccursSpecificallyAt, value);
				NotifyPropertyChanged(this.GetPropertyName((PlanSchedule x) => x.ProxyOccursSpecificallyAtDate));
				NotifyPropertyChanged(this.GetPropertyName((PlanSchedule x) => x.ProxyOccursSpecificallyAtTime));
			}
		}

		#region Helper Proxies

		public virtual DateTime ProxyOccursSpecificallyAtDate
		{
			get
			{
				return OccursSpecificallyAt.HasValue
					? OccursSpecificallyAt.Value.ToLocalTime()
					: DateTimeUtils.RoundUpToQuarterHours(DateTime.Now);
			}
			set
			{
				DateTime v = OccursSpecificallyAt.HasValue
					? OccursSpecificallyAt.Value
					: DateTimeUtils.RoundUpToQuarterHours(DateTime.Now);
				OccursSpecificallyAt = new DateTime(value.Year, value.Month, value.Day, v.Hour, v.Minute, 0).ToUniversalTime();
				//NotifyCallingPropertyChanged();
			}
		}

		public virtual DateTime ProxyOccursSpecificallyAtTime
		{
			get
			{
				return OccursSpecificallyAt.HasValue
					? OccursSpecificallyAt.Value.ToLocalTime()
					: DateTimeUtils.RoundUpToQuarterHours(DateTime.Now);
			}
			set
			{
				DateTime v = OccursSpecificallyAt.HasValue
					? OccursSpecificallyAt.Value
					: DateTimeUtils.RoundUpToQuarterHours(DateTime.Now);
				OccursSpecificallyAt = new DateTime(v.Year, v.Month, v.Day, value.Hour, value.Minute, 0).ToUniversalTime();
				//NotifyCallingPropertyChanged();
			}
		}

		#endregion

		#endregion

		#region ScheduleType == ScheduleTypeEnum.RECURRING

		private FrequencyTypeEnum? _RecurrencyFrequencyType;
		public virtual FrequencyTypeEnum? RecurrencyFrequencyType
		{
			get { return _RecurrencyFrequencyType; }
			set { SetField(ref _RecurrencyFrequencyType, value); }
		}

		private DailyFrequencyTypeEnum? _RecurrencyDailyFrequencyType;
		public virtual DailyFrequencyTypeEnum? RecurrencyDailyFrequencyType
		{
			get { return _RecurrencyDailyFrequencyType; }
			set { SetField(ref _RecurrencyDailyFrequencyType, value); }
		}

		#region RecurrencyDailyFrequencyType == DailyFrequencyTypeEnum.SPECIFIC

		private TimeSpan? _RecurrencySpecificallyAtTime;
		public virtual TimeSpan? RecurrencySpecificallyAtTime // Use only as HH:mm
		{
			get { return _RecurrencySpecificallyAtTime; }
			set { SetField(ref _RecurrencySpecificallyAtTime, value); }
		}

		#endregion

		#region RecurrencyDailyFrequencyType == DailyFrequencyTypeEnum.EVERY

		private Int16? _RecurrencyTimeInterval;
		public virtual Int16? RecurrencyTimeInterval // Represents number of HOURS or MINUTES
		{
			get { return _RecurrencyTimeInterval; }
			set { SetField(ref _RecurrencyTimeInterval, value); }
		}

		private TimeUnitEnum? _RecurrencyTimeUnit;
		public virtual TimeUnitEnum? RecurrencyTimeUnit
		{
			get { return _RecurrencyTimeUnit; }
			set { SetField(ref _RecurrencyTimeUnit, value); }
		}

		private TimeSpan? _RecurrencyWindowStartsAtTime;
		public virtual TimeSpan? RecurrencyWindowStartsAtTime // Use only as HH:mm
		{
			get { return _RecurrencyWindowStartsAtTime; }
			set { SetField(ref _RecurrencyWindowStartsAtTime, value); }
		}

		private TimeSpan? _RecurrencyWindowEndsAtTime;
		public virtual TimeSpan? RecurrencyWindowEndsAtTime // Use only as HH:mm
		{
			get { return _RecurrencyWindowEndsAtTime; }
			set { SetField(ref _RecurrencyWindowEndsAtTime, value); }
		}

		#endregion

		#region Daily frequency
		#endregion

		#region Weekly frequency

		private IList<PlanScheduleDayOfWeek> _OccursAtDaysOfWeek = new List<PlanScheduleDayOfWeek>();
		public virtual IList<PlanScheduleDayOfWeek> OccursAtDaysOfWeek
		{
			get { return _OccursAtDaysOfWeek; }
			set
			{
				//if (value != null && value.Count > 0) // This condition is somehow misleading.
				//	Assert.AreEqual(FrequencyTypeEnum.WEEKLY, RecurrencyFrequencyType.Value);
				SetField(ref _OccursAtDaysOfWeek, value);
			}
		}

		#endregion

		#region Monthly frequency

		private MonthlyOccurrenceTypeEnum? _MonthlyOccurrenceType;
		public virtual MonthlyOccurrenceTypeEnum? MonthlyOccurrenceType
		{
			get { return _MonthlyOccurrenceType; }
			set
			{
				if (value != null)
					//Assert.AreEqual(FrequencyTypeEnum.MONTHLY, RecurrencyFrequencyType.Value);
				SetField(ref _MonthlyOccurrenceType, value);
			}
		}

		private DayOfWeek? _OccursMonthlyAtDayOfWeek;
		public virtual DayOfWeek? OccursMonthlyAtDayOfWeek
		{
			get { return _OccursMonthlyAtDayOfWeek; }
			set
			{
				//if (value != null)
				//	Assert.AreEqual(FrequencyTypeEnum.MONTHLY, RecurrencyFrequencyType.Value);
				SetField(ref _OccursMonthlyAtDayOfWeek, value);
			}
		}

		#endregion

		#region Day of Month frequency

		private Int16? _OccursAtDayOfMonth;
		public virtual Int16? OccursAtDayOfMonth // Day number (1..31)
		{
			get { return _OccursAtDayOfMonth; }
			set
			{
				//if (value != null)
				//	Assert.AreEqual(FrequencyTypeEnum.DAY_OF_MONTH, RecurrencyFrequencyType.Value);
				SetField(ref _OccursAtDayOfMonth, value);
			}
		}

		#endregion

		#endregion

		public virtual bool IsSpecific
		{
			get { return ScheduleType == ScheduleTypeEnum.SPECIFIC; }
		}

		public virtual bool IsRecurrent
		{
			get { return ScheduleType == ScheduleTypeEnum.RECURRING; }
		}

		public virtual bool IsRecurrencyDailyFrequencySpecific
		{
			get { return RecurrencyDailyFrequencyType == DailyFrequencyTypeEnum.SPECIFIC; }
		}

		private ArgumentException BuildInvalidEnumValueException(string propertyName, Enum value)
		{
			var message = string.Format("Unhandled {0} value: {1}", typeof(TimeUnitEnum).FullName, value);
			return new ArgumentException(message, propertyName);
		}

		public virtual Int16 MinimumRecurrencyTimeInterval
		{
			get
			{
				Assert.IsNotNull(this.RecurrencyTimeUnit);

				switch (this.RecurrencyTimeUnit.Value)
				{
					default: throw BuildInvalidEnumValueException(this.GetPropertyName((PlanSchedule x) => x.RecurrencyTimeUnit), this.RecurrencyTimeUnit.Value);
					case TimeUnitEnum.MINUTES: return 30;
					case TimeUnitEnum.HOURS: return 1;
				}
			}
		}

		public virtual Int16 MaximumRecurrencyTimeInterval
		{
			get
			{
				Assert.IsNotNull(this.RecurrencyTimeUnit);

				switch (this.RecurrencyTimeUnit.Value)
				{
					default: throw BuildInvalidEnumValueException(this.GetPropertyName((PlanSchedule x) => x.RecurrencyTimeUnit), this.RecurrencyTimeUnit.Value);
					// If you're configuring an interval for repetition, you want to run
					// at least twice a day, so having a daily recurrence interval > 12 hours
					// doesn't make much sense.
					// You should use `RecurringSpecificallyAt` instead.
					case TimeUnitEnum.MINUTES: return 180;
					case TimeUnitEnum.HOURS: return 12;
				}
			}
		}

		public virtual bool IsRecurringValid()
		{
			bool isValid = true;

			switch (RecurrencyFrequencyType.Value)
			{
				default: throw new ArgumentException(
					string.Format("Unhandled value for {0}: {1}", typeof(FrequencyTypeEnum).FullName, RecurrencyFrequencyType.Value),
					this.GetPropertyName((PlanSchedule x) => x.RecurrencyFrequencyType));
				case FrequencyTypeEnum.DAILY:
					{
						break;
					}
				case FrequencyTypeEnum.WEEKLY:
					{
						isValid = OccursAtDaysOfWeek != null && OccursAtDaysOfWeek.Count > 0;
						if (!isValid)
							return false;

						break;
					}
				case FrequencyTypeEnum.MONTHLY:
					{
						isValid = MonthlyOccurrenceType.HasValue;
						if (!isValid)
							return false;

						isValid = MonthlyOccurrenceType.Value != MonthlyOccurrenceTypeEnum.UNDEFINED;
						if (!isValid)
							return false;

						isValid = OccursMonthlyAtDayOfWeek.HasValue;
						if (!isValid)
							return false;

						break;
					}
				case FrequencyTypeEnum.DAY_OF_MONTH:
					{
						isValid = OccursAtDayOfMonth.HasValue;
						if (!isValid)
							return false;

						// TODO: Validate whether the informed day of month exists for all months?
						//       For example, February doesn't have days 30 and 31, and only has 29 on leap years.

						break;
					}
			}

			if (!RecurrencyDailyFrequencyType.HasValue)
				return false;

			switch (RecurrencyDailyFrequencyType.Value)
			{
				default: throw new ArgumentException(
					string.Format("Unhandled value for {0}: {1}", typeof(DailyFrequencyTypeEnum).FullName, RecurrencyDailyFrequencyType.Value),
					this.GetPropertyName((PlanSchedule x) => x.RecurrencyDailyFrequencyType));
				case DailyFrequencyTypeEnum.SPECIFIC:
					{
						if (!RecurrencySpecificallyAtTime.HasValue)
							return false;

						break;
					}
				case DailyFrequencyTypeEnum.EVERY:
					{
						if (!RecurrencyTimeUnit.HasValue)
							return false;

						if (!RecurrencyTimeInterval.HasValue)
							return false;

						short interval = RecurrencyTimeInterval.Value;
						if (interval < MinimumRecurrencyTimeInterval || interval > MaximumRecurrencyTimeInterval)
							return false;

						if (!RecurrencyWindowStartsAtTime.HasValue)
							return false;

						if (!RecurrencyWindowEndsAtTime.HasValue)
							return false;

						break;
					}
			}

			return true;
		}

		public virtual bool IsValid()
		{
			switch (ScheduleType)
			{
				default: throw new ArgumentException(
					string.Format("Unhandled value for {0}: {1}", typeof(ScheduleTypeEnum).FullName, ScheduleType),
					this.GetPropertyName((PlanSchedule x ) => x.ScheduleType));
				case ScheduleTypeEnum.RUN_MANUALLY:
					break;
				case ScheduleTypeEnum.SPECIFIC:
					{
						if (!OccursSpecificallyAt.HasValue)
							return false;
						if (OccursSpecificallyAt.Value <= DateTime.UtcNow)
							return false;
						break;
					}
				case ScheduleTypeEnum.RECURRING:
					{
						if (!IsRecurringValid())
							return false;
						break;
					}
			}

			return true;
		}
	}
}

