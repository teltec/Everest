using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Common.Controls;
using Teltec.Common.Extensions;
using Models = Teltec.Everest.Data.Models;
//using T = Teltec.Everest.Data.Models.BackupPlan;

namespace Teltec.Everest.App.Forms.Notification
{
	public partial class NotificationOptionsForm<T> : Teltec.Forms.Wizard.WizardForm where T : Models.SchedulablePlan<T>, new()
	{
		private Models.SchedulablePlan<T> Plan = new T();

		public NotificationOptionsForm()
		{
			InitializeComponent();

			{
				StringDictionary vars = new StringDictionary();
				vars.Add("operation", Plan.GetConcretePlanTypeName());
				cbNotificationEnabled.Text = cbNotificationEnabled.Text.ExpandVariables(vars);
			}

			this.ModelChangedEvent += (sender, args) =>
			{
				this.Plan = args.Model as T;

				if (this.Plan.Notification == null)
					this.Plan.Notification = new Models.PlanNotification();

				// Setup data bindings

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
			cbNotificationEnabled.DataBindings.Clear();
			rbtnWhenItFails.DataBindings.Clear();
			rbtnAlways.DataBindings.Clear();

			txtEmailAddress.DataBindings.Clear();
			txtFullName.DataBindings.Clear();
			txtEmailSubject.DataBindings.Clear();
		}

		private void ModelDefaults()
		{
		}

		private void ModelToForm()
		{
			cbNotificationEnabled.Checked = this.Plan.Notification.IsNotificationEnabled;
			rbtnWhenItFails.Checked = this.Plan.Notification.WhenToNotify == Models.PlanNotification.TriggerCondition.FAILED;
			rbtnAlways.Checked = this.Plan.Notification.WhenToNotify == Models.PlanNotification.TriggerCondition.ALWAYS;
			txtEmailAddress.Text = this.Plan.Notification.EmailAddress;
			txtFullName.Text = this.Plan.Notification.FullName;
			txtEmailSubject.Text = this.Plan.Notification.EmailAddress;
		}

		private void WireBindings()
		{
			cbNotificationEnabled.DataBindings.Add(new Binding("Checked", this.Plan.Notification,
				this.GetPropertyName((Models.PlanNotification x) => x.IsNotificationEnabled)));

			rbtnWhenItFails.DataBindings.Add(new Binding("Enabled", this.cbNotificationEnabled,
				this.GetPropertyName((CheckBox x) => x.Checked)));
			rbtnAlways.DataBindings.Add(new Binding("Enabled", this.cbNotificationEnabled,
				this.GetPropertyName((CheckBox x) => x.Checked)));
			txtEmailAddress.DataBindings.Add(new Binding("Enabled", this.cbNotificationEnabled,
				this.GetPropertyName((CheckBox x) => x.Checked)));
			txtFullName.DataBindings.Add(new Binding("Enabled", this.cbNotificationEnabled,
				this.GetPropertyName((CheckBox x) => x.Checked)));
			txtEmailSubject.DataBindings.Add(new Binding("Enabled", this.cbNotificationEnabled,
				this.GetPropertyName((CheckBox x) => x.Checked)));

			rbtnWhenItFails.DataBindings.Add(new EqualsBinding<Models.PlanNotification.TriggerCondition>("Checked", this.Plan.Notification,
				this.GetPropertyName((Models.PlanNotification x) => x.WhenToNotify), Models.PlanNotification.TriggerCondition.FAILED));
			rbtnAlways.DataBindings.Add(new EqualsBinding<Models.PlanNotification.TriggerCondition>("Checked", this.Plan.Notification,
				this.GetPropertyName((Models.PlanNotification x) => x.WhenToNotify), Models.PlanNotification.TriggerCondition.ALWAYS));

			txtEmailAddress.DataBindings.Add(new Binding("Text", this.Plan.Notification,
				this.GetPropertyName((Models.PlanNotification x) => x.EmailAddress)));
			txtFullName.DataBindings.Add(new Binding("Text", this.Plan.Notification,
				this.GetPropertyName((Models.PlanNotification x) => x.FullName)));
			txtEmailSubject.DataBindings.Add(new Binding("Text", this.Plan.Notification,
				this.GetPropertyName((Models.PlanNotification x) => x.Subject)));
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			//rbtnWhenItFails.Focus();
		}

		protected override bool IsValid()
		{
			bool isNotificationEnabled = cbNotificationEnabled.Checked;
			if (isNotificationEnabled)
			{
				bool missingEmailAddress = string.IsNullOrEmpty(txtEmailAddress.Text.Trim());
				bool missingFullName = string.IsNullOrEmpty(txtFullName.Text.Trim());
				bool missingEmailSubject = string.IsNullOrEmpty(txtEmailSubject.Text.Trim());

				if (missingEmailAddress || missingFullName || missingEmailSubject)
					return false;
			}

			bool hasWhenToNotify = rbtnWhenItFails.Checked || rbtnAlways.Checked;

			return hasWhenToNotify;
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

		private void WhenToNotifyChanged(object sender, EventArgs e)
		{
			RadioButton rbtn = sender as RadioButton;

			if (rbtn == rbtnWhenItFails && rbtnWhenItFails.Checked)
			{
				Plan.Notification.WhenToNotify = Models.PlanNotification.TriggerCondition.FAILED;
			}
			else if (rbtn == rbtnAlways && rbtnAlways.Checked)
			{
				Plan.Notification.WhenToNotify = Models.PlanNotification.TriggerCondition.ALWAYS;
			}
		}
	}
}
