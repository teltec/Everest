using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.Data.Models;
using Teltec.Common.Extensions;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.Actions
{
	public partial class ExecuteCommandsForm : Teltec.Forms.Wizard.WizardForm
	{
		private readonly AmazonS3AccountRepository _s3dao = new AmazonS3AccountRepository();
		private Models.PlanConfig Config = new Models.PlanConfig();
		private Models.PlanActionExecuteCommand BeforeAction = new Models.PlanActionExecuteCommand(PlanTriggerTypeEnum.BEFORE_PLAN_STARTS);
		private Models.PlanActionExecuteCommand AfterAction = new Models.PlanActionExecuteCommand(PlanTriggerTypeEnum.AFTER_PLAN_FINISHES);

		// TODO(jweyrich): At some point, handle OpenFileDialog filters for binary files on other platforms as well.
		private readonly string FileFilter = "All supported files (*.exe, *.bat)|*.exe;*.bat|Executable files (*.exe)|*.exe|Batch files (*.bat)|*.bat";

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

			txtFodBefore.Filter = FileFilter;
			txtFodAfter.Filter = FileFilter;

			this.ModelChangedEvent += (sender, args) =>
			{
				ISchedulablePlan plan = args.Model as Models.ISchedulablePlan;
				Config = plan.Config;

				// NOTE: We currently support actions of type `PlanActionExecuteCommand`, only! This should be improved later!
				IEnumerable<PlanAction> allBefore = Config.FilterActionsByTriggerType(PlanTriggerTypeEnum.BEFORE_PLAN_STARTS);
				IEnumerable<PlanAction> allAfter = Config.FilterActionsByTriggerType(PlanTriggerTypeEnum.AFTER_PLAN_FINISHES);
				if (allBefore.Count() > 0)
					BeforeAction = allBefore.First() as Models.PlanActionExecuteCommand;
				else
					Config.Actions.Add(BeforeAction);

				if (allAfter.Count() > 0)
					AfterAction = allAfter.First() as Models.PlanActionExecuteCommand;
				else
					Config.Actions.Add(AfterAction);

				BeforeAction.PlanConfig = Config;
				AfterAction.PlanConfig = Config;

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
			cbBeforeOperation.DataBindings.Clear();
			cbAfterOperation.DataBindings.Clear();
			cbAbortIfFailed.DataBindings.Clear();
			cbExecuteOnlyIfSuccess.DataBindings.Clear();
			txtFodBefore.DataBindings.Clear();
			txtFodAfter.DataBindings.Clear();
			cbAbortIfFailed.DataBindings.Clear();
			cbExecuteOnlyIfSuccess.DataBindings.Clear();
		}

		private void ModelDefaults()
		{
		}

		private void ModelToForm()
		{
		}

		private void WireBindings()
		{
			cbBeforeOperation.DataBindings.Add(new Binding("Checked", this.BeforeAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.IsEnabled), false, DataSourceUpdateMode.OnPropertyChanged));
			cbAfterOperation.DataBindings.Add(new Binding("Checked", this.AfterAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.IsEnabled), false, DataSourceUpdateMode.OnPropertyChanged));

			txtFodBefore.DataBindings.Add(new Binding("Enabled", this.BeforeAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.IsEnabled), false, DataSourceUpdateMode.OnPropertyChanged));
			txtFodAfter.DataBindings.Add(new Binding("Enabled", this.AfterAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.IsEnabled), false, DataSourceUpdateMode.OnPropertyChanged));

			cbAbortIfFailed.DataBindings.Add(new Binding("Enabled", this.BeforeAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.IsEnabled), false, DataSourceUpdateMode.OnPropertyChanged));
			cbExecuteOnlyIfSuccess.DataBindings.Add(new Binding("Enabled", this.AfterAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.IsEnabled), false, DataSourceUpdateMode.OnPropertyChanged));

			txtFodBefore.DataBindings.Add(new Binding("Text", this.BeforeAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.Command)));

			txtFodAfter.DataBindings.Add(new Binding("Text", this.AfterAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.Command)));

			cbAbortIfFailed.DataBindings.Add(new Binding("Checked", this.BeforeAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.AbortIfExecutionFails), false, DataSourceUpdateMode.OnPropertyChanged));

			cbExecuteOnlyIfSuccess.DataBindings.Add(new Binding("Checked", this.AfterAction,
				this.GetPropertyName((PlanActionExecuteCommand x) => x.ConsiderShouldExecute), false, DataSourceUpdateMode.OnPropertyChanged));
		}

		protected override bool IsValid()
		{
			// TODO(jweyrich): Implement form validation for ExecuteCommandsForm.

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
				this.ShowErrorMessage("You have an invalid command.");
			}
			base.OnBeforeNextOrFinish(sender, e);
		}
	}
}
