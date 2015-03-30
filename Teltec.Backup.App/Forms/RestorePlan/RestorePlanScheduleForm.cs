using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Common.Extensions;

namespace Teltec.Backup.App.Forms.RestorePlan
{
	public partial class RestorePlanScheduleForm : Teltec.Forms.Wizard.WizardForm
	{
		private Models.RestorePlan Plan = new Models.RestorePlan();

		public RestorePlanScheduleForm()
		{
			InitializeComponent();

			// Setup data bindings
			this.ModelChangedEvent += (sender, args) =>
			{
				this.Plan = args.Model as Models.RestorePlan;

				// Setup data bindings
				rbtnManual.DataBindings.Clear();
				rbtnManual.DataBindings.Add(new Binding("Checked", this.Model,
					this.GetPropertyName((Models.RestorePlan x) => x.IsRunManually)));
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
				this.ShowErrorMessage("Please, select a scheduling option.");
			}
			base.OnBeforeNextOrFinish(sender, e);
		}
	}
}
