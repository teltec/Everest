using NLog;
using System;
using System.Windows.Forms;
using Teltec.Backup.DAO;
using Teltec.Backup.Models;
using Teltec.Forms.Wizard;

namespace Teltec.Backup.Forms.BackupPlan
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
