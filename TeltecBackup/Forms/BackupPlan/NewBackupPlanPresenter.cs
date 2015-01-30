using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Backup.Models;
using Teltec.Common.Forms;
using Teltec.Forms.Wizard;

namespace Teltec.Backup.Forms.BackupPlan
{
	sealed class NewBackupPlanPresenter : WizardPresenter
	{
		private Models.BackupPlan Plan = new Models.BackupPlan();

		public NewBackupPlanPresenter() : base()
		{
			Plan.Name = "Testing name";
			Plan.RunManually = true;
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
			Console.WriteLine("RunManually = {0}", Plan.RunManually);
		}
		
	}
}
