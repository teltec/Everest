using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.Ipc.Protocol;
using Teltec.Backup.Ipc.TcpSocket;
using Teltec.Backup.PlanExecutor.Backup;
using Teltec.Backup.PlanExecutor.Restore;
using Teltec.Common.Threading;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor
{
	// Documentation at https://github.com/gsscoder/commandline
	class Options
	{
		[CommandLine.Option("svc-host", DefaultValue = "127.0.0.1", HelpText = "The host where the IPC server is running.")]
		public string ServiceHost { get; set; }

		[CommandLine.Option("svc-port", DefaultValue = 8000, HelpText = "The port where the IPC server is running.")]
		public int ServicePort { get; set; }

		[CommandLine.Option("client-name", Required = true, HelpText = "The client name to register on the IPC server.")]
		public string ClientName { get; set; }

		[CommandLine.Option('t', "type", Required = true, HelpText = "The type of the plan to be executed [backup|restore].")]
		public string PlanType { get; set; }

		[CommandLine.Option('p', "plan", Required = true, HelpText = "The ID of the plan to be executed.")]
		public Int32 PlanIdentifier { get; set; }

		[CommandLine.Option("resume", DefaultValue = false, HelpText = "Whether to continue a previous interrupted operation.")]
		public bool Resume { get; set; }

		[CommandLine.Option('v', "verbose", DefaultValue = true, HelpText = "Print all messages to standard output.")]
		public bool Verbose { get; set; }

		[CommandLine.HelpOption]
		public string GetUsage()
		{
			return CommandLine.Text.HelpText.AutoBuild(this,
				(CommandLine.Text.HelpText current) => CommandLine.Text.HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}

	public enum OperationType
	{
		Backup = 0,
		Restore = 1,
	}

	public enum PlanTypeEnum
	{
		Backup = 0,
		Restore = 1,
	}

	class PlanExecutor : IDisposable
	{
		static readonly Logger logger = LogManager.GetCurrentClassLogger();

		static readonly Options options = new Options();

		private ISynchronizeInvoke SynchronizingObject = new MockSynchronizeInvoke();
		private ExecutorHandler Handler;

		//static OperationProgressReporter Reporter = new OperationProgressReporter(50052);

		#region Main

		static void Main(string[] args)
		{
			try
			{
				UnsafeMain(args);
			}
			catch (Exception ex)
			{
				if (Environment.UserInteractive)
				{
					//string message = string.Format(
					//	"Caught a fatal exception ({0}). Check the log file for more details.",
					//	ex.Message);
					//if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
					//	MessageBox.Show(message);
				}
				logger.Log(LogLevel.Fatal, ex, "Caught a fatal exception");
			}
		}

		static void UnsafeMain(string[] args)
		{
			if (!CommandLine.Parser.Default.ParseArguments(args, options))
				return;

			try
			{
				LoadSettings();
				var executor = new PlanExecutor();
				executor.Run();
				LetMeDebugThisBeforeExiting();
			}
			catch (Exception ex)
			{
				if (options.Verbose)
					logger.Log(LogLevel.Fatal, ex, "Oops! An unexpected problem happened");
				else
					logger.Fatal("Oops! An unexpected problem happened: {0}", ex.Message);

				LetMeDebugThisBeforeExiting();
				Environment.Exit(1);
			}
		}

		#endregion

		static void LetMeDebugThisBeforeExiting()
		{
			if (Debugger.IsAttached)
			{
				//Debugger.Break();
				Console.WriteLine("\nPress ENTER to exit.\n");
				Console.ReadLine();
			}
		}

		public PlanExecutor()
		{
			Handler = new ExecutorHandler(SynchronizingObject, options.ClientName, options.ServiceHost, options.ServicePort);
			Handler.OnControlPlanCancel = OnControlPlanCancel;
		}

		private void OnControlPlanCancel(object sender, ExecutorCommandEventArgs e)
		{
			if (RunningOperation == null || !RunningOperation.IsRunning)
			{
				Handler.Send(Commands.ReportError("Can't cancel. Operation is not running"));
				return;
			}

			RunningOperation.Cancel();
		}

		readonly BackupPlanRepository _daoBackupPlan = new BackupPlanRepository();
		readonly RestorePlanRepository _daoRestorePlan = new RestorePlanRepository();
		object Model = null;

		BaseOperation<TransferResults> RunningOperation = null;
		TransferResults TransferResults = null;
		bool MustResumeLastOperation = false;

		/// <summary>
		///  Event set when the process is terminated.
		/// </summary>
		readonly ManualResetEvent RunningOperationEndedEvent = new ManualResetEvent(false);

		private void Run()
		{
			// Validate plan type
			PlanTypeEnum selectedPlanType;
			bool validPlanType = Enum.TryParse<PlanTypeEnum>(options.PlanType, true, out selectedPlanType);
			if (!validPlanType)
				ExitShowingHelpText(1);

			Models.ISchedulablePlan plan = null;

			switch (selectedPlanType)
			{
				case PlanTypeEnum.Backup:
					{
						plan = _daoBackupPlan.Get(options.PlanIdentifier);
						break;
					}
				case PlanTypeEnum.Restore:
					{
						plan = _daoRestorePlan.Get(options.PlanIdentifier);
						break;
					}
			}

			if (plan == null)
			{
				logger.Fatal("The {0} plan with id {1} does not exist.",
					options.PlanType.ToString().ToLowerInvariant(), options.PlanIdentifier);
				ExitShowingHelpText(1);
			}

			Model = plan;

			if (options.Verbose)
			{
				logger.Info("Running {0} #{1}", options.PlanType, options.PlanIdentifier);
			}

			bool ok = RunOperation(selectedPlanType, plan);
			if (!ok)
			{
				Console.Error.WriteLine("Operation did not run.");
				Environment.Exit(1);
			}

			while (!RunningOperationEndedEvent.WaitOne(250))
			{
				RunningOperation.DoEvents();
			}

			Console.WriteLine("Operation finished.");
		}

		private bool RunOperation(PlanTypeEnum planType, object plan)
		{
			bool result = false;
			switch (planType)
			{
				case PlanTypeEnum.Backup:
					{
						result = RunBackupOperation(plan as Models.BackupPlan);
						break;
					}
				case PlanTypeEnum.Restore:
					{
						result = RunRestoreOperation(plan as Models.RestorePlan);
						break;
					}
			}
			return result;
		}

		private void ExitShowingHelpText(int exitCode)
		{
			CommandLine.Parser.Default.Settings.HelpWriter.Write(options.GetUsage());

			LetMeDebugThisBeforeExiting();
			Environment.Exit(exitCode);
		}

		private bool RunBackupOperation(Models.BackupPlan plan)
		{
			var dao = new BackupRepository();

			Models.Backup latest = dao.GetLatestByPlan(plan);
			MustResumeLastOperation = latest != null && latest.NeedsResume();

			if (MustResumeLastOperation && !options.Resume)
			{
				logger.Warn(
					"The backup (#{0}) has not finished yet."
					+ " If it's still running, please, wait until it finishes,"
					+ " otherwise you should resume it manually.",
					latest.Id);
				return false;
			}

			// Create new backup or resume the last unfinished one.
			BackupOperation obj = MustResumeLastOperation
				? new ResumeBackupOperation(latest) as BackupOperation
				: new NewBackupOperation(plan) as BackupOperation;

			obj.Updated += (sender2, e2) => BackupUpdateStatsInfo(e2.Status);
			//obj.EventLog = ...
			//obj.TransferListControl = ...

			BackupUpdateStatsInfo(BackupOperationStatus.Unknown);

			RunningOperation = obj;

			//
			// Actually RUN it
			//
			RunningOperation.Start(out TransferResults);
			return true;
		}

		private bool RunRestoreOperation(Models.RestorePlan plan)
		{
			var dao = new RestoreRepository();

			Models.Restore latest = dao.GetLatestByPlan(plan);
			MustResumeLastOperation = latest != null && latest.NeedsResume();

			if (MustResumeLastOperation && options.Resume)
			{
				throw new NotImplementedException("The restore operation still does not support resuming.");
			}

			if (MustResumeLastOperation && !options.Resume)
			{
				logger.Warn(
					"The restore (#{0}) has not finished yet."
					+ " If it's still running, please, wait until it finishes,"
					+ " otherwise you should resume it manually.",
					latest.Id);
				return false;
			}

			// Create new restore or resume the last unfinished one.
			RestoreOperation obj = /* MustResumeLastOperation
				? new ResumeRestoreOperation(latest) as RestoreOperation
				: */ new NewRestoreOperation(plan) as RestoreOperation;

			obj.Updated += (sender2, e2) => RestoreUpdateStatsInfo(e2.Status);
			//obj.EventLog = ...
			//obj.TransferListControl = ...

			RestoreUpdateStatsInfo(RestoreOperationStatus.Unknown);

			RunningOperation = obj;

			//
			// Actually RUN it
			//
			RunningOperation.Start(out TransferResults);
			return true;
		}

		private void BackupUpdateStatsInfo(BackupOperationStatus status)
		{
			if (RunningOperation == null)
				return;

			Models.BackupPlan plan = Model as Models.BackupPlan;

			//BackupUpdateMsg message = new BackupUpdateMsg
			//{
			//	PlanId = plan.Id.Value,
			//	OperationId = RunningOperation.OperationId.Value,
			//	OperationStatus = (byte)status,
			//};

			switch (status)
			{
				default: throw new ArgumentException("Unhandled status", "status");
				case BackupOperationStatus.Unknown:
					{
						/*
						this.lblSources.Text = RunningBackup.Sources;
						this.lblStatus.Text = MustResumeLastBackup ? LBL_STATUS_INTERRUPTED : LBL_STATUS_STOPPED;
						this.llblRunNow.Text = MustResumeLastBackup ? LBL_RUNNOW_RESUME : LBL_RUNNOW_STOPPED;
						this.lblFilesTransferred.Text = LBL_FILES_TRANSFER_STOPPED;
						this.lblDuration.Text = LBL_DURATION_INITIAL;
						*/
						break;
					}
				case BackupOperationStatus.Started:
				case BackupOperationStatus.Resumed:
					{
						logger.Info("{0} backup", status == BackupOperationStatus.Resumed ? "Resuming" : "Starting");

						//message.StartedAt = (RunningOperation as BackupOperation).StartedAt.Value;
						//message.IsResuming = status == BackupOperationStatus.Resumed;

						/*
						Assert.IsNotNull(BackupResults);
						this.lblSources.Text = RunningBackup.Sources;
						this.llblRunNow.Text = LBL_RUNNOW_RUNNING;
						this.lblStatus.Text = LBL_STATUS_STARTED;
						this.lblDuration.Text = LBL_DURATION_STARTED;
						this.lblFilesTransferred.Text = string.Format("{0} of {1}",
							BackupResults.Stats.Completed, BackupResults.Stats.Total);

						this.llblEditPlan.Enabled = false;
						this.llblDeletePlan.Enabled = false;
						this.llblRestore.Enabled = false;

						timer1.Enabled = true;
						timer1.Start();
						*/

						// Report
						Commands.OperationStatus cmdState = status == BackupOperationStatus.Started
							? Commands.OperationStatus.STARTED : Commands.OperationStatus.RESUMED;
						Handler.Send(Commands.ReportOperationStatus("backup", plan.Id.Value, cmdState));
						break;
					}
				case BackupOperationStatus.ScanningFilesStarted:
					{
						logger.Info("Scanning files...");

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.SCANNING_FILES_STARTED;
						Handler.Send(Commands.ReportOperationStatus("backup", plan.Id.Value, cmdState));
						break;
					}
				case BackupOperationStatus.ScanningFilesFinished:
					{
						logger.Info("Scanning files finished.");

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.SCANNING_FILES_FINISHED;
						Handler.Send(Commands.ReportOperationStatus("backup", plan.Id.Value, cmdState));
						break;
					}
				case BackupOperationStatus.ProcessingFilesStarted:
					{
						logger.Info("Processing files...");

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.PROCESSING_FILES_STARTED;
						Handler.Send(Commands.ReportOperationStatus("backup", plan.Id.Value, cmdState));
						break;
					}
				case BackupOperationStatus.ProcessingFilesFinished:
					{
						logger.Info("Processing files finished.");
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.PROCESSING_FILES_FINISHED;
						Handler.Send(Commands.ReportOperationStatus("backup", plan.Id.Value, cmdState));
						break;
					}
				case BackupOperationStatus.Finished:
					{
						/*
						UpdateDuration(status);
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = LBL_STATUS_COMPLETED;

						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;
						this.llblRestore.Enabled = true;

						timer1.Stop();
						timer1.Enabled = false;
						*/

						logger.Info("Backup finished.");

						// Update timestamps.
						plan.LastRunAt = plan.LastSuccessfulRunAt = DateTime.UtcNow;
						_daoBackupPlan.Update(plan);

						//message.FinishedAt = plan.LastRunAt.Value;

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.FINISHED;
						Handler.Send(Commands.ReportOperationStatus("backup", plan.Id.Value, cmdState));
						break;
					}
				case BackupOperationStatus.Updated:
					{
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);

						//message.TransferResults = TransferResultsMsgPart.CopyFrom(TransferResults);

						/*
						this.lblFilesTransferred.Text = string.Format("{0} of {1}",
							BackupResults.Stats.Completed, BackupResults.Stats.Total);
						*/

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.UPDATED;
						Handler.Send(Commands.ReportOperationStatus("backup", plan.Id.Value, cmdState));
						break;
					}
				case BackupOperationStatus.Failed:
				case BackupOperationStatus.Canceled:
					{
						/*
						UpdateDuration(status);

						this.lblSources.Text = RunningBackup.Sources;
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = status == BackupOperationStatus.Canceled ? LBL_STATUS_CANCELED : LBL_STATUS_FAILED;

						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;
						this.llblRestore.Enabled = true;

						timer1.Stop();
						timer1.Enabled = false;
						*/

						logger.Info("Backup {0}.", status == BackupOperationStatus.Failed ? "failed" : "was canceled");
						logger.Info("{0}: {1} of {2}", status == BackupOperationStatus.Failed ? "Failed" : "Canceled",
							TransferResults.Stats.Completed, TransferResults.Stats.Total);

						// Update timestamps.
						plan.LastRunAt = DateTime.UtcNow;
						_daoBackupPlan.Update(plan);

						//message.FinishedAt = plan.LastRunAt.Value;

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();

						// Report
						Commands.OperationStatus cmdState = status == BackupOperationStatus.Failed
							? Commands.OperationStatus.FAILED : Commands.OperationStatus.CANCELED;
						Handler.Send(Commands.ReportOperationStatus("backup", plan.Id.Value, cmdState));
						break;
					}
			}

			//Reporter.Publish(message);
		}

		private void RestoreUpdateStatsInfo(RestoreOperationStatus status)
		{
			if (RunningOperation == null)
				return;

			Models.RestorePlan plan = Model as Models.RestorePlan;

			//RestoreUpdateMsg message = new RestoreUpdateMsg
			//{
			//	PlanId = plan.Id.Value,
			//	OperationId = RunningOperation.OperationId.Value,
			//	OperationStatus = (byte)status,
			//};

			switch (status)
			{
				default: throw new ArgumentException("Unhandled status", "status");
				case RestoreOperationStatus.Unknown:
					{
						/*
						this.lblSources.Text = RunningRestore.Sources;
						this.lblStatus.Text = MustResumeLastRestore ? LBL_STATUS_INTERRUPTED : LBL_STATUS_STOPPED;
						this.llblRunNow.Text = MustResumeLastRestore ? LBL_RUNNOW_RESUME : LBL_RUNNOW_STOPPED;
						this.lblFilesTransferred.Text = LBL_FILES_TRANSFER_STOPPED;
						this.lblDuration.Text = LBL_DURATION_INITIAL;
						 */
						break;
					}
				case RestoreOperationStatus.Started:
				case RestoreOperationStatus.Resumed:
					{
						logger.Info("{0} restore", status == RestoreOperationStatus.Resumed ? "Resuming" : "Starting");

						//message.StartedAt = (RunningOperation as RestoreOperation).StartedAt.Value;
						//message.IsResuming = status == RestoreOperationStatus.Resumed;

						/*
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
						*/

						// Report
						Commands.OperationStatus cmdState = status == RestoreOperationStatus.Started
							? Commands.OperationStatus.STARTED : Commands.OperationStatus.RESUMED;
						Handler.Send(Commands.ReportOperationStatus("restore", plan.Id.Value, cmdState));
						break;
					}
				case RestoreOperationStatus.ScanningFilesStarted:
					{
						logger.Info("Scanning files...");

						/*
						this.lblSources.Text = "Scanning files...";
						*/

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.SCANNING_FILES_STARTED;
						Handler.Send(Commands.ReportOperationStatus("restore", plan.Id.Value, cmdState));
						break;
					}
				case RestoreOperationStatus.ScanningFilesFinished:
					{
						logger.Info("Scanning files finished.");

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.SCANNING_FILES_FINISHED;
						Handler.Send(Commands.ReportOperationStatus("restore", plan.Id.Value, cmdState));
						break;
					}
				case RestoreOperationStatus.ProcessingFilesStarted:
					{
						logger.Info("Processing files...");

						/*
						this.lblSources.Text = "Processing files...";
						*/

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.PROCESSING_FILES_STARTED;
						Handler.Send(Commands.ReportOperationStatus("restore", plan.Id.Value, cmdState));
						break;
					}
				case RestoreOperationStatus.ProcessingFilesFinished:
					{
						logger.Info("Processing files finished.");
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);

						/*
						this.lblSources.Text = RunningRestore.Sources;
						this.lblFilesTransferred.Text = string.Format("{0} of {1}",
							RestoreResults.Stats.Completed, RestoreResults.Stats.Total);
						*/

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.PROCESSING_FILES_FINISHED;
						Handler.Send(Commands.ReportOperationStatus("restore", plan.Id.Value, cmdState));
						break;
					}
				case RestoreOperationStatus.Finished:
					{
						/*
						UpdateDuration(status);
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = LBL_STATUS_COMPLETED;

						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;

						timer1.Stop();
						timer1.Enabled = false;
						*/

						logger.Info("Restore finished.");

						// Update timestamps.
						plan.LastRunAt = plan.LastSuccessfulRunAt = DateTime.UtcNow;
						_daoRestorePlan.Update(plan);

						//message.FinishedAt = plan.LastRunAt.Value;

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.FINISHED;
						Handler.Send(Commands.ReportOperationStatus("restore", plan.Id.Value, cmdState));
						break;
					}
				case RestoreOperationStatus.Updated:
					{
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);

						//message.TransferResults = TransferResultsMsgPart.CopyFrom(TransferResults);

						/*
						this.lblFilesTransferred.Text = string.Format("{0} of {1}",
							RestoreResults.Stats.Completed, RestoreResults.Stats.Total);
						*/

						// Report
						Commands.OperationStatus cmdState = Commands.OperationStatus.UPDATED;
						Handler.Send(Commands.ReportOperationStatus("restore", plan.Id.Value, cmdState));
						break;
					}
				case RestoreOperationStatus.Failed:
				case RestoreOperationStatus.Canceled:
					{
						/*
						UpdateDuration(status);

						this.lblSources.Text = RunningRestore.Sources;
						this.llblRunNow.Text = LBL_RUNNOW_STOPPED;
						this.lblStatus.Text = status == RestoreOperationStatus.Canceled ? LBL_STATUS_CANCELED : LBL_STATUS_FAILED;

						this.llblEditPlan.Enabled = true;
						this.llblDeletePlan.Enabled = true;

						timer1.Stop();
						timer1.Enabled = false;
						*/

						logger.Info("Restore {0}.", status == RestoreOperationStatus.Failed ? "failed" : "was canceled");
						logger.Info("{0}: {1} of {2}", status == RestoreOperationStatus.Failed ? "Failed" : "Canceled",
							TransferResults.Stats.Completed, TransferResults.Stats.Total);

						// Update timestamps.
						plan.LastRunAt = DateTime.UtcNow;
						_daoRestorePlan.Update(plan);

						//message.FinishedAt = plan.LastRunAt.Value;

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();

						// Report
						Commands.OperationStatus cmdState = status == RestoreOperationStatus.Failed
							? Commands.OperationStatus.FAILED : Commands.OperationStatus.CANCELED;
						Handler.Send(Commands.ReportOperationStatus("restore", plan.Id.Value, cmdState));
						break;
					}
			}

			//Reporter.Publish(message);
		}

		private static void LoadSettings()
		{
			AsyncHelper.SettingsMaxThreadCount = Teltec.Backup.Settings.Properties.Current.MaxThreadCount;
		}

		#region Dispose Pattern Implementation

		bool _shouldDispose = false;
		bool _isDisposed;

		/// <summary>
		/// Implements the Dispose pattern
		/// </summary>
		/// <param name="disposing">Whether this object is being disposed via a call to Dispose
		/// or garbage collected.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					/*
					if (Reporter != null)
					{
						Reporter.Dispose();
						Reporter = null;
					}
					*/
				}
				this._isDisposed = true;
			}
		}

		/// <summary>
		/// Disposes of all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
