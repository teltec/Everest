using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Backup.Data.DAO;
using Teltec.Common.Extensions;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.Actions
{
	public partial class ExecuteCommandsForm : Teltec.Forms.Wizard.WizardForm
	{
		private readonly AmazonS3AccountRepository _s3dao = new AmazonS3AccountRepository();
		private Models.BackupPlan Plan = new Models.BackupPlan();

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

		public ExecuteCommandsForm()
		{
			InitializeComponent();

			this.ModelChangedEvent += (sender, args) =>
			{
				this.Plan = args.Model as Models.BackupPlan;

				// TODO: pre-fill visual components like textboxes.
			};

			// Setup data bindings
			//cbAmazonS3.DataBindings.Add(new Binding("Enabled", rbtnAbortBeforeActionFailed,
			//	this.GetPropertyName((RadioButton x) => x.Checked)));
			//cbFileSystem.DataBindings.Add(new Binding("Enabled", rbtnFileSystem,
			//	this.GetPropertyName((RadioButton x) => x.Checked)));
		}

		protected override bool IsValid()
		{
			//bool didSelectAccountType = rbtnAbortBeforeActionFailed.Checked || rbtnFileSystem.Checked;
			//
			//bool didSelectAccount = false;
			//if (rbtnAbortBeforeActionFailed.Checked)
			//	didSelectAccount = cbAmazonS3.SelectedIndex > 0;
			//else if (rbtnFileSystem.Checked)
			//	didSelectAccount = cbFileSystem.SelectedIndex > 0;
			//
			//return didSelectAccountType && didSelectAccount;
			return true;
		}

		protected override void OnBeforeNextOrFinish(object sender, CancelEventArgs e)
		{
			if (DoValidate && !IsValid())
			{
				e.Cancel = true;
				this.ShowErrorMessage("Please, select an account.");
			}
			base.OnBeforeNextOrFinish(sender, e);
		}
	}
}