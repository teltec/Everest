﻿using System;
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
		private DBContextScope _dbContextScope = new DBContextScope();
		private Models.BackupPlan Plan = new Models.BackupPlan();

		public NewBackupPlanPresenter() : base()
		{
			Plan.Name = "Testing name";
			Plan.ScheduleType = Models.BackupPlan.ScheduleTypeE.RunManually;
			Model = Plan;
			
			WizardFormOptions options = new WizardFormOptions { DoValidate = false };
			RegisterFormClass(typeof(BackupPlanSelectAccountForm), options);
			RegisterFormClass(typeof(BackupPlanGiveNameForm), options);
			RegisterFormClass(typeof(BackupPlanSelectSourceForm), options);
			RegisterFormClass(typeof(BackupPlanScheduleForm), options);
		}

		public override void OnFinish()
		{
			base.OnFinish();

			Console.WriteLine("Name = {0}", Plan.Name);
			foreach (BackupPlanSourceEntry entry in Plan.SelectedSources)
				Console.WriteLine("SelectedSource => {0} {1}", entry.Type.ToString(), entry.Path);
			Console.WriteLine("IsRunManually = {0}", Plan.IsRunManually);

			Plan.Id = Guid.NewGuid();
			_dbContextScope.BackupPlans.Insert(Plan);
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
