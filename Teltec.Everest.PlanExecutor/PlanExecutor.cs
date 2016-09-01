using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Teltec.Common;
using Teltec.Common.Threading;
using Teltec.Everest.Data.DAO;
using Teltec.Everest.Ipc.Protocol;
using Teltec.Everest.Ipc.TcpSocket;
using Teltec.Everest.Logging;
using Teltec.Everest.PlanExecutor.Backup;
using Teltec.Everest.PlanExecutor.Restore;
using Teltec.Storage;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.PlanExecutor
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
		public PlanTypeEnum PlanType { get; set; }

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

		static readonly Options Options = new Options();

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

			string cwd = AppDomain.CurrentDomain.BaseDirectory;
			Directory.SetCurrentDirectory(cwd ?? ".");
			logger.Info("Current directory is {0}", cwd);

			// log4net - We are using NLog, so we should adapt the following.
			//XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));

			if (!IsReleaseVersion && System.Environment.UserInteractive)
				ConsoleAppHelper.CatchSpecialConsoleEvents();

			// Disable parser case-sensitivity to allow convertion from string to PlanType - Supported only in CommandLine > 2.0
			//CommandLine.Parser.Default.Settings.CaseSensitive = false;

			if (!CommandLine.Parser.Default.ParseArguments(args, Options))
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
				if (Options.Verbose)
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

		private void ExitShowingHelpText(int exitCode)
		{
			CommandLine.Parser.Default.Settings.HelpWriter.Write(Options.GetUsage());

			LetMeDebugThisBeforeExiting();
			Environment.Exit(exitCode);
		}

		public PlanExecutor()
		{
			Handler = new ExecutorHandler(SynchronizingObject, Options.ClientName, Options.ServiceHost, Options.ServicePort);
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
				Handler.Send(Commands.ReportError(0, "Can't cancel. Operation is not running"));
				return;
			}

			RunningOperation.Cancel();
		}

		readonly BackupPlanRepository _daoBackupPlan = new BackupPlanRepository();
		readonly RestorePlanRepository _daoRestorePlan = new RestorePlanRepository();
		object Model = null;

		IBaseOperation RunningOperation = null;
		bool MustResumeLastOperation = false;

		/// <summary>
		///  Event set when the process is terminated.
		/// </summary>
		readonly ManualResetEvent RunningOperationEndedEvent = new ManualResetEvent(false);

		private void ValidateOptions()
		{

		}

		private void Run()
		{
			// Load informed plan
			Models.ISchedulablePlan plan = LoadPlan(Options);
			if (plan == null)
			{
				logger.Fatal("The {0} plan with id {1} does not exist.",
					Options.PlanType.ToString().ToLowerInvariant(), Options.PlanIdentifier);
				ExitShowingHelpText(1);
			}

			Model = plan;

			if (Options.Verbose)
			{
				logger.Info("Running {0} plan #{1}", Options.PlanType.ToString().ToLowerInvariant(), Options.PlanIdentifier);
			}

#if !DEBUG
			try
#endif
			{
				RunningOperation = CreateOperation(plan, Options);

				RunningOperation.Start();

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

				logger.Info("Operation finished.");

				Handler.Client.WaitUntilDone();

				RunningOperation.SendReport();
			}
#if !DEBUG
			catch (Exception ex)
			{
				string message = ex.Message;

				Handler.Send(Commands.ReportError(0, message));
				logger.Error(message);

				if (RunningOperation != null)
				{
					BaseOperationReport report = RunningOperation.GetReport() as BaseOperationReport;
					report.OperationStatus = OperationStatus.FAILED;
					report.AddErrorMessage(message);
					RunningOperation.SendReport();
				}

				Environment.Exit(1);
			}
#endif

#if DEBUG
			// Wait 10 seconds before exiting, only for debugging purposes.
			Thread.Sleep(10000);
#endif
		}

		private Models.ISchedulablePlan LoadPlan(Options options)
		{
			Models.ISchedulablePlan result = null;

			switch (options.PlanType)
			{
				case PlanTypeEnum.Backup:
					result = _daoBackupPlan.Get(options.PlanIdentifier);
					break;
				case PlanTypeEnum.Restore:
					result = _daoRestorePlan.Get(options.PlanIdentifier);
					break;
			}

			return result;
		}

		private IBaseOperation CreateOperation(Models.ISchedulablePlan plan, Options options)
		{
			IBaseOperation result = null;

			switch (options.PlanType)
			{
				case PlanTypeEnum.Backup:
					result = CreateBackupOperation(plan as Models.BackupPlan);
					//result = CreateBackupOperation(plan as Models.BackupPlan);
					break;
				case PlanTypeEnum.Restore:
					result = CreateRestoreOperation(plan as Models.RestorePlan);
					//result = CreateRestoreOperation(plan as Models.RestorePlan);
					break;
			}

			return result;
		}

		/// <exception cref="System.InvalidOperationException">
		///   Thrown when the previous backup operation has not finished yet.
		/// </exception>
		private BackupOperation CreateBackupOperation(Models.BackupPlan plan)
		{
			var dao = new BackupRepository();

			Models.Backup latest = dao.GetLatestByPlan(plan);
			MustResumeLastOperation = latest != null && latest.NeedsResume();

			if (MustResumeLastOperation && !Options.Resume)
			{
				string message = string.Format("The backup (#{0}) has not finished yet."
					+ " If it's still running, please, wait until it finishes,"
					+ " otherwise you should resume it manually.",
					latest.Id);
				throw new InvalidOperationException(message);
			}

			// Create new backup or resume the last unfinished one.
			BackupOperation obj = MustResumeLastOperation
				? new ResumeBackupOperation(latest) as BackupOperation
				: new NewBackupOperation(plan) as BackupOperation;

			obj.Updated += (sender2, e2) => BackupUpdateStatsInfo(e2.Status, e2.TransferStatus);
			//obj.EventLog = ...
			//obj.TransferListControl = ...

			BackupUpdateStatsInfo(BackupOperationStatus.Unknown, TransferStatus.STOPPED);

			return obj;
		}

		/// <exception cref="System.InvalidOperationException">
		///   Thrown when the previous backup operation has not finished yet.
		/// </exception>
		/// <exception cref="System.NotImplementedException">
		///   Thrown when the previous backup operation has not finished yet and it's marked to be resumed (`options.Resume` is `true`).
		///   The restore operation doesn't support resume.
		/// </exception>
		private RestoreOperation CreateRestoreOperation(Models.RestorePlan plan)
		{
			var dao = new RestoreRepository();

			Models.Restore latest = dao.GetLatestByPlan(plan);
			MustResumeLastOperation = latest != null && latest.NeedsResume();

			if (MustResumeLastOperation && Options.Resume)
			{
				throw new NotImplementedException("The restore operation still does not support resuming.");
			}

			if (MustResumeLastOperation && !Options.Resume)
			{
				string message = string.Format("The restore (#{0}) has not finished yet."
					+ " If it's still running, please, wait until it finishes,"
					+ " otherwise you should resume it manually.",
					latest.Id);
				throw new InvalidOperationException(message);
			}

			// Create new restore or resume the last unfinished one.
			RestoreOperation obj = /* MustResumeLastOperation
				? new ResumeRestoreOperation(latest) as RestoreOperation
				: */ new NewRestoreOperation(plan) as RestoreOperation;

			obj.Updated += (sender2, e2) => RestoreUpdateStatsInfo(e2.Status, e2.TransferStatus);
			//obj.EventLog = ...
			//obj.TransferListControl = ...

			RestoreUpdateStatsInfo(RestoreOperationStatus.Unknown, TransferStatus.STOPPED);

			return obj;
		}

		private Commands.GuiReportPlanStatus BuildGuiReportPlanStatus(Commands.OperationStatus status)
		{
			Commands.GuiReportPlanStatus data = null;

			if (RunningOperation is BackupOperation)
			{
				BackupOperation op = RunningOperation as BackupOperation;
				Models.BackupPlan plan = Model as Models.BackupPlan;
				data = new Commands.GuiReportPlanStatus
				{
					Status = status,
					StartedAt = op.StartedAt,
					FinishedAt = op.FinishedAt,
					LastRunAt = plan.LastRunAt,
					LastSuccessfulRunAt = plan.LastSuccessfulRunAt,
					//Sources = op.Sources,
				};

				// Sources
				if (status == Commands.OperationStatus.PROCESSING_FILES_FINISHED
					|| status == Commands.OperationStatus.FINISHED
					|| status == Commands.OperationStatus.FAILED
					|| status == Commands.OperationStatus.CANCELED)
				{
					data.Sources = op.Sources;
				}
			}
			else if (RunningOperation is RestoreOperation)
			{
				RestoreOperation op = RunningOperation as RestoreOperation;
				Models.RestorePlan plan = Model as Models.RestorePlan;
				data = new Commands.GuiReportPlanStatus
				{
					Status = status,
					StartedAt = op.StartedAt,
					FinishedAt = op.FinishedAt,
					LastRunAt = plan.LastRunAt,
					LastSuccessfulRunAt = plan.LastSuccessfulRunAt,
					//Sources = op.Sources,
				};

				// Sources
				if (status == Commands.OperationStatus.PROCESSING_FILES_FINISHED
					|| status == Commands.OperationStatus.FINISHED
					|| status == Commands.OperationStatus.FAILED
					|| status == Commands.OperationStatus.CANCELED)
				{
					data.Sources = op.Sources;
				}
			}
			else
			{
				string message = string.Format("Type {0} is not handled", RunningOperation.GetType().FullName);
				throw new NotImplementedException(message);
			}

			return data;
		}

		private Commands.GuiReportPlanProgress BuildGuiReportPlanProgress(Commands.OperationStatus status)
		{
			TransferOperationReport report = RunningOperation.GetReport() as TransferOperationReport;
			Commands.GuiReportPlanProgress data = new Commands.GuiReportPlanProgress
			{
				Completed = report.TransferResults.Stats.Completed,
				Total = report.TransferResults.Stats.Total,
				BytesCompleted = report.TransferResults.Stats.BytesCompleted,
				BytesTotal = report.TransferResults.Stats.BytesTotal,
			};
			return data;
		}

		private void BackupUpdateStatsInfo(BackupOperationStatus status, TransferStatus xferStatus)
		{
			if (RunningOperation == null)
				return;

			Models.BackupPlan plan = Model as Models.BackupPlan;
			BackupOperation operation = RunningOperation as BackupOperation;
			BackupOperationReport report = operation.Report;

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
						logger.Info("Completed: {0} of {1}", report.TransferResults.Stats.Completed, report.TransferResults.Stats.Total);

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
						if (xferStatus == TransferStatus.COMPLETED || xferStatus == TransferStatus.CANCELED || xferStatus == TransferStatus.FAILED)
							logger.Info("Completed: {0} of {1}", report.TransferResults.Stats.Completed, report.TransferResults.Stats.Total);

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

		private void RestoreUpdateStatsInfo(RestoreOperationStatus status, TransferStatus xferStatus)
		{
			if (RunningOperation == null)
				return;

			Models.RestorePlan plan = Model as Models.RestorePlan;
			RestoreOperation operation = RunningOperation as RestoreOperation;
			RestoreOperationReport report = operation.Report;

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
						logger.Info("Completed: {0} of {1}", report.TransferResults.Stats.Completed, report.TransferResults.Stats.Total);

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
						if (xferStatus == TransferStatus.COMPLETED || xferStatus == TransferStatus.CANCELED || xferStatus == TransferStatus.FAILED)
							logger.Info("Completed: {0} of {1}", report.TransferResults.Stats.Completed, report.TransferResults.Stats.Total);

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
			AsyncHelper.SettingsMaxThreadCount = Teltec.Everest.Settings.Properties.Current.MaxThreadCount;
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

					RunningOperationEndedEvent.Dispose();
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
