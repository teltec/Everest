/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Common.Extensions;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.BackupPlan
{
	public partial class BackupPlanPurgeOptionsForm : Teltec.Forms.Wizard.WizardForm
	{
		private Models.BackupPlan Plan = new Models.BackupPlan();

		public BackupPlanPurgeOptionsForm()
		{
			InitializeComponent();
			this.ModelChangedEvent += (sender, args) => {
				this.Plan = args.Model as Models.BackupPlan;

				if (this.Plan.PurgeOptions == null)
					this.Plan.PurgeOptions = new Models.BackupPlanPurgeOptions();

				// Setup data bindings
				switch (this.Plan.PurgeOptions.PurgeType)
				{
					case Models.BackupPlanPurgeTypeEnum.DEFAULT:
						rbtnDefault.Checked = true;
						break;
					case Models.BackupPlanPurgeTypeEnum.CUSTOM:
						rbtnCustom.Checked = true;
						break;
				}

				// Remove all data bindings
				ClearBindings();

				// Load Model default values
				ModelDefaults();

				// Setup Form state based on Model
				ModelToForm();

				// Setup data bindings between Form <=> Model
				WireBindings();
			};
		}

		private void ClearBindings()
		{
			rbtnDefault.DataBindings.Clear();
			rbtnCustom.DataBindings.Clear();
			cbEnabledKeepNumberOfVersions.DataBindings.Clear();
			nudNumberOfVersionsToKeep.DataBindings.Clear();
		}

		private void ModelDefaults()
		{
		}

		private void ModelToForm()
		{
			rbtnDefault.Checked = this.Plan.PurgeOptions.IsTypeDefault;

			rbtnCustom.Checked = this.Plan.PurgeOptions.IsTypeCustom;
				cbEnabledKeepNumberOfVersions.Enabled = rbtnCustom.Checked;
					nudNumberOfVersionsToKeep.Enabled = cbEnabledKeepNumberOfVersions.Enabled;
					nudNumberOfVersionsToKeep.Value = this.Plan.PurgeOptions.NumberOfVersionsToKeep;
		}

		private void WireBindings()
		{
			rbtnDefault.DataBindings.Add(new Binding("Checked", this.Plan.PurgeOptions,
				this.GetPropertyName((Models.BackupPlanPurgeOptions x) => x.IsTypeDefault)));
			rbtnCustom.DataBindings.Add(new Binding("Checked", this.Plan.PurgeOptions,
				this.GetPropertyName((Models.BackupPlanPurgeOptions x) => x.IsTypeCustom)));

			cbEnabledKeepNumberOfVersions.DataBindings.Add(new Binding("Enabled", rbtnCustom,
				this.GetPropertyName((RadioButton x) => x.Checked)));
			nudNumberOfVersionsToKeep.DataBindings.Add(new Binding("Enabled", rbtnCustom,
				this.GetPropertyName((RadioButton x) => x.Checked)));

			// Bindings that may change the model
			cbEnabledKeepNumberOfVersions.DataBindings.Add(new Binding("Checked", this.Plan.PurgeOptions,
				this.GetPropertyName((Models.BackupPlanPurgeOptions x) => x.EnabledKeepNumberOfVersions)));
			nudNumberOfVersionsToKeep.DataBindings.Add(new Binding("Value", this.Plan.PurgeOptions,
				this.GetPropertyName((Models.BackupPlanPurgeOptions x) => x.NumberOfVersionsToKeep)));
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			//rbtnDefault.Focus();
		}

		protected override bool IsValid()
		{
			bool didSelectType = rbtnDefault.Checked || rbtnCustom.Checked;

			return didSelectType;
		}

		protected override void OnBeforeNextOrFinish(object sender, CancelEventArgs e)
		{
			if (DoValidate && !IsValid())
			{
				e.Cancel = true;
				this.ShowErrorMessage("Please, inform the required fields.");
			}
			base.OnBeforeNextOrFinish(sender, e);
		}

		private void PurgeTypeChanged(object sender, EventArgs e)
		{
			RadioButton rbtn = sender as RadioButton;

			if (rbtn == rbtnDefault && rbtnDefault.Checked)
			{
				Plan.PurgeOptions.PurgeType = Models.BackupPlanPurgeTypeEnum.DEFAULT;
			}
			else if (rbtn == rbtnCustom && rbtnCustom.Checked)
			{
				Plan.PurgeOptions.PurgeType = Models.BackupPlanPurgeTypeEnum.CUSTOM;
			}
		}
	}
}
