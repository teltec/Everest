using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Backup.Models;
using Teltec.Common.Forms;
using Teltec.Forms.Wizard;

namespace Teltec.Backup.Forms.BackupPlan
{
	sealed class NewBackupPlanPresenter : WizardPresenter
	{
		public DBContextScope DBContextScope { get; set; }

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

		public override void OnFinish()
		{
			base.OnFinish();

			Models.BackupPlan plan = Model as Models.BackupPlan;

			Console.WriteLine("Name = {0}", plan.Name);
			ICloudStorageAccount storageAccount = Models.BackupPlan.GetStorageAccount(plan, DBContextScope);
			Console.WriteLine("StorageAccount = {0}", storageAccount.DisplayName);
			Console.WriteLine("StorageAccountType = {0}", plan.StorageAccountType.ToString());
			foreach (BackupPlanSourceEntry entry in plan.SelectedSources)
				Console.WriteLine("SelectedSource => {0} {1}", entry.Type.ToString(), entry.Path);
			Console.WriteLine("ScheduleType = {0}", plan.ScheduleType.ToString());

			if (IsEditingModel)
			{
				DBContextScope.BackupPlans.Update(plan);
			}
			else
			{
				plan.Id = Guid.NewGuid();
				DBContextScope.BackupPlans.Insert(plan);
			}
			
			try
			{
				DBContextScope.Save();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
			} 
		}
		
	}
}
