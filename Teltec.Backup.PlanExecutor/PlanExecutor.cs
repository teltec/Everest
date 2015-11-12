using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.Ipc.Protocol;
using Teltec.Backup.Ipc.TcpSocket;
using Teltec.Backup.Logging;
using Teltec.Backup.PlanExecutor.Backup;
using Teltec.Backup.PlanExecutor.Restore;
using Teltec.Common;
using Teltec.Common.Threading;
using Teltec.FileSystem;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor
{
	// Documentation at https://github.com/gsscoder/commandline
	class Options
	{
		[CommandLine.Option("svc-host", DefaultValue = Commands.IPC_DEFAULT_HOST, HelpText = "The host where the IPC server is running.")]
		public string ServiceHost { get; set; }

		[CommandLine.Option("svc-port", DefaultValue = Commands.IPC_DEFAULT_PORT, HelpText = "The port where the IPC server is running.")]
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

#if RELEASE
		static readonly bool IsReleaseVersion = true;
#else
		static readonly bool IsReleaseVersion = false;
#endif

		private ISynchronizeInvoke SynchronizingObject = new MockSynchronizeInvoke();
		private ExecutorHandler Handler;

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
			LoggingHelper.ChangeFilenamePostfix("executor");

			if (!IsReleaseVersion && System.Environment.UserInteractive)
				ConsoleAppHelper.CatchSpecialConsoleEvents();

			if (!CommandLine.Parser.Default.ParseArguments(args, options))
				return;

			PlanExecutor executor = null;

			try
			{
				LoadSettings();

				executor = new PlanExecutor();
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

			if (!IsReleaseVersion && System.Environment.UserInteractive)
			{
				// Set this to terminate immediately (if not set, the OS will eventually kill the process)
				ConsoleAppHelper.TerminationCompletedEvent.Set();
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
			Handler.OnControlPlanCancel += OnControlPlanCancel;
			Handler.OnError += OnErrorReceived;
		}

		private void OnErrorReceived(object sender, ExecutorCommandEventArgs e)
		{
			string message = e.Command.GetArgumentValue<string>("message");
			logger.Warn("ERROR RECEIVED: {0}", message);
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

		private DriveInfo GetDriveInfo(string drive)
		{
			DriveInfo[] drivesInUse = DriveInfo.GetDrives();
			foreach (DriveInfo d in drivesInUse)
			{
				if (d.RootDirectory.FullName.Equals(drive))
					return d;
			}
			return null;
		}

		private void MapAllNetworkDrives()
		{
			NetworkCredentialRepository dao = new NetworkCredentialRepository();
			List<Models.NetworkCredential> allCredentials = dao.GetAll();

			if (allCredentials.Count > 0)
				logger.Info("Mounting network shares...");

			foreach (Models.NetworkCredential cred in allCredentials)
			{
				try
				{
					DriveInfo drive = GetDriveInfo(cred.MountPoint);
					if (drive != null)
					{
						string remotePath = MappedDriveResolver.ResolveToRootUNC(cred.MountPoint);
						logger.Info("{0} is already mounted to {1}", cred.MountPoint, remotePath);
						return;
					}

					NetworkDriveMapper.MountNetworkLocation(cred.MountPoint, cred.Path, cred.Login, cred.Password, false);
					logger.Info("Successfully mounted {0} to {1}", cred.MountPoint, cred.Path);
				}
				catch (Win32Exception ex)
				{
					string reason = ex.Message;

					switch (ex.NativeErrorCode)
					{
						case NetworkDriveMapper.ERROR_ALREADY_ASSIGNED:
							string remotePath = MappedDriveResolver.ResolveToRootUNC(cred.MountPoint);
							reason = string.Format("It's already mounted to {0}", remotePath);
							break;
					}

					logger.Warn("Failed to mount {0} to {1} - {2}", cred.MountPoint, cred.Path, reason);
				}
			}
		}

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

			MapAllNetworkDrives();

			bool ok = RunOperation(selectedPlanType, plan);
			if (!ok)
			{
				Console.Error.WriteLine("Operation did not run.");
				Environment.Exit(1);
			}

			while (true)
			{
				if (!IsReleaseVersion && System.Environment.UserInteractive)
				{
					if (ConsoleAppHelper.TerminationRequestedEvent.WaitOne(1))
					{
						if (RunningOperation != null && RunningOperation.IsRunning)
							RunningOperation.Cancel();
						else
							break;
					}
				}

				if (RunningOperationEndedEvent.WaitOne(100))
					break;

				RunningOperation.DoEvents();
			}

			Console.WriteLine("Operation finished.");

			Handler.Client.WaitUntilDone();

#if DEBUG
			// Wait 10 seconds before exiting, just for debugging purposes.
			Thread.Sleep(10000);
#endif
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

		private Commands.GuiReportPlanStatus BuildGuiReportPlanStatus(Commands.OperationStatus status)
		{
			Models.BackupPlan plan = Model as Models.BackupPlan;
			BackupOperation op = RunningOperation as BackupOperation;
			Commands.GuiReportPlanStatus data = new Commands.GuiReportPlanStatus
			{
				Status = status,
				StartedAt = op.StartedAt,
				LastRunAt = plan.LastRunAt,
				LastSuccessfulRunAt = plan.LastSuccessfulRunAt,
				Sources = op.Sources,
			};

			// Sources
			if (status == Commands.OperationStatus.PROCESSING_FILES_FINISHED
				|| status == Commands.OperationStatus.FINISHED
				|| status == Commands.OperationStatus.FAILED
				|| status == Commands.OperationStatus.CANCELED)
			{
				data.Sources = op.Sources;
			}

			return data;
		}

		private Commands.GuiReportPlanProgress BuildGuiReportPlanProgress(Commands.OperationStatus status)
		{
			Models.BackupPlan plan = Model as Models.BackupPlan;
			BackupOperation op = RunningOperation as BackupOperation;
			Commands.GuiReportPlanProgress data = new Commands.GuiReportPlanProgress
			{
				Completed = TransferResults.Stats.Completed,
				Total = TransferResults.Stats.Total,
				BytesCompleted = TransferResults.Stats.BytesCompleted,
				BytesTotal = TransferResults.Stats.BytesTotal,
			};
			return data;
		}

		private void BackupUpdateStatsInfo(BackupOperationStatus status)
		{
			if (RunningOperation == null)
				return;

			Models.BackupPlan plan = Model as Models.BackupPlan;

			switch (status)
			{
				default: throw new ArgumentException("Unhandled status", "status");
				case BackupOperationStatus.Unknown:
					{
						break;
					}
				case BackupOperationStatus.Started:
				case BackupOperationStatus.Resumed:
					{
						logger.Info("{0} backup", status == BackupOperationStatus.Resumed ? "Resuming" : "Starting");

						// Update timestamps.
						plan.LastRunAt = DateTime.UtcNow;
						_daoBackupPlan.Update(plan);

						// Report
						Commands.OperationStatus cmdStatus = status == BackupOperationStatus.Started
								? Commands.OperationStatus.STARTED
								: Commands.OperationStatus.RESUMED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("backup", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case BackupOperationStatus.ScanningFilesStarted:
					{
						logger.Info("Scanning files...");

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.SCANNING_FILES_STARTED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("backup", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case BackupOperationStatus.ScanningFilesFinished:
					{
						logger.Info("Scanning files finished.");

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.SCANNING_FILES_FINISHED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("backup", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case BackupOperationStatus.ProcessingFilesStarted:
					{
						logger.Info("Processing files...");

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.PROCESSING_FILES_STARTED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("backup", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case BackupOperationStatus.ProcessingFilesFinished:
					{
						logger.Info("Processing files finished.");
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.PROCESSING_FILES_FINISHED;
						// Report sources
						Commands.GuiReportPlanStatus cmdData1 = BuildGuiReportPlanStatus(cmdStatus);
						string cmd1 = Commands.GuiReportOperationStatus("backup", plan.Id.Value, cmdData1);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd1);
						// Report counts
						Commands.GuiReportPlanProgress cmdData2 = BuildGuiReportPlanProgress(cmdStatus);
						string cmd2 = Commands.GuiReportOperationProgress("backup", plan.Id.Value, cmdData2);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd2);
						break;
					}
				case BackupOperationStatus.Finished:
					{
						//var message = string.Format(
						//	"Backup {0}! Stats: {1} completed, {2} failed, {3} canceled, {4} pending, {5} running",
						//	"finished",
						//	TransferResults.Stats.Completed, TransferResults.Stats.Failed,
						//	TransferResults.Stats.Canceled, TransferResults.Stats.Pending,
						//	TransferResults.Stats.Running);
						//logger.Info(message);

						// Update success timestamp.
						plan.LastSuccessfulRunAt = DateTime.UtcNow;
						_daoBackupPlan.Update(plan);

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.FINISHED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("backup", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case BackupOperationStatus.Updated:
					{
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.UPDATED;
						Commands.GuiReportPlanProgress cmdData = BuildGuiReportPlanProgress(cmdStatus);
						string cmd = Commands.GuiReportOperationProgress("backup", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case BackupOperationStatus.Failed:
				case BackupOperationStatus.Canceled:
					{
						//var message = string.Format(
						//	"Backup {0}! Stats: {1} completed, {2} failed, {3} canceled, {4} pending, {5} running",
						//	status == BackupOperationStatus.Failed ? "failed" : "was canceled",
						//	TransferResults.Stats.Completed, TransferResults.Stats.Failed,
						//	TransferResults.Stats.Canceled, TransferResults.Stats.Pending,
						//	TransferResults.Stats.Running);
						//logger.Info(message);

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();

						// Report
						Commands.OperationStatus cmdStatus = status == BackupOperationStatus.Failed
							? Commands.OperationStatus.FAILED : Commands.OperationStatus.CANCELED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("backup", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
			}
		}

		private void RestoreUpdateStatsInfo(RestoreOperationStatus status)
		{
			if (RunningOperation == null)
				return;

			Models.RestorePlan plan = Model as Models.RestorePlan;

			switch (status)
			{
				default: throw new ArgumentException("Unhandled status", "status");
				case RestoreOperationStatus.Unknown:
					{
						break;
					}
				case RestoreOperationStatus.Started:
				case RestoreOperationStatus.Resumed:
					{
						logger.Info("{0} restore", status == RestoreOperationStatus.Resumed ? "Resuming" : "Starting");

						// Update timestamps.
						plan.LastRunAt = DateTime.UtcNow;
						_daoRestorePlan.Update(plan);

						// Report
						Commands.OperationStatus cmdStatus = status == RestoreOperationStatus.Started
							? Commands.OperationStatus.STARTED : Commands.OperationStatus.RESUMED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("restore", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case RestoreOperationStatus.ScanningFilesStarted:
					{
						logger.Info("Scanning files...");

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.SCANNING_FILES_STARTED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("restore", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case RestoreOperationStatus.ScanningFilesFinished:
					{
						logger.Info("Scanning files finished.");

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.SCANNING_FILES_FINISHED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("restore", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case RestoreOperationStatus.ProcessingFilesStarted:
					{
						logger.Info("Processing files...");

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.PROCESSING_FILES_STARTED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						Handler.Send(Commands.GuiReportOperationStatus("restore", plan.Id.Value, cmdData));
						break;
					}
				case RestoreOperationStatus.ProcessingFilesFinished:
					{
						logger.Info("Processing files finished.");
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.PROCESSING_FILES_FINISHED;
						// Report sources
						Commands.GuiReportPlanStatus cmdData1 = BuildGuiReportPlanStatus(cmdStatus);
						string cmd1 = Commands.GuiReportOperationStatus("restore", plan.Id.Value, cmdData1);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd1);
						// Report counts
						Commands.GuiReportPlanProgress cmdData2 = BuildGuiReportPlanProgress(cmdStatus);
						string cmd2 = Commands.GuiReportOperationProgress("restore", plan.Id.Value, cmdData2);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd2);
						break;
					}
				case RestoreOperationStatus.Finished:
					{
						//var message = string.Format(
						//	"Restore {0}! Stats: {1} completed, {2} failed, {3} canceled, {4} pending, {5} running",
						//	"finished",
						//	TransferResults.Stats.Completed, TransferResults.Stats.Failed,
						//	TransferResults.Stats.Canceled, TransferResults.Stats.Pending,
						//	TransferResults.Stats.Running);
						//logger.Info(message);

						// Update success timestamp.
						plan.LastSuccessfulRunAt = DateTime.UtcNow;
						_daoRestorePlan.Update(plan);

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.FINISHED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("restore", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case RestoreOperationStatus.Updated:
					{
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);

						// Report
						Commands.OperationStatus cmdStatus = Commands.OperationStatus.UPDATED;
						Commands.GuiReportPlanProgress cmdData = BuildGuiReportPlanProgress(cmdStatus);
						string cmd = Commands.GuiReportOperationProgress("restore", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
				case RestoreOperationStatus.Failed:
				case RestoreOperationStatus.Canceled:
					{
						//var message = string.Format(
						//	"Restore {0}! Stats: {1} completed, {2} failed, {3} canceled, {4} pending, {5} running",
						//	status == RestoreOperationStatus.Failed ? "failed" : "was canceled",
						//	TransferResults.Stats.Completed, TransferResults.Stats.Failed,
						//	TransferResults.Stats.Canceled, TransferResults.Stats.Pending,
						//	TransferResults.Stats.Running);
						//logger.Info(message);

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();

						// Report
						Commands.OperationStatus cmdStatus = status == RestoreOperationStatus.Failed
							? Commands.OperationStatus.FAILED : Commands.OperationStatus.CANCELED;
						Commands.GuiReportPlanStatus cmdData = BuildGuiReportPlanStatus(cmdStatus);
						string cmd = Commands.GuiReportOperationStatus("restore", plan.Id.Value, cmdData);
						Handler.Route(Commands.IPC_DEFAULT_GUI_CLIENT_NAME, cmd);
						break;
					}
			}
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
					if (Handler != null)
					{
						Handler.Dispose();
						Handler = null;
					}
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
