using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Common.Extensions;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.RestorePlan
{
	public partial class RestorePlanGiveNameForm : Teltec.Forms.Wizard.WizardForm
	{
		private Models.RestorePlan Plan = new Models.RestorePlan();

		public RestorePlanGiveNameForm()
		{
			InitializeComponent();
			this.ModelChangedEvent += (sender, args) => {
				this.Plan = args.Model as Models.RestorePlan;

				// Setup data bindings
				textBox1.DataBindings.Clear();
				textBox1.DataBindings.Add(new Binding("Text", this.Plan,
					this.GetPropertyName((Models.RestorePlan x) => x.Name)));
			};
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox1.Focus();
		}

		protected override bool IsValid()
		{
			bool emptyName = String.IsNullOrEmpty(this.Plan.Name);
			return !emptyName;
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
