using NLog;
using System;
using Teltec.Everest.App.Forms.Notification;
using Teltec.Everest.App.Forms.Schedule;
using Teltec.Everest.Data.DAO;
using Teltec.Forms.Wizard;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.RestorePlan
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
			// Only show `RestorePlanSelectAccountForm` if the `StorageAccount` is not already informed.
			if (plan.StorageAccount == null || !plan.StorageAccount.Id.HasValue)
				RegisterFormClass(typeof(RestorePlanSelectAccountForm), options);
			RegisterFormClass(typeof(RestorePlanGiveNameForm), options);
			RegisterFormClass(typeof(RestorePlanSelectSourceForm), options);
			RegisterFormClass(typeof(SchedulablePlanForm<Models.RestorePlan>), options);
			RegisterFormClass(typeof(NotificationOptionsForm<Models.RestorePlan>), options);
		}

		public override void OnFormClosed()
		{
			base.OnFormClosed();

			Models.RestorePlan plan = Model as Models.RestorePlan;
			_dao.Refresh(plan);
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
			Console.WriteLine("StorageAccount = {0}", plan.StorageAccount.DisplayName);
			foreach (Models.RestorePlanSourceEntry entry in plan.SelectedSources)
				Console.WriteLine("SelectedSource => #{0}, {1}, {2}, {3}",
					entry.Id, entry.Type.ToString(), entry.Path, entry.Version);
			Console.WriteLine("ScheduleType = {0}", plan.ScheduleType.ToString());

			plan.UpdatedAt = DateTime.UtcNow;

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
