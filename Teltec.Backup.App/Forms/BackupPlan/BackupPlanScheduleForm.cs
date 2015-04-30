using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Teltec.Common.Extensions;
using System.Data;
using Teltec.Backup.App.Forms.Schedule;
using Teltec.Backup.App.DAO;

namespace Teltec.Backup.App.Forms.BackupPlan
{
	public partial class BackupPlanScheduleForm : Teltec.Forms.Wizard.WizardForm
	{
		private readonly PlanScheduleRepository daoSchedule = new PlanScheduleRepository();

		private Models.BackupPlan Plan = new Models.BackupPlan();

		public BackupPlanScheduleForm()
		{
			InitializeComponent();

			// Setup data bindings
			this.ModelChangedEvent += (sender, args) =>
			{
				this.Plan = args.Model as Models.BackupPlan;

				//
				// Setup data bindings
				//
				rbtnManual.DataBindings.Clear();
				rbtnSpecific.DataBindings.Clear();
				rbtnRecurring.DataBindings.Clear();
				dtpSpecificDate.DataBindings.Clear();
				dtpSpecificTime.DataBindings.Clear();

				// Bindings related to radio buttons
				rbtnManual.DataBindings.Add(new Binding("Checked", this.Plan,
					this.GetPropertyName((Models.BackupPlan x) => x.IsRunManually)));
				rbtnSpecific.DataBindings.Add(new Binding("Checked", this.Plan,
					this.GetPropertyName((Models.BackupPlan x) => x.IsSpecific)));
				rbtnRecurring.DataBindings.Add(new Binding("Checked", this.Plan,
					this.GetPropertyName((Models.BackupPlan x) => x.IsRecurring)));
				dtpSpecificDate.DataBindings.Add(new Binding("Enabled", rbtnSpecific,
					this.GetPropertyName((RadioButton x) => x.Checked)));
				dtpSpecificTime.DataBindings.Add(new Binding("Enabled", rbtnSpecific,
					this.GetPropertyName((RadioButton x) => x.Checked)));
				llblEditSchedule.DataBindings.Add(new Binding("Enabled", rbtnRecurring,
					this.GetPropertyName((RadioButton x) => x.Checked)));
				
				// Binding for `dtpSpecificDate` <=> `this.Plan.Schedule.ProxyOccursSpecificallyAtDate`.
				dtpSpecificDate.DataBindings.Add(new Binding("Value", this.Plan.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.ProxyOccursSpecificallyAtDate)));
				// Binding for `dtpSpecificTime` <=> `this.Plan.Schedule.ProxyOccursSpecificallyAtTime`.
				dtpSpecificTime.DataBindings.Add(new Binding("Value", this.Plan.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.ProxyOccursSpecificallyAtTime)));
			};
		}

		protected override bool IsValid()
		{
			return Plan != null && Plan.Schedule.IsValid();
		}

		protected override void OnBeforeNextOrFinish(object sender, CancelEventArgs e)
		{
			if (rbtnManual.Checked)
			{
				Plan.ScheduleType = Models.BackupPlan.EScheduleType.RunManually;
			}
			else if (rbtnSpecific.Checked)
			{
				Plan.ScheduleType = Models.BackupPlan.EScheduleType.Specific;
			}
			else if (rbtnRecurring.Checked)
			{
				Plan.ScheduleType = Models.BackupPlan.EScheduleType.Recurring;
			}

			if (DoValidate && !IsValid())
			{
				e.Cancel = true;
				this.ShowErrorMessage("Please, select a scheduling option.");
			}
			base.OnBeforeNextOrFinish(sender, e);
		}

		private void OpenScheduleRecurringOptionsDialog()
		{
			using (var form = new ScheduleRecurringOptionsForm())
			{
				form.Owner = this;
				form.CancelEvent += form_CancelEvent;
				form.ConfirmEvent += form_ConfirmEvent;
				form.Model = this.Plan.Schedule;
				form.DoValidate = base.DoValidate;
				form.Model = Plan.Schedule;
				form.ShowDialog(this);
			}
		}

		void form_ConfirmEvent(Form sender, CancelEventArgs e)
		{
			// TODO: save instance? The cascade should take care of it on `NewBackupPlanPresenter` and others.
		}

		void form_CancelEvent(Form sender, EventArgs e)
		{
			daoSchedule.Refresh(Plan.Schedule);
		}

		private void llblEditSchedule_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenScheduleRecurringOptionsDialog();
		}

		private void rbtnRecurring_CheckedChanged(object sender, EventArgs e)
		{
			RadioButton rdbtn = sender as RadioButton;
			if (rdbtn != null && rdbtn.Checked)
				OpenScheduleRecurringOptionsDialog();
		}
	}

	public static class TimeSpanToDateTimeConverter
	{
		static public void TimeSpanToDateTime(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(DateTime))
			{
				TimeSpan time = (TimeSpan)e.Value;
				e.Value = new DateTime(time.Ticks);
			}
		}
	}

	public static class DateTimeToTimeSpanConverter
	{
		static public void DateTimeToTimeSpan(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(TimeSpan))
			{
				DateTime dt = (DateTime)e.Value;
				e.Value = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
			}
		}
	}
}
