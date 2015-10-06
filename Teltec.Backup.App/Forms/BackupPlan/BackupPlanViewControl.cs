using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.PlanExecutor.Backup;
using Teltec.Common;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.BackupPlan
{
	public partial class BackupPlanViewControl : ObservableUserControl, IDisposable
	{
		private readonly BackupPlanRepository _daoBackupPlan = new BackupPlanRepository();
		private readonly BackupRepository _daoBackup = new BackupRepository();
		//private OperationProgressWatcher Watcher = new OperationProgressWatcher(50052);

		BackupOperation RunningOperation = null;
		TransferResults OperationResults = null;
		bool MustResumeLastOperation = false;

		public bool IsRunning
		{
			get { return RunningOperation != null ? RunningOperation.IsRunning : false; }
		}

		public BackupPlanViewControl()
		{
			InitializeComponent();

			/*
			EventDispatcher dispatcher = new EventDispatcher();

			Watcher.Subscribe((BackupUpdateMsg msg) =>
			{
				if (this.Model == null)
					return;

				Models.BackupPlan plan = this.Model as Models.BackupPlan;

				// Only process messages that are related to the plan associated with this control.
				if (msg.PlanId != plan.Id.Value)
					return;

				// IMPORTANT: Always invoke from Main thread!
				dispatcher.Invoke(() => { ProcessRemoteMessage(msg); });
			});
			*/

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

				NewBackupOperation(this.Model as Models.BackupPlan);
			};
		}

		private void NewBackupOperation(Models.BackupPlan plan)
		{
			Models.Backup latest = _daoBackup.GetLatestByPlan(plan);
			MustResumeLastOperation = latest != null && latest.NeedsResume();

			// Create new backup or resume the last unfinished one.
			BackupOperation obj = MustResumeLastOperation
				? new ResumeBackupOperation(latest) as BackupOperation
				: new NewBackupOperation(plan) as BackupOperation;

			obj.Updated += (sender2, e2) => UpdateStatsInfo(e2.Status);
			//obj.EventLog = ...
			//obj.TransferListControl = ...

			// IMPORTANT: Dispose before assigning.
			if (RunningOperation != null)
				RunningOperation.Dispose();

			RunningOperation = obj;

			UpdateStatsInfo(BackupOperationStatus.Unknown);
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
			Models.BackupPlan plan = this.Model as Models.BackupPlan;

			using (var presenter = new NewBackupPlanPresenter(plan))
			{
				presenter.ShowDialog(this.ParentForm);
			}

			// Reload after edit is complete.
			plan.InvalidateCachedSelectedSourcesAsDelimitedString();
			UpdateStatsInfo(BackupOperationStatus.Unknown);
		}

		private void llblDeletePlan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Models.BackupPlan plan = Model as Models.BackupPlan;

			DialogResult result = MessageBox.Show(
				"Are you sure you want to delete this plan?",
				string.Format("Deleting {0}", plan.Name),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1);

			if (result == DialogResult.Yes)
			{
				_daoBackupPlan.Delete(plan);
				Model = null;
				OnDelete(this, e);
			}
		}

		private void llblRunNow_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (RunningOperation.IsRunning)
			{
				this.llblRunNow.Enabled = false;
				RunningOperation.Cancel();
				this.llblRunNow.Enabled = true;
			}
			else
			{
				this.llblRunNow.Enabled = false;

				// Create new backup operation for every 'Run' click.
				NewBackupOperation(this.Model as Models.BackupPlan);

				// FIXME: Re-enable before starting the backup because it's not an async task.
				this.llblRunNow.Enabled = true;

				RunningOperation.Start(out OperationResults);
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
			UpdateDuration(RunningOperation.IsRunning ? BackupOperationStatus.Updated : BackupOperationStatus.Finished);
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

#if false
		private void ProcessRemoteMessage(BackupUpdateMsg msg)
		{
			if (Model == null)
				return;

			Models.BackupPlan plan = this.Model as Models.BackupPlan;

			if (msg.IsAssignableFrom(typeof(BackupUpdateMsg)))
			{
				BackupOperationStatus opStatus = (BackupOperationStatus)Enum.ToObject(
					typeof(BackupOperationStatus), msg.OperationStatus);

				switch (opStatus)
				{
					case BackupOperationStatus.Updated:
						{
							RemoteBackupOperation operation = RunningOperation as RemoteBackupOperation;
							msg.TransferResults.CopyTo(OperationResults);
							break;
						}
					case BackupOperationStatus.Started:
					case BackupOperationStatus.Resumed:
						{
							RemoteBackupOperation operation = new RemoteBackupOperation(plan);
							operation.RemoteBackup.DidStartAt(msg.StartedAt);
							operation.IsRunning = true;
							RunningOperation = operation;
							OperationResults = new TransferResults();
							break;
						}
					case BackupOperationStatus.Finished:
						{
							RemoteBackupOperation operation = RunningOperation as RemoteBackupOperation;
							operation.IsRunning = false;
							operation.RemoteBackup.DidCompleteAt(msg.FinishedAt);
							break;
						}
					case BackupOperationStatus.Failed:
						{
							RemoteBackupOperation operation = RunningOperation as RemoteBackupOperation;
							operation.IsRunning = false;
							operation.RemoteBackup.DidFailAt(msg.FinishedAt);
							break;
						}
					case BackupOperationStatus.Canceled:
						{
							RemoteBackupOperation operation = RunningOperation as RemoteBackupOperation;
							operation.IsRunning = false;
							operation.RemoteBackup.WasCanceledAt(msg.FinishedAt);
							break;
						}
				}

				UpdateStatsInfo(opStatus, true);
			}
		}
#endif

		private void UpdateStatsInfo(BackupOperationStatus status, bool runningRemotely = false)
		{
			if (RunningOperation == null)
				return;

			switch (status)
			{
				default: throw new ArgumentException("Unhandled status", "status");
				case BackupOperationStatus.Unknown:
					{
						this.lblSources.Text = RunningOperation.Sources;
						this.lblStatus.Text = MustResumeLastOperation ? LBL_STATUS_INTERRUPTED : LBL_STATUS_STOPPED;
						this.llblRunNow.Text = MustResumeLastOperation ? LBL_RUNNOW_RESUME : LBL_RUNNOW_STOPPED;
						this.lblFilesTransferred.Text = LBL_FILES_TRANSFER_STOPPED;
						this.lblDuration.Text = LBL_DURATION_INITIAL;
						break;
					}
				case BackupOperationStatus.Started:
				case BackupOperationStatus.Resumed:
					{
						Assert.IsNotNull(OperationResults);
						this.lblSources.Text = RunningOperation.Sources;
						this.llblRunNow.Text = LBL_RUNNOW_RUNNING;
						this.lblStatus.Text = LBL_STATUS_STARTED;
						this.lblDuration.Text = LBL_DURATION_STARTED;
						this.lblFilesTransferred.Text = string.Format("{0} of {1} ({2} / {3})",
							OperationResults.Stats.Completed, OperationResults.Stats.Total,
							FileSizeUtils.FileSizeToString(OperationResults.Stats.BytesCompleted),
							FileSizeUtils.FileSizeToString(OperationResults.Stats.BytesTotal));

						this.llblEditPlan.Enabled = false;
						this.llblDeletePlan.Enabled = false;
						this.llblRestore.Enabled = false;

						timer1.Enabled = true;
						timer1.Start();
						break;
					}
				case BackupOperationStatus.ScanningFilesStarted:
					{
						this.lblSources.Text = "Scanning files...";
						break;
					}
				case BackupOperationStatus.ScanningFilesFinished:
					{
						break;
					}
				case BackupOperationStatus.ProcessingFilesStarted:
					{
						this.lblSources.Text = "Processing files...";
						break;
					}
				case BackupOperationStatus.ProcessingFilesFinished:
					{
						this.lblSources.Text = RunningOperation.Sources;
						this.lblFilesTransferred.Text = string.Format("{0} of {1} ({2} / {3})",
							OperationResults.Stats.Completed, OperationResults.Stats.Total,
							FileSizeUtils.FileSizeToString(OperationResults.Stats.BytesCompleted),
							FileSizeUtils.FileSizeToString(OperationResults.Stats.BytesTotal));
						break;
					}
				case BackupOperationStatus.Finished:
					{
						UpdateDuration(status);
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = LBL_STATUS_COMPLETED;

						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;
						this.llblRestore.Enabled = true;

						timer1.Stop();
						timer1.Enabled = false;

						if (!runningRemotely)
						{
							// Update timestamps.
							Models.BackupPlan plan = Model as Models.BackupPlan;
							plan.LastRunAt = plan.LastSuccessfulRunAt = DateTime.UtcNow;
							_daoBackupPlan.Update(plan);
						}
						break;
					}
				case BackupOperationStatus.Updated:
					{
						this.lblFilesTransferred.Text = string.Format("{0} of {1} ({2} / {3})",
							OperationResults.Stats.Completed, OperationResults.Stats.Total,
							FileSizeUtils.FileSizeToString(OperationResults.Stats.BytesCompleted),
							FileSizeUtils.FileSizeToString(OperationResults.Stats.BytesTotal));
						break;
					}
				case BackupOperationStatus.Failed:
				case BackupOperationStatus.Canceled:
					{
						UpdateDuration(status);

						this.lblSources.Text = RunningOperation.Sources;
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = status == BackupOperationStatus.Canceled ? LBL_STATUS_CANCELED : LBL_STATUS_FAILED;

						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;
						this.llblRestore.Enabled = true;

						timer1.Stop();
						timer1.Enabled = false;

						if (!runningRemotely)
						{
							// Update timestamps.
							Models.BackupPlan plan = Model as Models.BackupPlan;
							plan.LastRunAt = DateTime.UtcNow;
							_daoBackupPlan.Update(plan);
						}
						break;
					}
			}
		}

		private void UpdateDuration(BackupOperationStatus status)
		{
			Assert.IsNotNull(RunningOperation);
			var duration = !status.IsEnded()
				? DateTime.UtcNow - RunningOperation.StartedAt.Value
				: RunningOperation.FinishedAt.Value - RunningOperation.StartedAt.Value;
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
				if (RunningOperation != null)
				{
					RunningOperation.Dispose();
					RunningOperation = null;
				}
				/*
				if (Watcher != null)
				{
					Watcher.Dispose();
					Watcher = null;
				}
				*/
			}
			base.Dispose(disposing);
		}

		#endregion

		private void llblRestore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
