using NLog;
using System;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Models;
using Teltec.Common.Extensions;
using Teltec.Forms.Wizard;

namespace Teltec.Backup.App.Forms.BackupPlan
{
	sealed class NewBackupPlanPresenter : WizardPresenter
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly BackupPlanRepository _dao = new BackupPlanRepository();

		public NewBackupPlanPresenter()
			: this(new Models.BackupPlan())
		{
			IsEditingModel = false;
			
			//Models.BackupPlan plan = Model as Models.BackupPlan;
			//plan.Name = "Testing name";
			//plan.ScheduleType = Models.BackupPlan.ScheduleTypeE.RunManually;
		}

		public NewBackupPlanPresenter(Models.BackupPlan plan)
		{
			IsEditingModel = true;
			Model = plan;

			WizardFormOptions options = new WizardFormOptions { DoValidate = true };
			RegisterFormClass(typeof(BackupPlanSelectAccountForm), options);
			RegisterFormClass(typeof(BackupPlanGiveNameForm), options);
			RegisterFormClass(typeof(BackupPlanSelectSourceForm), options);
			RegisterFormClass(typeof(BackupPlanScheduleForm), options);
		}

		public override void OnCancel()
		{
			base.OnCancel();

			Models.BackupPlan plan = Model as Models.BackupPlan;
			_dao.Refresh(plan);
		}

		public override void OnFinish()
		{
			base.OnFinish();

			Models.BackupPlan plan = Model as Models.BackupPlan;

			Console.WriteLine("Name = {0}", plan.Name);
			Console.WriteLine("StorageAccount = {0}", plan.StorageAccount.DisplayName);
			Console.WriteLine("StorageAccountType = {0}", plan.StorageAccountType.ToString());
			foreach (BackupPlanSourceEntry entry in plan.SelectedSources)
				Console.WriteLine("SelectedSource => #{0}, {1}, {2}", entry.Id, entry.Type.ToString(), entry.Path);
			Console.WriteLine("ScheduleType = {0}", plan.ScheduleType.ToString());
			
			PlanSchedule schedule = plan.Schedule;
			switch (plan.ScheduleType)
			{
				case Models.BackupPlan.EScheduleType.RunManually:
					break;
				case Models.BackupPlan.EScheduleType.Specific:
					Console.WriteLine("OccursSpecificallyAt = {0}", schedule.OccursSpecificallyAt.Value);
					break;
				case Models.BackupPlan.EScheduleType.Recurring:
					Console.WriteLine("RecurrencyFrequencyType      = {0}", schedule.RecurrencyFrequencyType.Value);
					switch (schedule.RecurrencyFrequencyType.Value)
					{
						case FrequencyTypeEnum.DAILY:
							break;
						case FrequencyTypeEnum.WEEKLY:
							Console.WriteLine("OccursAtDaysOfWeek           = {0}", schedule.OccursAtDaysOfWeek.ToReadableString());
							break;
						case FrequencyTypeEnum.MONTHLY:
							Console.WriteLine("MonthlyOccurrenceType        = {0}", schedule.MonthlyOccurrenceType.Value);
							Console.WriteLine("OccursMonthlyAtDayOfWeek     = {0}", schedule.OccursMonthlyAtDayOfWeek.Value);
							break;
						case FrequencyTypeEnum.DAY_OF_MONTH:
							Console.WriteLine("OccursAtDayOfMonth           = {0}", schedule.OccursAtDayOfMonth.Value);
							break;
					}
					Console.WriteLine("RecurrencySpecificallyAtTime = {0}", schedule.RecurrencySpecificallyAtTime.Value);
					Console.WriteLine("RecurrencyTimeInterval       = {0}", schedule.RecurrencyTimeInterval.Value);
					Console.WriteLine("RecurrencyTimeUnit           = {0}", schedule.RecurrencyTimeUnit.Value);
					Console.WriteLine("RecurrencyWindowStartsAtTime = {0}", schedule.RecurrencyWindowStartsAtTime.Value);
					Console.WriteLine("RecurrencyWindowEndsAtTime   = {0}", schedule.RecurrencyWindowEndsAtTime.Value);
					break;
			}

			//try
			//{
				if (IsEditingModel)
				{
					_dao.Update(plan);
				}
				else
				{
					_dao.Insert(plan);
				}
			//}
			//catch (Exception ex)
			//{
			//	MessageBox.Show(ex.Message, "Error");
			//} 
		}
	}
}
