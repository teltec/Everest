using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Forms.Wizard;

namespace Teltec.Backup.Forms.BackupPlan
{
	sealed class NewBackupPlanPresenter : WizardPresenter
	{
		public NewBackupPlanPresenter() : base()
		{
			var plan = new Models.BackupPlan();
			plan.Name = "Testing name";
			Model = plan;
			RegisterFormClass(typeof(BackupPlanSelectAccountForm));
			RegisterFormClass(typeof(BackupPlanGiveNameForm));
			RegisterFormClass(typeof(BackupPlanSelectSourceForm));
		}
	}
}
