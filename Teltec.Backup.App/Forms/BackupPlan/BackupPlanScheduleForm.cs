using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Common.Extensions;

namespace Teltec.Backup.App.Forms.BackupPlan
{
	public partial class BackupPlanScheduleForm : Teltec.Forms.Wizard.WizardForm
	{
		private Models.BackupPlan Plan = new Models.BackupPlan();

		public BackupPlanScheduleForm()
		{
			InitializeComponent();

			// Setup data bindings
			this.ModelChangedEvent += (sender, args) =>
			{
				this.Plan = args.Model as Models.BackupPlan;

				// Setup data bindings
				rbtnManual.DataBindings.Clear();
				rbtnManual.DataBindings.Add(new Binding("Checked", this.Model,
					this.GetPropertyName((Models.BackupPlan x) => x.IsRunManually)));
			};
			
		}

		protected override bool IsValid()
		{
			return true;
		}

		protected override void OnBeforeNextOrFinish(object sender, CancelEventArgs e)
		{
			if (DoValidate && !IsValid())
			{
				e.Cancel = true;
				this.ShowErrorMessage("Please, inform a name.");
			}
			base.OnBeforeNextOrFinish(sender, e);
		}
	}
}
