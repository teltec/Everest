using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltec.Forms.Wizard;

namespace Teltec.Backup.Forms.BackupPlan
{
	class NewBackupPlanPresenter : WizardPresenter
	{
		public NewBackupPlanPresenter() : base()
		{
			RegisterFormClass(typeof(BackupPlanAccountSelectionForm));
			RegisterFormClass(typeof(BackupPlanAccountSelectionForm));
			RegisterFormClass(typeof(BackupPlanAccountSelectionForm));
		}
	}
}
