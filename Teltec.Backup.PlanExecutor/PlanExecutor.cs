using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using Teltec.Backup.App;
using Teltec.Backup.App.Backup;
using Teltec.Backup.App.Restore;
using Teltec.Backup.Data.DAO;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor
{
	// https://github.com/gsscoder/commandline
	class Options
	{
		[CommandLine.Option('t', "type", Required = true, HelpText = "Inform the type of the plan to be executed: (backup | restore)")]
		public string PlanType { get; set; }

		[CommandLine.Option('p', "plan", Required = true, HelpText = "Inform the ID of the plan to be executed.")]
		public Int32 PlanIdentifier { get; set; }

		[CommandLine.Option('v', "verbose", DefaultValue = true, HelpText = "Print all messages to standard output.")]
		public bool Verbose { get; set; }

		[CommandLine.HelpOption]
		public string GetUsage()
		{
			return CommandLine.Text.HelpText.AutoBuild(this,
				(CommandLine.Text.HelpText current) => CommandLine.Text.HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}

	public enum PlanTypeEnum
	{
		Backup = 0,
		Restore = 1,
	}

	class PlanExecutor
	{
		static readonly Logger logger = LogManager.GetCurrentClassLogger();

		static readonly Options options = new Options();

		static void Main(string[] args)
		{
			if (!CommandLine.Parser.Default.ParseArguments(args, options))
				return;

			try
			{
				var executor = new PlanExecutor();
				executor.Run();
				LetMeDebugThisBeforeExiting();
			}
			catch (Exception ex)
			{
				if (options.Verbose)
					logger.FatalException("Oops! An unexpected problem happened", ex);
				else
					logger.Fatal("Oops! An unexpected problem happened: {0}", ex.Message);

				LetMeDebugThisBeforeExiting();
				Environment.Exit(1);
			}
		}

		static void LetMeDebugThisBeforeExiting()
		{
			if (Debugger.IsAttached)
			{
				//Debugger.Break();
				Console.WriteLine("\nPress ENTER to exit.\n");
				Console.ReadLine();
			}
		}

		readonly BackupPlanRepository _daoBackupPlan = new BackupPlanRepository();
		readonly RestorePlanRepository _daoRestorePlan = new RestorePlanRepository();
		object Model = null;

		BaseOperation RunningOperation = null;
		TransferResults TransferResults = null;
		bool MustResumeLastOperation = false;

		/// <summary>
		///  Event set when the process is terminated.
		/// </summary>
		static readonly ManualResetEvent RunningOperationEndedEvent = new ManualResetEvent(false);

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

			RunOperation(selectedPlanType, plan);

			while (!RunningOperationEndedEvent.WaitOne(250))
			{
				RunningOperation.DoEvents();
			}

			Console.WriteLine("Operation finished.");
		}

		private void RunOperation(PlanTypeEnum planType, object plan)
		{
			switch (planType)
			{
				case PlanTypeEnum.Backup:
					{
						RunBackupOperation(plan as Models.BackupPlan);
						break;
					}
				case PlanTypeEnum.Restore:
					{
						RunRestoreOperation(plan as Models.RestorePlan);
						break;
					}
			}
		}

		private void ExitShowingHelpText(int exitCode)
		{
			CommandLine.Parser.Default.Settings.HelpWriter.Write(options.GetUsage());

			LetMeDebugThisBeforeExiting();
			Environment.Exit(exitCode);
		}

		private void RunBackupOperation(Models.BackupPlan plan)
		{
			var dao = new BackupRepository();

			Models.Backup latest = dao.GetLatestByPlan(plan);
			MustResumeLastOperation = latest != null && latest.NeedsResume();

			if (MustResumeLastOperation)
			{
				logger.Warn(
					"The backup (#{0}) has not finished yet."
					+ " If it's still running, please, wait until it finishes,"
					+ " otherwise you should resume it manually.",
					latest.Id);
				return;
			}

			// Create new backup or resume the last unfinished one.
			BackupOperation obj = /* MustResumeLastOperation
				? new ResumeBackupOperation(latest) as BackupOperation
				: */ new NewBackupOperation(plan) as BackupOperation;

			obj.Updated += (sender2, e2) => BackupUpdateStatsInfo(e2.Status);
			//obj.EventLog = ...
			//obj.TransferListControl = ...

			BackupUpdateStatsInfo(BackupOperationStatus.Unknown);

			RunningOperation = obj;

			//
			// Actually RUN it
			//
			RunningOperation.Start(out TransferResults);
		}

		private void RunRestoreOperation(Models.RestorePlan plan)
		{
			var dao = new RestoreRepository();

			Models.Restore latest = dao.GetLatestByPlan(plan);
			MustResumeLastOperation = latest != null && latest.NeedsResume();

			if (MustResumeLastOperation)
			{
				logger.Warn(
					"The restore (#{0}) has not finished yet."
					+ " If it's still running, please, wait until it finishes,"
					+ " otherwise you should resume it manually.",
					latest.Id);
				return;
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
		}

		private void BackupUpdateStatsInfo(BackupOperationStatus status)
		{
			if (RunningOperation == null)
				return;

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
						break;
					}
				case BackupOperationStatus.ScanningFilesStarted:
					{
						logger.Info("Scanning files...");
						break;
					}
				case BackupOperationStatus.ScanningFilesFinished:
					{
						logger.Info("Scanning files finished.");
						break;
					}
				case BackupOperationStatus.ProcessingFilesStarted:
					{
						logger.Info("Processing files...");
						break;
					}
				case BackupOperationStatus.ProcessingFilesFinished:
					{
						logger.Info("Processing files finished.");
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);
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
						Models.BackupPlan plan = Model as Models.BackupPlan;
						plan.LastRunAt = plan.LastSuccessfulRunAt = DateTime.UtcNow;
						_daoBackupPlan.Update(plan);

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();
						break;
					}
				case BackupOperationStatus.Updated:
					{
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);
						/*
						this.lblFilesTransferred.Text = string.Format("{0} of {1}",
							BackupResults.Stats.Completed, BackupResults.Stats.Total);
						*/
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
						Models.BackupPlan plan = Model as Models.BackupPlan;
						plan.LastRunAt = DateTime.UtcNow;
						_daoBackupPlan.Update(plan);

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();
						break;
					}
			}
		}

		private void RestoreUpdateStatsInfo(RestoreOperationStatus status)
		{
			if (RunningOperation == null)
				return;

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
						break;
					}
				case RestoreOperationStatus.ScanningFilesStarted:
					{
						logger.Info("Scanning files...");
						/*
						this.lblSources.Text = "Scanning files...";
						*/
						break;
					}
				case RestoreOperationStatus.ScanningFilesFinished:
					{
						logger.Info("Scanning files finished.");
						break;
					}
				case RestoreOperationStatus.ProcessingFilesStarted:
					{
						logger.Info("Processing files...");
						/*
						this.lblSources.Text = "Processing files...";
						*/
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
						Models.RestorePlan plan = Model as Models.RestorePlan;
						plan.LastRunAt = plan.LastSuccessfulRunAt = DateTime.UtcNow;
						_daoRestorePlan.Update(plan);

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();
						break;
					}
				case RestoreOperationStatus.Updated:
					{
						logger.Info("Completed: {0} of {1}", TransferResults.Stats.Completed, TransferResults.Stats.Total);
						/*
						this.lblFilesTransferred.Text = string.Format("{0} of {1}",
							RestoreResults.Stats.Completed, RestoreResults.Stats.Total);
						*/
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
						Models.RestorePlan plan = Model as Models.RestorePlan;
						plan.LastRunAt = DateTime.UtcNow;
						_daoRestorePlan.Update(plan);

						// Signal to the other thread it may terminate.
						RunningOperationEndedEvent.Set();
						break;
					}
			}
		}
	}
}
