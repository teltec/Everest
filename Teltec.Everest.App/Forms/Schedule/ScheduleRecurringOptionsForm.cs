/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Common;
using Teltec.Common.Extensions;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.App.Forms.Schedule
{
	public partial class ScheduleRecurringOptionsForm : ObservableForm
	{
		private Models.PlanSchedule Schedule = new Models.PlanSchedule();
		private ObservableCollection<ObservableWrapper<bool>> WeeklyDaysChecked =
			new ObservableCollection<ObservableWrapper<bool>>()
				{ false, false, false, false, false, false, false };

		private void ModelToForm()
		{
			//
			// Recurrency
			//

			// Reset controls
			rbtnOccursAt.Checked = false;
			rbtnOccursAt.Checked = false;

			// Setup controls
			if (this.Schedule.RecurrencyDailyFrequencyType.HasValue)
			{
				switch (this.Schedule.RecurrencyDailyFrequencyType.Value)
				{
					default: throw new ArgumentException(string.Format("Invalid {0} value for {1}: {2}",
						typeof(Models.DailyFrequencyTypeEnum).FullName,
						this.GetPropertyName((Models.PlanSchedule x) => x.RecurrencyDailyFrequencyType),
						this.Schedule.RecurrencyDailyFrequencyType));
					case Models.DailyFrequencyTypeEnum.SPECIFIC:
						rbtnOccursAt.Checked = true;
						break;
					case Models.DailyFrequencyTypeEnum.EVERY:
						rbtnOccursEvery.Checked = true;
						break;
				}
			}

			//
			// Weekly
			//

			// Reset controls
			cbWeeklyMonday.Checked = false;
			cbWeeklyTuesday.Checked = false;
			cbWeeklyWednesday.Checked = false;
			cbWeeklyThursday.Checked = false;
			cbWeeklyFriday.Checked = false;
			cbWeeklySaturday.Checked = false;
			cbWeeklySunday.Checked = false;

			// Setup controls
			if (this.Schedule.OccursAtDaysOfWeek != null && this.Schedule.OccursAtDaysOfWeek.Count > 0)
			{
				IList<Models.PlanScheduleDayOfWeek> days = this.Schedule.OccursAtDaysOfWeek;
				foreach (var day in days)
				{
					switch (day.DayOfWeek)
					{
						default: throw new ArgumentException(string.Format("Invalid {0} value for {1}: {2}",
							typeof(Models.PlanScheduleDayOfWeek).FullName,
							this.GetPropertyName((Models.PlanScheduleDayOfWeek x) => x.DayOfWeek),
							day.DayOfWeek));
						case DayOfWeek.Monday:
							WeeklyDaysChecked[0] = true;
							//cbWeeklyMonday.Checked = true;
							break;
						case DayOfWeek.Tuesday:
							WeeklyDaysChecked[1] = true;
							//cbWeeklyTuesday.Checked = true;
							break;
						case DayOfWeek.Wednesday:
							WeeklyDaysChecked[2] = true;
							//cbWeeklyWednesday.Checked = true;
							break;
						case DayOfWeek.Thursday:
							WeeklyDaysChecked[3] = true;
							//cbWeeklyThursday.Checked = true;
							break;
						case DayOfWeek.Friday:
							WeeklyDaysChecked[4] = true;
							//cbWeeklyFriday.Checked = true;
							break;
						case DayOfWeek.Saturday:
							WeeklyDaysChecked[5] = true;
							cbWeeklySaturday.Checked = true;
							break;
						case DayOfWeek.Sunday:
							WeeklyDaysChecked[6] = true;
							//cbWeeklySunday.Checked = true;
							break;
					}
				}
			}

			//
			// Monthly
			//

			// Reset controls
			cbMonthlyOccurrence.SelectedIndex = -1;
			cbMonthlyDay.SelectedIndex = -1;

			// Setup controls
			if (this.Schedule.MonthlyOccurrenceType.HasValue)
			{
				int result;
				ConvertMonthlyOccurrenceToSelectedIndex(
					this.Schedule.MonthlyOccurrenceType.Value,
					out result);
				cbMonthlyOccurrence.SelectedIndex = result;
			}

			if (this.Schedule.OccursMonthlyAtDayOfWeek.HasValue)
			{
				int result;
				ConvertDayOfWeekToSelectedIndex(
					this.Schedule.OccursMonthlyAtDayOfWeek.Value,
					out result);
				cbMonthlyDay.SelectedIndex = result;
			}

			//
			// Day of Month
			//

			// Reset controls
			nudDayOfMonth.Value = 1;

			// Setup controls
			if (this.Schedule.OccursAtDayOfMonth.HasValue)
			{
				short dayNumber = this.Schedule.OccursAtDayOfMonth.Value;
				if (dayNumber < 1 || dayNumber > 31)
				{
					throw new ArgumentException(string.Format("Invalid {0} value for {1}: {2}",
						typeof(short?).FullName,
						this.GetPropertyName((Models.PlanSchedule x) => x.OccursAtDayOfMonth),
						this.Schedule.OccursAtDayOfMonth.Value));
				}

				nudDayOfMonth.Value = (decimal)dayNumber;
			}
		}

		private void FormToModel()
		{
			//
			// Weekly
			//
			var days = new[]
			{
				new { Checked = this.WeeklyDaysChecked[0].Value, DayOfWeek = DayOfWeek.Monday    },
				new { Checked = this.WeeklyDaysChecked[1].Value, DayOfWeek = DayOfWeek.Tuesday   },
				new { Checked = this.WeeklyDaysChecked[2].Value, DayOfWeek = DayOfWeek.Wednesday },
				new { Checked = this.WeeklyDaysChecked[3].Value, DayOfWeek = DayOfWeek.Thursday  },
				new { Checked = this.WeeklyDaysChecked[4].Value, DayOfWeek = DayOfWeek.Friday    },
				new { Checked = this.WeeklyDaysChecked[5].Value, DayOfWeek = DayOfWeek.Saturday  },
				new { Checked = this.WeeklyDaysChecked[6].Value, DayOfWeek = DayOfWeek.Sunday    },
			};

			// Remove any saved days.
			this.Schedule.OccursAtDaysOfWeek.Clear();

			// Re-add only the currently selected days.
			foreach (var day in days)
			{
				if (!day.Checked)
					continue;
				var obj = new Models.PlanScheduleDayOfWeek { Schedule = this.Schedule, DayOfWeek = day.DayOfWeek };
				this.Schedule.OccursAtDaysOfWeek.Add(obj);
			}
		}

		private void ModelDefaults()
		{
			//DateTime now = DateTime.Now;
			//TimeSpan nowTime = new TimeSpan(now.Hour, now.Minute, now.Second);
			TimeSpan todayStart = new TimeSpan(0, 0, 0);
			TimeSpan todayEnd = new TimeSpan(23, 59, 59);

			// Frequency
			if (!this.Schedule.RecurrencyFrequencyType.HasValue)
			{
				this.Schedule.RecurrencyFrequencyType = Models.FrequencyTypeEnum.DAILY;
			}

			// Daily frequency
			if (!this.Schedule.RecurrencyDailyFrequencyType.HasValue)
			{
				//rbtnOccursAt.Checked = true; // Select "Occurs at"
				this.Schedule.RecurrencyDailyFrequencyType = Models.DailyFrequencyTypeEnum.SPECIFIC;
			}

			// IMPORTANT: This MUST happen before we change the value of `dtpOccursAt`.
			if (!this.Schedule.RecurrencyTimeUnit.HasValue)
			{
				//dudUnit.SelectedIndex = 1; // Select "hour(s)"
				this.Schedule.RecurrencyTimeUnit = Models.TimeUnitEnum.HOURS;
			}

			// Occurs at
			if (!this.Schedule.RecurrencySpecificallyAtTime.HasValue)
			{
				//dtpOccursAt.Value = now;
				this.Schedule.RecurrencySpecificallyAtTime = todayStart;
			}

			// Occurs every
			if (!this.Schedule.RecurrencyTimeInterval.HasValue)
			{
				nudInterval.Minimum = MinimumInterval;
				nudInterval.Maximum = MaximumInterval;
				//nudInterval.Value = nudInterval.Minimum;
				this.Schedule.RecurrencyTimeInterval = MinimumInterval;
			}

			if (!this.Schedule.RecurrencyWindowStartsAtTime.HasValue)
			{
				//dtpFrom.Value = now.Date;
				this.Schedule.RecurrencyWindowStartsAtTime = todayStart;
			}

			if (!this.Schedule.RecurrencyWindowEndsAtTime.HasValue)
			{
				//dtpTo.Value = now.Date.AddSeconds(60 /* seconds */ * 60 /* minutes */ * 24 /* hours */ - 1); // 23:59:59
				this.Schedule.RecurrencyWindowEndsAtTime = todayEnd;
			}
		}

		private void ClearBindings()
		{
			cbFrequencyType.DataBindings.Clear();
			dtpOccursAt.DataBindings.Clear();
			nudInterval.DataBindings.Clear();
			dudUnit.DataBindings.Clear();
			dtpFrom.DataBindings.Clear();
			dtpTo.DataBindings.Clear();

			// Weekly
			cbWeeklyMonday.DataBindings.Clear();
			cbWeeklyTuesday.DataBindings.Clear();
			cbWeeklyWednesday.DataBindings.Clear();
			cbWeeklyThursday.DataBindings.Clear();
			cbWeeklyFriday.DataBindings.Clear();
			cbWeeklySaturday.DataBindings.Clear();
			cbWeeklySunday.DataBindings.Clear();
			// Monthly
			cbMonthlyOccurrence.DataBindings.Clear();
			cbMonthlyDay.DataBindings.Clear();
			// Day of Month
			nudDayOfMonth.DataBindings.Clear();
		}

		private void WireBindings()
		{
			// Occurs at
			dtpOccursAt.DataBindings.Add(new Binding("Enabled", rbtnOccursAt,
				this.GetPropertyName((RadioButton x) => x.Checked)));

			// Occurs every
			nudInterval.DataBindings.Add(new Binding("Enabled", rbtnOccursEvery,
				this.GetPropertyName((RadioButton x) => x.Checked)));
			dudUnit.DataBindings.Add(new Binding("Enabled", rbtnOccursEvery,
				this.GetPropertyName((RadioButton x) => x.Checked)));
			dtpFrom.DataBindings.Add(new Binding("Enabled", rbtnOccursEvery,
				this.GetPropertyName((RadioButton x) => x.Checked)));
			dtpTo.DataBindings.Add(new Binding("Enabled", rbtnOccursEvery,
				this.GetPropertyName((RadioButton x) => x.Checked)));

			// Control <=> this.Schedule.x
			{
				Binding modelToControlBinding = new Binding("SelectedIndex", this.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.RecurrencyFrequencyType), true);
				modelToControlBinding.Parse += new ConvertEventHandler(ParseToFrequencyTypeEnum); // int => Models.FrequencyTypeEnum?
				modelToControlBinding.Format += new ConvertEventHandler(FormatFrequencyTypeEnum); // Models.FrequencyTypeEnum? => int
				cbFrequencyType.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Value", this.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.RecurrencySpecificallyAtTime), true);
				modelToControlBinding.Parse += new ConvertEventHandler(ParseToStartingTimeSpan); // DateTime => TimeSpan?
				modelToControlBinding.Format += new ConvertEventHandler(FormatStartingTimeSpan); // TimeSpan? => DateTime
				dtpOccursAt.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Value", this.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.RecurrencyTimeInterval), true);
				// The formatter MUST return a value between `nudInterval.Minimum` and `nudInterval.Maximum`.
				modelToControlBinding.Parse += new ConvertEventHandler(ParseToRecurrencyTimeInterval); // decimal => short?
				modelToControlBinding.Format += new ConvertEventHandler(FormatRecurrencyTimeInterval); // short? => decimal
				nudInterval.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("SelectedIndex", this.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.RecurrencyTimeUnit), true);
				modelToControlBinding.Parse += new ConvertEventHandler(ParseToTimeUnitEnum); // int => Models.TimeUnitEnum?
				modelToControlBinding.Format += new ConvertEventHandler(FormatTimeUnitEnum); // Models.TimeUnitEnum? => int
				dudUnit.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Value", this.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.RecurrencyWindowStartsAtTime), true);
				modelToControlBinding.Parse += new ConvertEventHandler(ParseToStartingTimeSpan); // DateTime => TimeSpan?
				modelToControlBinding.Format += new ConvertEventHandler(FormatStartingTimeSpan); // TimeSpan? => DateTime
				dtpFrom.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Value", this.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.RecurrencyWindowEndsAtTime), true);
				modelToControlBinding.Parse += new ConvertEventHandler(ParseToEndingTimeSpan); // DateTime => TimeSpan?
				modelToControlBinding.Format += new ConvertEventHandler(FormatEndingTimeSpan); // TimeSpan? => DateTime
				dtpTo.DataBindings.Add(modelToControlBinding);
			}

			//
			// Weekly
			//
			{
				Binding modelToControlBinding = new Binding("Checked", this.WeeklyDaysChecked[0],
					this.GetPropertyName((ObservableWrapper<bool> x) => x.Value), true);
				cbWeeklyMonday.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Checked", this.WeeklyDaysChecked[1],
					this.GetPropertyName((ObservableWrapper<bool> x) => x.Value), true);
				cbWeeklyTuesday.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Checked", this.WeeklyDaysChecked[2],
					this.GetPropertyName((ObservableWrapper<bool> x) => x.Value), true);
				cbWeeklyWednesday.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Checked", this.WeeklyDaysChecked[3],
					this.GetPropertyName((ObservableWrapper<bool> x) => x.Value), true);
				cbWeeklyThursday.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Checked", this.WeeklyDaysChecked[4],
					this.GetPropertyName((ObservableWrapper<bool> x) => x.Value), true);
				cbWeeklyFriday.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Checked", this.WeeklyDaysChecked[5],
					this.GetPropertyName((ObservableWrapper<bool> x) => x.Value), true);
				cbWeeklySaturday.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("Checked", this.WeeklyDaysChecked[6],
					this.GetPropertyName((ObservableWrapper<bool> x) => x.Value), true);
				cbWeeklySunday.DataBindings.Add(modelToControlBinding);
			}

			//
			// Monthly
			//
			{
				Binding modelToControlBinding = new Binding("SelectedIndex", this.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.MonthlyOccurrenceType), true);
				modelToControlBinding.Parse += new ConvertEventHandler(ParseToMonthlyOccurrence); // int => Models.MonthlyOccurrenceTypeEnum?
				modelToControlBinding.Format += new ConvertEventHandler(FormatMonthlyOccurrence); // Models.MonthlyOccurrenceTypeEnum? => int
				cbMonthlyOccurrence.DataBindings.Add(modelToControlBinding);
			}
			{
				Binding modelToControlBinding = new Binding("SelectedIndex", this.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.OccursMonthlyAtDayOfWeek), true);
				modelToControlBinding.Parse += new ConvertEventHandler(ParseToMonthlyAtDayOfWeek); // int => DayOfWeek?
				modelToControlBinding.Format += new ConvertEventHandler(FormatMonthlyAtDayOfWeek); // DayOfWeek? => int
				cbMonthlyDay.DataBindings.Add(modelToControlBinding);
			}

			//
			// Day of Month
			//
			{
				Binding modelToControlBinding = new Binding("Value", this.Schedule,
					this.GetPropertyName((Models.PlanSchedule x) => x.OccursAtDayOfMonth), true);
				modelToControlBinding.Parse += new ConvertEventHandler(ParseToDayOfMonth); // decimal => short?
				modelToControlBinding.Format += new ConvertEventHandler(FormatDayOfMonth); // short? => decimal
				nudDayOfMonth.DataBindings.Add(modelToControlBinding);
			}
		}

		public ScheduleRecurringOptionsForm()
		{
			InitializeComponent();

			this.CancelEvent += form_CancelEvent;
			this.ConfirmEvent += form_ConfirmEvent;

			// Setup defaults and data bindings
			this.ModelChangedEvent += (sender, args) =>
			{
				this.Schedule = args.Model as Models.PlanSchedule;

				NUnit.Framework.Assert.AreEqual(Models.ScheduleTypeEnum.RECURRING, this.Schedule.ScheduleType);

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

		// Copied verbatim from `Teltec.Forms.Wizard.WizardForm`
		// TODO: avoid code duplication.
		#region Validation

		[
		Bindable(true),
		Category("Validation"),
		DefaultValue(true),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
		]
		protected internal bool _DoValidate = true;
		protected internal bool DoValidate
		{
			get { return _DoValidate; }
			set { _DoValidate = value; }
		}

		protected virtual bool IsValid()
		{
			return true;
		}

		protected virtual void ShowErrorMessage(string caption, string message)
		{
			MessageBox.Show(message, caption);
		}

		protected virtual void ShowErrorMessage(string message)
		{
			MessageBox.Show(message);
		}

		#endregion

		// Copied parts from `Teltec.Forms.Wizard.WizardForm`
		// TODO: avoid code duplication.
		#region Custom properties

		[
		Bindable(true),
		Category("Data"),
		DefaultValue(null),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
		]
		protected object _Model;
		public virtual object Model
		{
			get { return _Model; }
			set
			{
				SetField(ref _Model, value);
				OnModelChanged(this, new ModelChangedEventArgs(_Model));
			}
		}

		#endregion

		// Copied parts from `Teltec.Forms.Wizard.WizardForm`
		// TODO: avoid code duplication.
		#region Custom events

		public class ModelChangedEventArgs : EventArgs
		{
			private object _model;
			public object Model
			{
				get { return _model; }
			}

			public ModelChangedEventArgs(object model)
			{
				_model = model;
			}
		}

		public delegate void ModelChangedEventHandler(object sender, ModelChangedEventArgs e);
		public delegate void FormActionEventHandler(object sender, EventArgs e);
		public delegate void FormCancelableActionEventHandler(object sender, CancelEventArgs e);

		public event ModelChangedEventHandler ModelChangedEvent;
		public event FormActionEventHandler CancelEvent;
		public event FormCancelableActionEventHandler ConfirmEvent;
		public event FormCancelableActionEventHandler BeforeConfirmEvent;

		protected virtual void OnModelChanged(object sender, ModelChangedEventArgs e)
		{
			if (ModelChangedEvent != null)
				ModelChangedEvent(this, e);
		}

		protected virtual void OnCancel(object sender, EventArgs e)
		{
			if (CancelEvent != null)
				CancelEvent(this, e);
		}

		protected virtual void OnConfirm(object sender, CancelEventArgs e)
		{
			if (!e.Cancel && ConfirmEvent != null)
				ConfirmEvent(this, e);
		}

		protected virtual void OnBeforeConfirm(object sender, CancelEventArgs e)
		{
			if (!e.Cancel && BeforeConfirmEvent != null)
				BeforeConfirmEvent(this, e);
		}

		#endregion

		// Copied parts from `Teltec.Forms.Wizard.WizardForm`
		// TODO: avoid code duplication.
		#region Form events

		private void dudUnit_SelectedItemChanged(object sender, EventArgs e)
		{
			switch (dudUnit.SelectedIndex)
			{
				default: throw new ArgumentOutOfRangeException(
					"dudUnit.SelectedIndex", "Value for {0} is out of the defined range", typeof(DomainUpDown).FullName);
				case 0: // "minute(s)"
					this.Schedule.RecurrencyTimeUnit = Models.TimeUnitEnum.MINUTES;
					break;
				case 1: // "hour(s)"
					this.Schedule.RecurrencyTimeUnit = Models.TimeUnitEnum.HOURS;
					break;
			}

			if (this.Schedule.RecurrencyTimeInterval.Value < MinimumInterval)
				this.Schedule.RecurrencyTimeInterval = MinimumInterval;
			else if (this.Schedule.RecurrencyTimeInterval.Value > MaximumInterval)
				this.Schedule.RecurrencyTimeInterval = MaximumInterval;

			nudInterval.Minimum = MinimumInterval;
			nudInterval.Maximum = MaximumInterval;
			nudInterval.Value = nudInterval.Minimum;

		}

		private void FrequencyTypeChanged(object sender, EventArgs e)
		{
			switch (cbFrequencyType.SelectedIndex)
			{
				case 0: // Daily
					panelWeekly.Visible = false;
					panelMonthly.Visible = false;
					panelDayOfMonth.Visible = false;
					break;
				case 1: // Weekly
					panelWeekly.Visible = true;
					panelMonthly.Visible = false;
					panelDayOfMonth.Visible = false;
					break;
				case 2: // Monthly
					panelWeekly.Visible = false;
					panelMonthly.Visible = true;
					panelDayOfMonth.Visible = false;

					//
					// Defaults
					//
					if (!Schedule.MonthlyOccurrenceType.HasValue)
						Schedule.MonthlyOccurrenceType = Models.MonthlyOccurrenceTypeEnum.FIRST;
					if (!Schedule.OccursMonthlyAtDayOfWeek.HasValue)
						Schedule.OccursMonthlyAtDayOfWeek = DayOfWeek.Monday;

					break;
				case 3: // Day of Month
					panelWeekly.Visible = false;
					panelMonthly.Visible = false;
					panelDayOfMonth.Visible = true;

					//
					// Defaults
					//
					if (!Schedule.OccursAtDayOfMonth.HasValue)
						Schedule.OccursAtDayOfMonth = 1;

					break;
			}
		}

		private void DailyFrequencyTypeChanged(object sender, EventArgs e)
		{
			if (rbtnOccursAt.Checked)
			{
				this.Schedule.RecurrencyDailyFrequencyType = Models.DailyFrequencyTypeEnum.SPECIFIC;
			}
			else if (rbtnOccursEvery.Checked)
			{
				this.Schedule.RecurrencyDailyFrequencyType = Models.DailyFrequencyTypeEnum.EVERY;
			}
		}

		//private void DayOfMonthChanged(object sender, EventArgs e)
		//{
		//	if (sender == nudDayOfMonth)
		//	{
		//		this.Schedule.OccursAtDayOfMonth = ;
		//	}
		//}

		private short MinimumInterval
		{
			get
			{
				return this.Schedule.MinimumRecurrencyTimeInterval;
			}
		}

		private short MaximumInterval
		{
			get
			{
				return this.Schedule.MaximumRecurrencyTimeInterval;
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			OnCancel(sender, e);
		}

		private void btnConfirm_Click(object sender, EventArgs e)
		{
			CancelEventArgs args = new CancelEventArgs();
			OnBeforeConfirm(sender, args);
			if (!args.Cancel)
				OnConfirm(sender, args);
		}

		#endregion

		protected virtual void form_CancelEvent(object sender, EventArgs e)
		{
			OnCancel();
		}

		protected virtual void form_ConfirmEvent(object sender, EventArgs e)
		{
			FormToModel();

			if (!this.Schedule.IsRecurringValid())
				this.ShowErrorMessage("Please, correct your scheduling options.");
			else
				OnConfirm();
		}

		protected virtual void CloseAndDipose()
		{
			this.Close();
			this.Dispose();
		}

		public virtual void OnConfirm()
		{
			CloseAndDipose();
		}

		public virtual void OnCancel()
		{
			CloseAndDipose();
		}

		#region Parsing and formatting

		private void ParseToRecurrencyTimeInterval(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(short?))
			{
				decimal v = (decimal)e.Value;
				short newValue = (short)v;
				e.Value = new Nullable<short>(newValue);
			}
		}

		private void FormatRecurrencyTimeInterval(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(decimal))
			{
				short? optional = (short?)e.Value;

				if (optional.HasValue)
				{
					short v = optional.Value;
					e.Value = (decimal)v;
				}
				else
				{
					e.Value = nudInterval.Minimum;
				}
			}
		}

		private void ParseToFrequencyTypeEnum(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(Models.FrequencyTypeEnum?))
			{
				int v = (int)e.Value;

				switch (v)
				{
					default: throw new ArgumentException(string.Format("Invalid value for {0}: {1}",
						 typeof(Models.FrequencyTypeEnum).FullName, e.Value));
					case 0: e.Value = Models.FrequencyTypeEnum.DAILY; break;
					case 1: e.Value = Models.FrequencyTypeEnum.WEEKLY; break;
					case 2: e.Value = Models.FrequencyTypeEnum.MONTHLY; break;
					case 3: e.Value = Models.FrequencyTypeEnum.DAY_OF_MONTH; break;
				}
			}
		}

		private void FormatFrequencyTypeEnum(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(int))
			{
				Models.FrequencyTypeEnum? optional = (Models.FrequencyTypeEnum?)e.Value;

				if (optional.HasValue)
				{
					switch (optional.Value)
					{
						default: throw new ArgumentException(string.Format("Invalid {0} value for SelectedIndex: {1}",
							typeof(Models.FrequencyTypeEnum).FullName, e.Value));
						case Models.FrequencyTypeEnum.DAILY: e.Value = 0; break;
						case Models.FrequencyTypeEnum.WEEKLY: e.Value = 1; break;
						case Models.FrequencyTypeEnum.MONTHLY: e.Value = 2; break;
						case Models.FrequencyTypeEnum.DAY_OF_MONTH: e.Value = 3; break;
					}
				}
				else
				{
					e.Value = 0; // Select "Daily"
				}
			}
		}

		private void ParseToTimeUnitEnum(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(Models.TimeUnitEnum?))
			{
				int v = (int)e.Value;

				switch (v)
				{
					default: throw new ArgumentException(string.Format("Invalid value for {0}: {1}",
						 typeof(Models.TimeUnitEnum).FullName, e.Value));
					case 0: e.Value = new Nullable<Models.TimeUnitEnum>(Models.TimeUnitEnum.MINUTES); break;
					case 1: e.Value = new Nullable<Models.TimeUnitEnum>(Models.TimeUnitEnum.HOURS); break;
				}
			}
		}

		private void FormatTimeUnitEnum(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(int))
			{
				Models.TimeUnitEnum? optional = (Models.TimeUnitEnum?)e.Value;

				if (optional.HasValue)
				{
					switch (optional.Value)
					{
						default: throw new ArgumentException(string.Format("Invalid {0} value for SelectedIndex: {1}",
							typeof(Models.FrequencyTypeEnum).FullName, e.Value));
						case Models.TimeUnitEnum.MINUTES: e.Value = 0; break;
						case Models.TimeUnitEnum.HOURS: e.Value = 1; break;
					}
				}
				else
				{
					e.Value = 1; // Select "hour(s)"
				}
			}
		}

		private void ParseToStartingTimeSpan(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(TimeSpan?))
			{
				DateTime? optional = (DateTime?)e.Value;

				if (optional.HasValue)
				{
					DateTime v = optional.Value;
					e.Value = new TimeSpan(v.Hour, v.Minute, v.Second);
				}
				else
				{
					DateTime v = DateTime.UtcNow;
					e.Value = new TimeSpan(0, 0, 0);
				}
			}
		}

		private void FormatStartingTimeSpan(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(DateTime))
			{
				TimeSpan? optional = (TimeSpan?)e.Value;

				if (optional.HasValue)
				{
					DateTime now = DateTime.UtcNow;
					TimeSpan v = optional.Value;
					DateTime newValue = new DateTime(now.Year, now.Month, now.Day, v.Hours, v.Minutes, v.Seconds);
					e.Value = newValue;
				}
				else
				{
					DateTime now = DateTime.UtcNow;
					e.Value = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
				}
			}
		}

		private void ParseToEndingTimeSpan(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(TimeSpan?))
			{
				DateTime? optional = (DateTime?)e.Value;

				if (optional.HasValue)
				{
					DateTime v = optional.Value;
					e.Value = new TimeSpan(v.Hour, v.Minute, v.Second);
				}
				else
				{
					DateTime v = DateTime.UtcNow;
					e.Value = new TimeSpan(23, 59, 59);
				}
			}
		}

		private void FormatEndingTimeSpan(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(DateTime))
			{
				TimeSpan? optional = (TimeSpan?)e.Value;

				if (optional.HasValue)
				{
					DateTime now = DateTime.UtcNow;
					TimeSpan v = optional.Value;
					DateTime newValue = new DateTime(now.Year, now.Month, now.Day, v.Hours, v.Minutes, v.Seconds);
					e.Value = newValue;
				}
				else
				{
					DateTime now = DateTime.UtcNow;
					e.Value = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
				}
			}
		}

		private void ParseToMonthlyOccurrence(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(Models.MonthlyOccurrenceTypeEnum?))
			{
				Models.MonthlyOccurrenceTypeEnum result;
				ConvertSelectedIndexToMonthlyOccurrence(e.Value, out result);
				e.Value = result;
			}
		}

		private void FormatMonthlyOccurrence(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(int))
			{
				Models.MonthlyOccurrenceTypeEnum? optional = (Models.MonthlyOccurrenceTypeEnum?)e.Value;

				if (optional.HasValue)
				{
					int result;
					ConvertMonthlyOccurrenceToSelectedIndex(optional.Value, out result);
					e.Value = result;
				}
				else
				{
					e.Value = 0; // Select "FIRST"
				}
			}
		}

		private void ConvertMonthlyOccurrenceToSelectedIndex(object input, out int output)
		{
			Models.MonthlyOccurrenceTypeEnum value = (Models.MonthlyOccurrenceTypeEnum)input;
			switch (value)
			{
				default: throw new ArgumentException(string.Format("Invalid {0} value for SelectedIndex: {1}",
					typeof(Models.MonthlyOccurrenceTypeEnum).FullName, value));
				case Models.MonthlyOccurrenceTypeEnum.FIRST: output = 0; break;
				case Models.MonthlyOccurrenceTypeEnum.SECOND: output = 1; break;
				case Models.MonthlyOccurrenceTypeEnum.THIRD: output = 2; break;
				case Models.MonthlyOccurrenceTypeEnum.FOURTH: output = 3; break;
				case Models.MonthlyOccurrenceTypeEnum.PENULTIMATE: output = 4; break;
				case Models.MonthlyOccurrenceTypeEnum.LAST: output = 5; break;
			}
		}

		private void ConvertSelectedIndexToMonthlyOccurrence(object input, out Models.MonthlyOccurrenceTypeEnum output)
		{
			int value = (int)input;
			switch (value)
			{
				default: throw new ArgumentException(string.Format("Invalid {0} value: {1}",
					typeof(Models.MonthlyOccurrenceTypeEnum).FullName, value));
				case 0: output = Models.MonthlyOccurrenceTypeEnum.FIRST; break;
				case 1: output = Models.MonthlyOccurrenceTypeEnum.SECOND; break;
				case 2: output = Models.MonthlyOccurrenceTypeEnum.THIRD; break;
				case 3: output = Models.MonthlyOccurrenceTypeEnum.FOURTH; break;
				case 4: output = Models.MonthlyOccurrenceTypeEnum.PENULTIMATE; break;
				case 5: output = Models.MonthlyOccurrenceTypeEnum.LAST; break;
			}
		}

		private void ParseToMonthlyAtDayOfWeek(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(DayOfWeek?))
			{
				DayOfWeek result;
				ConvertSelectedIndexToDayOfWeek(e.Value, out result);
				e.Value = result;
			}
		}

		private void FormatMonthlyAtDayOfWeek(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(int))
			{
				DayOfWeek? optional = (DayOfWeek?)e.Value;

				if (optional.HasValue)
				{
					int result;
					ConvertDayOfWeekToSelectedIndex(optional.Value, out result);
					e.Value = result;
				}
				else
				{
					e.Value = 0; // Select "Monday"
				}
			}
		}

		private void ConvertDayOfWeekToSelectedIndex(object input, out int output)
		{
			DayOfWeek value = (DayOfWeek)input;
			switch (value)
			{
				default: throw new ArgumentException(string.Format("Invalid {0} value for SelectedIndex: {1}",
					typeof(DayOfWeek).FullName, value));
				case DayOfWeek.Monday:
					output = 0;
					break;
				case DayOfWeek.Tuesday:
					output = 1;
					break;
				case DayOfWeek.Wednesday:
					output = 2;
					break;
				case DayOfWeek.Thursday:
					output = 3;
					break;
				case DayOfWeek.Friday:
					output = 4;
					break;
				case DayOfWeek.Saturday:
					output = 5;
					break;
				case DayOfWeek.Sunday:
					output = 6;
					break;
			}
		}

		private void ConvertSelectedIndexToDayOfWeek(object input, out DayOfWeek output)
		{
			int value = (int)input;
			switch (value)
			{
				default: throw new ArgumentException(string.Format("Invalid {0} value: {1}",
					typeof(DayOfWeek).FullName, value));
				case 0: output = DayOfWeek.Monday; break;
				case 1: output = DayOfWeek.Tuesday; break;
				case 2: output = DayOfWeek.Wednesday; break;
				case 3: output = DayOfWeek.Thursday; break;
				case 4: output = DayOfWeek.Friday; break;
				case 5: output = DayOfWeek.Saturday; break;
				case 6: output = DayOfWeek.Sunday; break;
			}
		}

		private void ParseToDayOfMonth(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(decimal))
			{
				short result;
				ConvertDecimalToDayOfMonth(e.Value, out result);
				e.Value = result;
			}
		}

		private void FormatDayOfMonth(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(short?))
			{
				short? optional = (short?)e.Value;

				if (optional.HasValue)
				{
					decimal result;
					ConvertDayOfMonthToDecimal(optional.Value, out result);
					e.Value = result;
				}
				else
				{
					e.Value = 1; // Default is 1
				}
			}
		}

		private void ConvertDecimalToDayOfMonth(object input, out short output)
		{
			decimal value = (decimal)input;
			if (value < 1 || value > 31)
			{
				throw new ArgumentException(string.Format("Invalid {0} value: {1}",
					typeof(decimal).FullName,
					value));
			}

			output = (short)value;
		}

		private void ConvertDayOfMonthToDecimal(object input, out decimal output)
		{
			short value = (short)input;
			if (value < 1 || value > 31)
			{
				throw new ArgumentException(string.Format("Invalid {0} value: {1}",
					typeof(short).FullName,
					value));
			}

			output = (decimal)value;
		}

		#endregion
	}
}
