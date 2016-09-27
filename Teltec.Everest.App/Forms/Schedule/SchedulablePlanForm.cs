/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Everest.Data.DAO;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.Schedule
{
	public partial class SchedulablePlanForm<T> : Teltec.Forms.Wizard.WizardForm where T : Models.ISchedulablePlan
	{
		private readonly PlanScheduleRepository daoSchedule = new PlanScheduleRepository();

		private T Plan = (T)Activator.CreateInstance(typeof(T));

		public SchedulablePlanForm()
		{
			InitializeComponent();

			// Setup data bindings
			this.ModelChangedEvent += (sender, args) =>
			{
				this.Plan = (T)args.Model;

				//
				// Default values
				//
				if (Plan.Schedule.ScheduleType == Models.ScheduleTypeEnum.UNDEFINED)
					Plan.Schedule.ScheduleType = Models.ScheduleTypeEnum.RUN_MANUALLY;

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
				// TODO: Write a formatter/parser to convert between UTC <=> local.
				dtpSpecificDate.DataBindings.Add(new Binding("Value", this.Plan.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.ProxyOccursSpecificallyAtDate)));
				// Binding for `dtpSpecificTime` <=> `this.Plan.Schedule.ProxyOccursSpecificallyAtTime`.
				// TODO: Write a formatter/parser to convert between UTC <=> local.
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
			if (DoValidate && !IsValid())
			{
				e.Cancel = true;
				this.ShowErrorMessage("Please, correct your scheduling options.");
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

		void form_ConfirmEvent(object sender, CancelEventArgs e)
		{
			// TODO: save instance? The cascade should take care of it on `NewBackupPlanPresenter` and others.
		}

		void form_CancelEvent(object sender, EventArgs e)
		{
			daoSchedule.Refresh(Plan.Schedule);
		}

		private void llblEditSchedule_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			OpenScheduleRecurringOptionsDialog();
		}

		private void ScheduleTypeChanged(object sender, EventArgs e)
		{
			RadioButton rbtn = sender as RadioButton;

			if (rbtn == rbtnManual && rbtnManual.Checked)
			{
				Plan.ScheduleType = Models.ScheduleTypeEnum.RUN_MANUALLY;
				Plan.Schedule.ScheduleType = Models.ScheduleTypeEnum.RUN_MANUALLY;
			}
			else if (rbtn == rbtnSpecific && rbtnSpecific.Checked)
			{
				Plan.ScheduleType = Models.ScheduleTypeEnum.SPECIFIC;
				Plan.Schedule.ScheduleType = Models.ScheduleTypeEnum.SPECIFIC;
				if (!Plan.Schedule.OccursSpecificallyAt.HasValue)
				{
					Plan.Schedule.OccursSpecificallyAt = DateTimeUtils.RoundUpToQuarterHours(DateTime.UtcNow);
				}
			}
			else if (rbtn == rbtnRecurring && rbtnRecurring.Checked)
			{
				Plan.ScheduleType = Models.ScheduleTypeEnum.RECURRING;
				Plan.Schedule.ScheduleType = Models.ScheduleTypeEnum.RECURRING;
				OpenScheduleRecurringOptionsDialog();
			}
		}
	}

	//public static class TimeSpanToDateTimeConverter
	//{
	//	static public void TimeSpanToDateTime(object sender, ConvertEventArgs e)
	//	{
	//		if (e.DesiredType == typeof(DateTime))
	//		{
	//			TimeSpan time = (TimeSpan)e.Value;
	//			e.Value = new DateTime(time.Ticks);
	//		}
	//	}
	//}

	//public static class DateTimeToTimeSpanConverter
	//{
	//	static public void DateTimeToTimeSpan(object sender, ConvertEventArgs e)
	//	{
	//		if (e.DesiredType == typeof(TimeSpan))
	//		{
	//			DateTime dt = (DateTime)e.Value;
	//			e.Value = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
	//		}
	//	}
	//}
}
