using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.Ipc.Protocol;
using Teltec.Backup.Ipc.TcpSocket;
using Teltec.Common;
using Teltec.Common.Utils;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.BackupPlan
{
	public partial class BackupPlanViewControl : ObservableUserControl, IDisposable
	{
		private readonly BackupPlanRepository _daoBackupPlan = new BackupPlanRepository();
		private readonly BackupRepository _daoBackup = new BackupRepository();

		private RemoteOperation CurrentOperation;
		public bool OperationIsRunning { get { return CurrentOperation.IsRunning; } }

		private void AttachEventHandlers()
		{
			// IMPORTANT: These handlers should be detached on Dispose.
			Provider.Handler.OnReportPlanStatus += OnReportPlanStatus;
			Provider.Handler.OnReportPlanProgress += OnReportPlanProgress;
		}

		private void DetachEventHandlers()
		{
			Provider.Handler.OnReportPlanStatus -= OnReportPlanStatus;
			Provider.Handler.OnReportPlanProgress -= OnReportPlanProgress;
		}

		public BackupPlanViewControl()
		{
			InitializeComponent();

			AttachEventHandlers();

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
				if (Model == null)
					return;

				Models.BackupPlan plan = Model as Models.BackupPlan;

				CurrentOperation = new RemoteOperation(this.components, DurationTimer_Tick);
				CurrentOperation.Status = Commands.OperationStatus.NOT_RUNNING;
				CurrentOperation.LastRunAt = plan.LastRunAt;
				CurrentOperation.LastSuccessfulRunAt = plan.LastSuccessfulRunAt;

				this.lblSources.Text = plan.SelectedSourcesAsDelimitedString(", ", 50, "..."); // Duplicate from BackupOperation.cs - Sources property
				this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
				this.llblRunNow.Enabled = false;
				this.lblStatus.Text = "Querying status..."; ;
				this.lblDuration.Text = "Unknown"; ;
				this.lblFilesTransferred.Text = "Unknown"; ;
				this.llblEditPlan.Enabled = false;
				this.llblDeletePlan.Enabled = false;
				this.llblRestore.Enabled = false;
				this.lblLastRun.Text = PlanCommon.Format(CurrentOperation.LastRunAt);
				this.lblLastSuccessfulRun.Text = PlanCommon.Format(CurrentOperation.LastSuccessfulRunAt);
				this.lblTitle.Text = PlanCommon.FormatTitle(plan.Name);
				this.lblSchedule.Text = plan.ScheduleType.ToString();

				CurrentOperation.RequestedInitialInfo = true;
				Provider.Handler.Send(Commands.ServerQueryPlan("backup", plan.Id.Value));
			};
		}

		private void OnReportPlanStatus(object sender, GuiCommandEventArgs e)
		{
			string planType = e.Command.GetArgumentValue<string>("planType");
			if (!planType.Equals("backup"))
				return;

			Models.BackupPlan plan = this.Model as Models.BackupPlan;

			Int32 planId = e.Command.GetArgumentValue<Int32>("planId");
			if (planId != plan.Id)
				return;

			Commands.GuiReportPlanStatus report = e.Command.GetArgumentValue<Commands.GuiReportPlanStatus>("report");
			UpdatePlanInfo(report);
		}

		private void OnReportPlanProgress(object sender, GuiCommandEventArgs e)
		{
			if (!CurrentOperation.GotInitialInfo)
				return;

			string planType = e.Command.GetArgumentValue<string>("planType");
			if (!planType.Equals("backup"))
				return;

			Models.BackupPlan plan = this.Model as Models.BackupPlan;

			Int32 planId = e.Command.GetArgumentValue<Int32>("planId");
			if (planId != plan.Id)
				return;

			Commands.GuiReportPlanProgress progress = e.Command.GetArgumentValue<Commands.GuiReportPlanProgress>("progress");
			UpdatePlanProgress(progress);
		}

		private void UpdatePlanInfo(Commands.GuiReportPlanStatus report)
		{
			CurrentOperation.Status = report.Status;

			switch (report.Status)
			{
				default: return; // TODO(jweyrich): Somehow report unexpected status?
				case Commands.OperationStatus.NOT_RUNNING:
				case Commands.OperationStatus.INTERRUPTED:
					{
						Models.BackupPlan plan = Model as Models.BackupPlan;

						//this.lblSources.Text = report.Sources;
						this.llblRunNow.Text = report.Status == Commands.OperationStatus.NOT_RUNNING
							? LBL_RUNNOW_STOPPED : LBL_RUNNOW_RESUME;
						this.llblRunNow.Enabled = true;
						this.lblStatus.Text = report.Status == Commands.OperationStatus.NOT_RUNNING
							? LBL_STATUS_STOPPED : LBL_STATUS_INTERRUPTED;
						this.lblDuration.Text = LBL_DURATION_INITIAL;
						this.lblFilesTransferred.Text = LBL_FILES_TRANSFER_STOPPED;
						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;
						this.llblRestore.Enabled = true;
						this.lblLastRun.Text = PlanCommon.Format(CurrentOperation.LastRunAt);
						this.lblLastSuccessfulRun.Text = PlanCommon.Format(CurrentOperation.LastSuccessfulRunAt);
						//this.lblTitle.Text = PlanCommon.FormatTitle(plan.Name);
						//this.lblSchedule.Text = plan.ScheduleType.ToString();

						CurrentOperation.GotInitialInfo = true;

						break;
					}
				case Commands.OperationStatus.STARTED:
				case Commands.OperationStatus.RESUMED:
					{
						Models.BackupPlan plan = Model as Models.BackupPlan;

						CurrentOperation.StartedAt = report.StartedAt;
						CurrentOperation.LastRunAt = report.LastRunAt;
						CurrentOperation.LastSuccessfulRunAt = report.LastSuccessfulRunAt;

						this.lblSources.Text = this.lblSources.Text = plan.SelectedSourcesAsDelimitedString(", ", 50, "..."); // Duplicate from BackupOperation.cs - Sources property
						this.llblRunNow.Text = LBL_RUNNOW_RUNNING;
						this.llblRunNow.Enabled = true;
						this.lblStatus.Text = LBL_STATUS_STARTED;
						this.lblDuration.Text = LBL_DURATION_STARTED;
						this.lblFilesTransferred.Text = string.Format("{0} of {1} ({2} / {3})",
							0, 0,
							FileSizeUtils.FileSizeToString(0),
							FileSizeUtils.FileSizeToString(0));
						this.llblEditPlan.Enabled = false;
						this.llblDeletePlan.Enabled = false;
						this.llblRestore.Enabled = false;
						this.lblLastRun.Text = PlanCommon.Format(CurrentOperation.LastRunAt);
						this.lblLastSuccessfulRun.Text = PlanCommon.Format(CurrentOperation.LastSuccessfulRunAt);
						//this.lblTitle.Text = PlanCommon.FormatTitle(plan.Name);
						//this.lblSchedule.Text = plan.ScheduleType.ToString();

						CurrentOperation.GotInitialInfo = true;
						CurrentOperation.StartTimer();
						break;
					}
				case Commands.OperationStatus.SCANNING_FILES_STARTED:
					{
						this.lblSources.Text = "Scanning files...";
						break;
					}
				case Commands.OperationStatus.SCANNING_FILES_FINISHED:
					{
						break;
					}
				case Commands.OperationStatus.PROCESSING_FILES_STARTED:
					{
						this.lblSources.Text = "Scanning files...";
						break;
					}
				case Commands.OperationStatus.PROCESSING_FILES_FINISHED:
					{
						this.lblSources.Text = report.Sources;
						//this.lblFilesTransferred.Text = string.Format("{0} of {1} ({2} / {3})",
						//	progress.Completed, progress.Total,
						//	FileSizeUtils.FileSizeToString(progress.BytesCompleted),
						//	FileSizeUtils.FileSizeToString(progress.BytesTotal));
						break;
					}
				case Commands.OperationStatus.UPDATED:
					{
						// Should be handled by another command.
						break;
					}
				case Commands.OperationStatus.FINISHED:
					{
						CurrentOperation.FinishedAt = report.LastRunAt;
						CurrentOperation.LastRunAt = report.LastRunAt;
						CurrentOperation.LastSuccessfulRunAt = report.LastSuccessfulRunAt;
						UpdateDuration(report.Status);

						this.lblSources.Text = report.Sources;
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.llblRunNow.Enabled = true;
						this.lblStatus.Text = LBL_STATUS_COMPLETED;
						//this.lblDuration.Text = LBL_DURATION_INITIAL;
						//this.lblFilesTransferred.Text = LBL_FILES_TRANSFER_STOPPED;
						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;
						this.llblRestore.Enabled = true;
						this.lblLastRun.Text = PlanCommon.Format(CurrentOperation.LastRunAt);
						this.lblLastSuccessfulRun.Text = PlanCommon.Format(CurrentOperation.LastSuccessfulRunAt);
						//this.lblTitle.Text = PlanCommon.FormatTitle(plan.Name);
						//this.lblSchedule.Text = plan.ScheduleType.ToString();

						CurrentOperation.Reset();
						break;
					}
				case Commands.OperationStatus.FAILED:
				case Commands.OperationStatus.CANCELED:
					{
						CurrentOperation.FinishedAt = report.LastRunAt;
						CurrentOperation.LastRunAt = report.LastRunAt;
						UpdateDuration(report.Status);

						this.lblSources.Text = report.Sources;
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.llblRunNow.Enabled = true;
						this.lblStatus.Text = report.Status == Commands.OperationStatus.CANCELED ? LBL_STATUS_CANCELED : LBL_STATUS_FAILED;
						//this.lblDuration.Text = LBL_DURATION_INITIAL;
						//this.lblFilesTransferred.Text = LBL_FILES_TRANSFER_STOPPED;
						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;
						this.llblRestore.Enabled = true;
						this.lblLastRun.Text = PlanCommon.Format(CurrentOperation.LastRunAt);
						//this.lblLastSuccessfulRun.Text = PlanCommon.Format(CurrentOperation.LastSuccessfulRunAt);
						//this.lblTitle.Text = PlanCommon.FormatTitle(plan.Name);
						//this.lblSchedule.Text = plan.ScheduleType.ToString();

						CurrentOperation.Reset();
						break;
					}
			}
		}

		private void UpdatePlanProgress(Commands.GuiReportPlanProgress progress)
		{
			this.lblFilesTransferred.Text = string.Format("{0} of {1} ({2} / {3})",
				progress.Completed, progress.Total,
				FileSizeUtils.FileSizeToString(progress.BytesCompleted),
				FileSizeUtils.FileSizeToString(progress.BytesTotal));
		}

		private void llblEditPlan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Models.BackupPlan plan = this.Model as Models.BackupPlan;

			using (var presenter = new NewBackupPlanPresenter(plan))
			{
				presenter.ShowDialog(this.ParentForm);
			}

			// Reload after edit is complete.
			//plan.InvalidateCachedSelectedSourcesAsDelimitedString();
			//UpdateStatusInfo(Commands.OperationStatus.QUERY);
			Provider.Handler.Send(Commands.ServerQueryPlan("backup", plan.Id.Value));
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
			Models.BackupPlan plan = this.Model as Models.BackupPlan;

			if (OperationIsRunning)
			{
				// These buttons are re-enabled after the GUI receives the command `Commands.GUI_REPORT_PLAN_STATUS`
				// where the report status is one of these:
				//   Commands.OperationStatus.FAILED:
				//   Commands.OperationStatus.CANCELED:
				//   Commands.OperationStatus.FINISHED:
				this.llblRunNow.Enabled = false;
				this.llblEditPlan.Enabled = false;
				this.llblDeletePlan.Enabled = false;
				this.llblRestore.Enabled = false;

				Provider.Handler.Send(Commands.ServerCancelPlan("backup", plan.Id.Value));
			}
			else
			{
				// These buttons are re-enabled after the GUI receives the command `Commands.GUI_REPORT_PLAN_STATUS`
				// where the report status is one of these:
				//   Commands.OperationStatus.FAILED:
				//   Commands.OperationStatus.CANCELED:
				//   Commands.OperationStatus.FINISHED:
				this.llblRunNow.Enabled = false;
				this.llblEditPlan.Enabled = false;
				this.llblDeletePlan.Enabled = false;
				this.llblRestore.Enabled = false;

				string cmd = CurrentOperation.Status == Commands.OperationStatus.INTERRUPTED
					? Commands.ServerResumePlan("backup", plan.Id.Value)
					: Commands.ServerRunPlan("backup", plan.Id.Value);
				Provider.Handler.Send(cmd);
			}
		}

		public delegate void DeleteEventHandler(object sender, EventArgs e);
		public event DeleteEventHandler Deleted;

		protected virtual void OnDelete(object sender, EventArgs e)
		{
			if (Deleted != null)
				Deleted(this, e);
		}

		private void DurationTimer_Tick(object sender, EventArgs e)
		{
			UpdateDuration(OperationIsRunning ? Commands.OperationStatus.UPDATED : Commands.OperationStatus.FINISHED);
		}

		private void UpdateDuration(Commands.OperationStatus status)
		{
			// Prevent a NPE when accessing `CurrentOperation.StartedAt` below.
			if (!CurrentOperation.GotInitialInfo)
				return;

			// Calculate duration.
			var duration = !status.IsEnded()
				? DateTime.UtcNow - CurrentOperation.StartedAt.Value
				: CurrentOperation.FinishedAt.Value - CurrentOperation.StartedAt.Value;

			lblDuration.Text = TimeSpanUtils.GetReadableTimespan(duration);
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
				DetachEventHandlers();
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
