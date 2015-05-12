using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Backup.Data.DAO;
using Teltec.Common.Extensions;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.RestorePlan
{
	public partial class RestorePlanSelectBackupPlanForm : Teltec.Forms.Wizard.WizardForm
	{
		private readonly BackupPlanRepository _dao = new BackupPlanRepository();
		private Models.RestorePlan Plan = new Models.RestorePlan();

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

		public RestorePlanSelectBackupPlanForm()
		{
			InitializeComponent();

			this.ModelChangedEvent += (sender, args) =>
			{
				this.Plan = args.Model as Models.RestorePlan;

				if (this.Plan.BackupPlan != null && this.Plan.BackupPlan.Id.HasValue)
				{
					LoadBackupPlans();
					SelectExistingBackupPlan(this.Plan.BackupPlan);
				}
			};
		}

		protected override bool IsValid()
		{
			bool didSelectBackupPlan = cbBackupPlan.SelectedIndex > 0;

			return didSelectBackupPlan;
		}

		protected override void OnBeforeNextOrFinish(object sender, CancelEventArgs e)
		{
			if (DoValidate && !IsValid())
			{
				e.Cancel = true;
				this.ShowErrorMessage("Please, select a Backup Plan.");
			}
			base.OnBeforeNextOrFinish(sender, e);
		}

		private void LoadBackupPlans()
		{
			if (this.cbBackupPlan.Items.Count > 0)
				return;

			var plans = _dao.GetAll();
			plans.Insert(0, new Models.BackupPlan() { Name = "<Create new Backup Plan>" });

			this.cbBackupPlan.DisplayMember = this.GetPropertyName((Models.BackupPlan x) => x.Name);
			this.cbBackupPlan.ValueMember = this.GetPropertyName((Models.BackupPlan x) => x.Id);
			this.cbBackupPlan.DataSource = plans;
		}

		private void SelectExistingBackupPlan(Models.BackupPlan plan)
		{
			this.cbBackupPlan.SelectedValue = plan.Id;
		}

		private void cbBackupPlan_DropDown(object sender, EventArgs e)
		{
			LoadBackupPlans();
		}

		private void cbBackupPlan_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (cbBackupPlan.SelectedIndex == 0)
			{
				MessageBox.Show("Show <Create new Backup Plan> window.");
			}
			else
			{
				Models.RestorePlan plan = Model as Models.RestorePlan;
				plan.BackupPlan = _dao.Get(((Int32?)cbBackupPlan.SelectedValue).Value);
			}
		}
	}
}