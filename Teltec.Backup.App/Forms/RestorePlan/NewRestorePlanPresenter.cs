using NLog;
using System;
using Teltec.Backup.App.Forms.Schedule;
using Teltec.Backup.Data.DAO;
using Teltec.Forms.Wizard;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.RestorePlan
{
	sealed class NewRestorePlanPresenter : WizardPresenter
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly RestorePlanRepository _dao = new RestorePlanRepository();

		public NewRestorePlanPresenter()
			: this(new Models.RestorePlan())
		{
			IsEditingModel = false;

			//Models.RestorePlan plan = Model as Models.RestorePlan;
			//plan.Name = "Testing name";
			//plan.ScheduleType = Models.BackupPlan.ScheduleTypeE.RunManually;
		}

		public NewRestorePlanPresenter(Models.RestorePlan plan)
		{
			IsEditingModel = true;
			Model = plan;

			WizardFormOptions options = new WizardFormOptions { DoValidate = true };
			// Only show `RestorePlanSelectBackupPlanForm` if the `BackupPlan` is not already informed.
			if (plan.BackupPlan == null || !plan.BackupPlan.Id.HasValue)
				RegisterFormClass(typeof(RestorePlanSelectBackupPlanForm), options);
			RegisterFormClass(typeof(RestorePlanGiveNameForm), options);
			RegisterFormClass(typeof(RestorePlanSelectSourceForm), options);
			RegisterFormClass(typeof(SchedulablePlanForm<Models.RestorePlan>), options);
		}

		public override void OnCancel()
		{
			base.OnCancel();

			Models.RestorePlan plan = Model as Models.RestorePlan;
			_dao.Refresh(plan);
		}

		public override void OnFinish()
		{
			base.OnFinish();

			Models.RestorePlan plan = Model as Models.RestorePlan;

			Console.WriteLine("Name = {0}", plan.Name);
			Console.WriteLine("BackupPlan = {0}", plan.BackupPlan.Name);
			foreach (Models.RestorePlanSourceEntry entry in plan.SelectedSources)
				Console.WriteLine("SelectedSource => #{0}, {1}, {2}, {3}",
					entry.Id, entry.Type.ToString(), entry.Path, entry.Version);
			Console.WriteLine("ScheduleType = {0}", plan.ScheduleType.ToString());

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
