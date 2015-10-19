using NUnit.Framework;
using System;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.PlanExecutor.Synchronize;
using Teltec.Common.Utils;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Forms.Sync
{
	public partial class SyncProgressForm : Teltec.Forms.Wizard.WizardForm
	{
		private Models.Synchronization Sync = new Models.Synchronization();
		private EventDispatcher Dispatcher = new EventDispatcher();

		public SyncProgressForm()
		{
			InitializeComponent();

			this.ModelChangedEvent += (sender, args) =>
			{
				this.Sync = args.Model as Models.Synchronization;

				switch (this.Sync.StorageAccountType)
				{
					case Models.EStorageAccountType.AmazonS3:
						break;
					case Models.EStorageAccountType.FileSystem:
						break;
				}

				NewSyncOperation(this.Model as Models.Synchronization);
			};
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			this.btnPrevious.Enabled = false;
			this.btnNext.Enabled = false;
			this.btnFinish.Enabled = false;
			this.btnCancel.Enabled = false;
		}

		// ----------------------------------------------------------------------------------------

		private readonly SynchronizationRepository _daoSynchronization = new SynchronizationRepository();
		//private OperationProgressWatcher Watcher = new OperationProgressWatcher(50052);

		SyncOperation RunningOperation = null;
		SyncResults OperationResults = null;

		public bool IsRunning
		{
			get { return RunningOperation != null ? RunningOperation.IsRunning : false; }
		}

		private void NewSyncOperation(Models.Synchronization sync)
		{
			// Create new sync operation.
			SyncOperation obj = new NewSyncOperation(sync) as SyncOperation;

			obj.Updated += (sender2, e2) => UpdateStatsInfo(e2.Status);
			//obj.EventLog = ...
			//obj.TransferListControl = ...

			// IMPORTANT: Dispose before assigning.
			if (RunningOperation != null)
				RunningOperation.Dispose();

			RunningOperation = obj;

			UpdateStatsInfo(SyncOperationStatus.Unknown);
		}

		private void llblRunNow_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
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

				this.btnPrevious.Enabled = false;
				this.btnNext.Enabled = false;
				this.btnFinish.Enabled = false;

				Models.Synchronization sync = Model as Models.Synchronization;
				if (sync.Id.HasValue)
				{
					_daoSynchronization.Update(sync);
				}
				else
				{
					_daoSynchronization.Insert(sync);
				}

				// Create new sync operation for every 'Run' click.
				NewSyncOperation(this.Model as Models.Synchronization);

				// FIXME: Re-enable before starting the backup because it's not an async task.
				this.llblRunNow.Enabled = true;

				RunningOperation.Start(out OperationResults);
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			UpdateDuration(RunningOperation.IsRunning ? SyncOperationStatus.ListingUpdated : SyncOperationStatus.Finished);
		}

		static string LBL_RUNNOW_RUNNING = "Cancel";
		static string LBL_RUNNOW_STOPPED = "Run now";
		static string LBL_STATUS_STARTED = "Running";
		static string LBL_STATUS_STOPPED = "Stopped";
		static string LBL_STATUS_CANCELED = "Canceled";
		static string LBL_STATUS_FAILED = "Failed";
		static string LBL_STATUS_COMPLETED = "Completed";
		static string LBL_DURATION_STARTED = "Starting...";
		static string LBL_DURATION_INITIAL = "Not started";
		static string LBL_TOTALFILES_STOPPED = "Not started";
		static string LBL_TOTALFILES_STARTED = "Starting...";
		static string LBL_FILESSYNCED_STOPPED = "Not started";
		static string LBL_FILESSYNCED_STARTED = "Waiting listing completion...";

		private void UpdateStatsInfo(SyncOperationStatus status, bool runningRemotely = false)
		{
			if (RunningOperation == null)
				return;

			switch (status)
			{
				default: throw new ArgumentException("Unhandled status", "status");
				case SyncOperationStatus.Unknown:
					{
						this.lblRemoteDirectory.Text = RunningOperation.RemoteRootDirectory;
						this.lblStatus.Text = LBL_STATUS_STOPPED;
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblTotalFiles.Text = LBL_TOTALFILES_STOPPED;
						this.lblFilesSynced.Text = LBL_FILESSYNCED_STOPPED;
						this.lblDuration.Text = LBL_DURATION_INITIAL;
						this.btnPrevious.Enabled = false;
						this.btnNext.Enabled = false;
						this.btnFinish.Enabled = false;
						break;
					}
				case SyncOperationStatus.Started:
				//case SyncOperationStatus.Resumed:
					{
						Assert.IsNotNull(OperationResults);
						this.lblRemoteDirectory.Text = RunningOperation.RemoteRootDirectory;
						this.llblRunNow.Text = LBL_RUNNOW_RUNNING;
						this.lblStatus.Text = LBL_STATUS_STARTED;
						this.lblDuration.Text = LBL_DURATION_STARTED;
						this.lblTotalFiles.Text = LBL_TOTALFILES_STARTED;
						this.lblFilesSynced.Text = LBL_FILESSYNCED_STARTED;

						timer1.Enabled = true;
						timer1.Start();
						break;
					}
				case SyncOperationStatus.ListingUpdated:
					{
						long totalSize = OperationResults.Stats.TotalSize;
						string totalSizeAsStr = totalSize == 0 ? "Completed" : FileSizeUtils.FileSizeToString(totalSize);

						this.lblTotalFiles.Text = string.Format("{0} files ({1})",
							 OperationResults.Stats.FileCount, totalSizeAsStr);
						break;
					}
				case SyncOperationStatus.SavingUpdated:
					{
						Dispatcher.Invoke(() =>
						{
							this.lblFilesSynced.Text = string.Format("{0} of {1}",
								OperationResults.Stats.SavedFileCount,
								OperationResults.Stats.FileCount);
						});
						break;
					}
				case SyncOperationStatus.Canceled:
				case SyncOperationStatus.Failed:
					{
						UpdateDuration(status);

						this.lblRemoteDirectory.Text = RunningOperation.RemoteRootDirectory;
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = status == SyncOperationStatus.Canceled ? LBL_STATUS_CANCELED : LBL_STATUS_FAILED;

						timer1.Stop();
						timer1.Enabled = false;

						this.btnPrevious.Enabled = true;
						this.btnNext.Enabled = true;
						this.btnFinish.Enabled = true;

						if (!runningRemotely)
						{
							// Update timestamps.
							Models.Synchronization sync = Model as Models.Synchronization;
							//sync.LastRunAt = DateTime.UtcNow;
							_daoSynchronization.Update(sync);
						}
						break;
					}
				case SyncOperationStatus.Finished:
					{
						UpdateDuration(status);
						this.lblRemoteDirectory.Text = RunningOperation.RemoteRootDirectory;
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = LBL_STATUS_COMPLETED;

						timer1.Stop();
						timer1.Enabled = false;

						this.btnPrevious.Enabled = true;
						this.btnNext.Enabled = true;
						this.btnFinish.Enabled = true;

						if (!runningRemotely)
						{
							// Update timestamps.
							Models.Synchronization sync = Model as Models.Synchronization;
							//sync.LastRunAt = sync.LastSuccessfulRunAt = DateTime.UtcNow;
							_daoSynchronization.Update(sync);
						}
						break;
					}
			}
		}

		private void UpdateDuration(SyncOperationStatus status)
		{
			Assert.IsNotNull(RunningOperation);
			var duration = !status.IsEnded()
				? DateTime.UtcNow - RunningOperation.StartedAt.Value
				: RunningOperation.FinishedAt.Value - RunningOperation.StartedAt.Value;
			lblDuration.Text = TimeSpanUtils.GetReadableTimespan(duration);
		}

		#region Dispose Pattern Implementation

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
	}
}
