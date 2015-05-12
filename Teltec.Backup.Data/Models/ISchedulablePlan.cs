
namespace Teltec.Backup.Data.Models
{
	public interface ISchedulablePlan
	{
		string ScheduleParamId
		{
			get;
		}

		string ScheduleParamName
		{
			get;
		}

		ScheduleTypeEnum ScheduleType
		{
			get;
			set;
		}

		PlanSchedule Schedule
		{
			get;
			set;
		}

		bool IsRunManually
		{
			get;
		}

		bool IsSpecific
		{
			get;
		}

		bool IsRecurring
		{
			get;
		}
	}
}
