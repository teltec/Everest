using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Backup.App.Restore;
using Teltec.Backup.Data.DAO;
using Teltec.Common;
using Teltec.Common.Extensions;
using Teltec.Storage;
using Teltec.Storage.Utils;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.RestorePlan
{
	public partial class RestorePlanViewControl : ObservableUserControl, IDisposable
	{
		private readonly RestorePlanRepository _daoRestorePlan = new RestorePlanRepository();
		private readonly RestoreRepository _daoRestore = new RestoreRepository();

		RestoreOperation RunningRestore = null;
		TransferResults RestoreResults = null;
		bool MustResumeLastRestore = false;

		public bool IsRunning
		{
			get { return RunningRestore != null ? RunningRestore.IsRunning : false; }
		}

		public RestorePlanViewControl()
		{
			InitializeComponent();

			this.ModelChangedEvent += (sender, args) =>
			{
				lblTitle.DataBindings.Clear();
				lblSchedule.DataBindings.Clear();
				lblLastRun.DataBindings.Clear();
				lblLastSuccessfulRun.DataBindings.Clear();

				if (Model == null)
					return;

				Binding lblTitleTextBinding = new Binding("Text", Model,
					this.GetPropertyName((Models.BackupPlan x) => x.Name));
				lblTitleTextBinding.Format += TitleFormatter;

				Binding lblScheduleTextBinding = new Binding("Text", Model,
					this.GetPropertyName((Models.BackupPlan x) => x.ScheduleType));

				Binding lblLastRunTextBinding = new Binding("Text", Model,
					this.GetPropertyName((Models.BackupPlan x) => x.LastRunAt));
				lblLastRunTextBinding.Format += DateTimeOptionalFormatter;

				Binding lblLastSuccessfulRunTextBinding = new Binding("Text", Model,
					this.GetPropertyName((Models.BackupPlan x) => x.LastSuccessfulRunAt));
				lblLastSuccessfulRunTextBinding.Format += DateTimeOptionalFormatter;

				lblTitle.DataBindings.Add(lblTitleTextBinding);
				lblSchedule.DataBindings.Add(lblScheduleTextBinding);
				lblLastRun.DataBindings.Add(lblLastRunTextBinding);
				lblLastSuccessfulRun.DataBindings.Add(lblLastSuccessfulRunTextBinding);

				NewRestoreOperation(this.Model as Models.RestorePlan);
			};
		}

		private void NewRestoreOperation(Models.RestorePlan plan)
		{
			Models.Restore latest = _daoRestore.GetLatestByPlan(plan);
			MustResumeLastRestore = latest != null && latest.NeedsResume();

			// Create new restore or resume the last unfinished one.
			RestoreOperation obj = /* MustResumeLastRestore
				? new ResumeRestoreOperation(latest) as RestoreOperation
				: */ new NewRestoreOperation(plan) as RestoreOperation;

			obj.Updated += (sender2, e2) => UpdateStatsInfo(e2.Status);
			//obj.EventLog = ...
			//obj.TransferListControl = ...

			// IMPORTANT: Dispose before assigning.
			if (RunningRestore != null)
				RunningRestore.Dispose();

			RunningRestore = obj;

			UpdateStatsInfo(RestoreOperationStatus.Unknown);
		}

		#region Binding formatters

		void TitleFormatter(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType != typeof(string))
				return;

			string value = e.Value as string;

			e.Value = string.IsNullOrEmpty(value)
				? "(UNNAMED)"
				: e.Value = value.ToUpper();
		}

		void DateTimeOptionalFormatter(object sender, ConvertEventArgs e)
		{
			if (e.DesiredType != typeof(string))
				return;

			DateTime? dt = e.Value as DateTime?;

			e.Value = dt.HasValue
				? string.Format("{0:yyyy-MM-dd HH:mm:ss zzzz}", dt.Value.ToLocalTime())
				: "Never";
		}

		#endregion

		private void llblEditPlan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var presenter = new NewRestorePlanPresenter(this.Model as Models.RestorePlan))
			{
				presenter.ShowDialog(this.ParentForm);
			}
		}

		private void llblDeletePlan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Models.RestorePlan plan = Model as Models.RestorePlan;
			_daoRestorePlan.Delete(plan);
			Model = null;
			OnDelete(this, e);
		}

		private void llblRunNow_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (RunningRestore.IsRunning)
			{
				this.llblRunNow.Enabled = false;
				RunningRestore.Cancel();
				this.llblRunNow.Enabled = true;
			}
			else
			{
				this.llblRunNow.Enabled = false;

				// Create new restore operation for every 'Run' click.
				NewRestoreOperation(this.Model as Models.RestorePlan);

				// FIXME: Re-enable before starting the backup because it's not an async task.
				this.llblRunNow.Enabled = true;
				RunningRestore.Start(out RestoreResults);
			}
		}

		public delegate void DeleteEventHandler(object sender, EventArgs e);
		public event DeleteEventHandler Deleted;

		protected virtual void OnDelete(object sender, EventArgs e)
		{
			if (Deleted != null)
				Deleted(this, e);
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			UpdateDuration(RunningRestore.IsRunning ? RestoreOperationStatus.Updated : RestoreOperationStatus.Finished);
		}

		static string LBL_RUNNOW_RUNNING = "Cancel";
		static string LBL_RUNNOW_STOPPED = "Run now";
		static string LBL_RUNNOW_RESUME = "Resume";
		static string LBL_STATUS_STARTED = "Running";
		static string LBL_STATUS_STOPPED = "Stopped";
		static string LBL_STATUS_INTERRUPTED = "Interrupted";
		static string LBL_STATUS_CANCELED = "Canceled";
		static string LBL_STATUS_FAILED = "Failed";
		static string LBL_STATUS_COMPLETED = "Completed";
		static string LBL_DURATION_STARTED = "Starting...";
		static string LBL_DURATION_INITIAL = "Not started";
		static string LBL_FILES_TRANSFER_STOPPED = "Not started";

		private void UpdateStatsInfo(RestoreOperationStatus status)
		{
			if (RunningRestore == null)
				return;

			switch (status)
			{
				default: throw new ArgumentException("Unhandled status", "status");
				case RestoreOperationStatus.Unknown:
					{
						this.lblSources.Text = RunningRestore.Sources;
						this.lblStatus.Text = MustResumeLastRestore ? LBL_STATUS_INTERRUPTED : LBL_STATUS_STOPPED;
						this.llblRunNow.Text = MustResumeLastRestore ? LBL_RUNNOW_RESUME : LBL_RUNNOW_STOPPED;
						this.lblFilesTransferred.Text = LBL_FILES_TRANSFER_STOPPED;
						this.lblDuration.Text = LBL_DURATION_INITIAL;
						break;
					}
				case RestoreOperationStatus.Started:
				case RestoreOperationStatus.Resumed:
					{
						Assert.IsNotNull(RestoreResults);
						this.lblSources.Text = RunningRestore.Sources;
						this.llblRunNow.Text = LBL_RUNNOW_RUNNING;
						this.lblStatus.Text = LBL_STATUS_STARTED;
						this.lblDuration.Text = LBL_DURATION_STARTED;
						this.lblFilesTransferred.Text = string.Format("{0} of {1}",
							RestoreResults.Stats.Completed, RestoreResults.Stats.Total);

						this.llblEditPlan.Enabled = false;
						this.llblDeletePlan.Enabled = false;

						timer1.Enabled = true;
						timer1.Start();
						break;
					}
				case RestoreOperationStatus.ScanningFilesStarted:
					{
						this.lblSources.Text = "Scanning files...";
						break;
					}
				case RestoreOperationStatus.ScanningFilesFinished:
					{
						break;
					}
				case RestoreOperationStatus.ProcessingFilesStarted:
					{
						this.lblSources.Text = "Processing files...";
						break;
					}
				case RestoreOperationStatus.ProcessingFilesFinished:
					{
						this.lblSources.Text = RunningRestore.Sources;
						this.lblFilesTransferred.Text = string.Format("{0} of {1}",
							RestoreResults.Stats.Completed, RestoreResults.Stats.Total);
						break;
					}
				case RestoreOperationStatus.Finished:
					{
						UpdateDuration(status);
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = LBL_STATUS_COMPLETED;

						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;

						timer1.Stop();
						timer1.Enabled = false;

						// Update timestamps.
						Models.RestorePlan plan = Model as Models.RestorePlan;
						plan.LastRunAt = plan.LastSuccessfulRunAt = DateTime.UtcNow;
						_daoRestorePlan.Update(plan);
						break;
					}
				case RestoreOperationStatus.Updated:
					{
						this.lblFilesTransferred.Text = string.Format("{0} of {1}",
							RestoreResults.Stats.Completed, RestoreResults.Stats.Total);
						break;
					}
				case RestoreOperationStatus.Failed:
				case RestoreOperationStatus.Canceled:
					{
						UpdateDuration(status);

						this.lblSources.Text = RunningRestore.Sources;
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = status == RestoreOperationStatus.Canceled ? LBL_STATUS_CANCELED : LBL_STATUS_FAILED;

						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;

						timer1.Stop();
						timer1.Enabled = false;

						// Update timestamps.
						Models.RestorePlan plan = Model as Models.RestorePlan;
						plan.LastRunAt = DateTime.UtcNow;
						_daoRestorePlan.Update(plan);
						break;
					}
			}
		}

		private void UpdateDuration(RestoreOperationStatus status)
		{
			Assert.IsNotNull(RunningRestore);
			var duration = !status.IsEnded()
				? DateTime.UtcNow - RunningRestore.StartedAt.Value
				: RunningRestore.FinishedAt.Value - RunningRestore.StartedAt.Value;
			lblDuration.Text = TimeSpanUtils.GetReadableTimespan(duration);
		}

		#region Model

		[
		Bindable(true),
		System.ComponentModel.Category("Data"),
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

		public event ModelChangedEventHandler ModelChangedEvent;

		protected virtual void OnModelChanged(object sender, ModelChangedEventArgs e)
		{
			if (ModelChangedEvent != null)
				ModelChangedEvent(this, e);
		}

		#endregion

		#region Panel collapsing

		private bool Collapsed = false;

		private void panelTitle_MouseClick(object sender, MouseEventArgs e)
		{
			Collapsed = !Collapsed;
			if (Collapsed)
			{
				this.Controls.Remove(this.panelContents);
				this.Size = new System.Drawing.Size(this.Size.Width, this.Size.Height - this.panelContents.Size.Height);
			}
			else
			{
				this.Controls.Add(this.panelContents);
				this.Size = new System.Drawing.Size(this.Size.Width, this.Size.Height + this.panelContents.Size.Height);
			}
		}

		#endregion

		//private void eventLog1_EntryWritten(object sender, System.Diagnostics.EntryWrittenEventArgs e)
		//{
		//	string message = string.Format("{0:o} {1} {2}",
		//		e.Entry.TimeWritten, e.Entry.EntryType.ToString().ToUpper(),
		//		e.Entry.Message);
		//	listBox1.Items.Add(message);
		//	// Auto-scroll
		//	listBox1.TopIndex = listBox1.Items.Count - 1;
		//}

		#region Dispose Pattern Implementation

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
				if (RunningRestore != null)
				{
					RunningRestore.Dispose();
					RunningRestore = null;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
