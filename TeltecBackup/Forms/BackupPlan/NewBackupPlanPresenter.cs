using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Backup.Models;
using Teltec.Common.Forms;
using Teltec.Forms.Wizard;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Teltec.Common.Extensions;

namespace Teltec.Backup.Forms.BackupPlan
{
	sealed class NewBackupPlanPresenter : WizardPresenter
	{
		private readonly DBContextScope _dbContextScope = new DBContextScope();

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
			ICloudStorageAccount storageAccount = Models.BackupPlan.GetStorageAccount(plan, _dbContextScope);
			Console.WriteLine("StorageAccount = {0}", storageAccount.DisplayName);
			Console.WriteLine("StorageAccountType = {0}", plan.StorageAccountType.ToString());
			foreach (BackupPlanSourceEntry entry in plan.SelectedSources)
				Console.WriteLine("SelectedSource => #{0}, {1}, {1}", entry.Id, entry.Type.ToString(), entry.Path);
			Console.WriteLine("ScheduleType = {0}", plan.ScheduleType.ToString());

			if (IsEditingModel)
			{
				//DbPropertyValues original = _dbContextScope.Context.Entry(plan).OriginalValues;
				//DbPropertyValues current = _dbContextScope.Context.Entry(plan).CurrentValues;
				//string propertyName = this.GetPropertyName((Models.BackupPlan x) => x.SelectedSources);
				//object persisted = original.GetValue<object>(propertyName);
				//_dbContextScope.Context.Entry(persisted).State = EntityState.Deleted;
				
				//foreach (var obj in plan.SelectedSources)
				//	_dbContextScope.Context.Entry(obj).State = EntityState.Added;
				_dbContextScope.BackupPlans.Update(plan);
			}
			else
			{
				plan.Id = Guid.NewGuid();
				_dbContextScope.BackupPlans.Insert(plan);
			}
			
			try
			{
				_dbContextScope.Save();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
			} 
		}
		
	}
}
