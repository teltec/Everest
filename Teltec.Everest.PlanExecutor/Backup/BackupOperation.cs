using Amazon.Runtime;
using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;
using Teltec.Everest.Data.DAO;
using Teltec.Everest.PlanExecutor.Versioning;
using Teltec.Storage;
using Teltec.Storage.Backend;
using Teltec.Storage.Implementations.S3;
using Teltec.Storage.Versioning;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.PlanExecutor.Backup
{
	public enum BackupOperationStatus : byte
	{
		Unknown					= 0,
		Started					= 1,
		Resumed					= 2,
		ScanningFilesStarted	= 3,
		ScanningFilesFinished	= 4,
		ProcessingFilesStarted	= 5,
		ProcessingFilesFinished = 6,
		Updated					= 7,
		Canceled				= 8,
		Failed					= 9,
		Finished				= 10,
	}

	public static class Extensions
	{
		public static bool IsEnded(this BackupOperationStatus status)
		{
			return status == BackupOperationStatus.Canceled
				|| status == BackupOperationStatus.Failed
				|| status == BackupOperationStatus.Finished;
		}
	}

	public sealed class BackupOperationEvent : EventArgs
	{
		public BackupOperationStatus Status;
		public string Message;
		public TransferStatus TransferStatus; // Only matters when Status == BackupOperationStatus.UPDATE
	}

	public sealed class BackupOperationOptions
	{
		// ...
	}

	public enum BackupOperationState
	{
		UNKNOWN = 0,
		STARTING,
		MAPPING_NETWORK_DRIVES,
		EXECUTING_PRE_ACTIONS,
		SCANNING_FILES,
		UPDATING_SYNCED_FILES,
		VERSIONING_FILES,
		TRANSFERRING_FILES,
		EXECUTING_POST_ACTIONS,
		FINISHING,
	}

	public abstract class BackupOperation : BaseOperation<BackupOperationReport>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected readonly BackupRepository _daoBackup = new BackupRepository();

		protected BaseOperationHelper Helper;
		protected Models.Backup Backup;
		protected BackupOperationState CurrentState = BackupOperationState.UNKNOWN;

		#region Properties

		public override Int32? OperationId
		{
			get { return Backup.Id; }
		}

		public delegate void UpdateEventHandler(object sender, BackupOperationEvent e);
		public event UpdateEventHandler Updated;

		public DateTime? StartedAt
		{
			get { Assert.IsNotNull(Backup); return Backup.StartedAt; }
		}

		public DateTime? FinishedAt
		{
			get { Assert.IsNotNull(Backup); return Backup.FinishedAt; }
		}

		public string Sources
		{
			get
			{
				Debug.Assert(BackupAgent != null || Backup != null);
				const string delimiter = ", ", trail = "...";
				const int maxLength = 50;
				return BackupAgent != null
					? BackupAgent.FilesAsDelimitedString(delimiter, maxLength, trail)
					: Backup.BackupPlan.SelectedSourcesAsDelimitedString(delimiter, maxLength, trail);
			}
		}

		//private string _RootDir;
		//public string RootDir
		//{
		//	get
		//	{
		//		if (_RootDir == null)
		//			_RootDir = string.Format("backup-{0}", Plan.Id);
		//		return _RootDir;
		//	}
		//}

		#endregion

		#region Constructors

		public BackupOperation(BackupOperationOptions options)
		{
			Options = options;
		}

		#endregion

		#region Report

		public override void SendReport()
		{
			var plan = Backup.BackupPlan;
			var report = Report;

			if (plan.Notification == null || !plan.Notification.IsNotificationEnabled)
				return;

			// Aggregate report results (from versioner, transfer, etc).
			report.AggregateResults();

			switch (report.OperationStatus)
			{
				default:
					logger.Info("Will not send report email because this operation terminated with status {0}", report.OperationStatus.ToString());
					break;
				case OperationStatus.COMPLETED:
					switch (plan.Notification.WhenToNotify)
					{
						default: throw new InvalidOperationException(string.Format("Invalid TriggerCondition: {0}", plan.Notification.WhenToNotify.ToString()));
						case Models.PlanNotification.TriggerCondition.ALWAYS:
							SendReportByEmail(report, plan.Notification);
							break;
						case Models.PlanNotification.TriggerCondition.FAILED:
							break;
					}
					break;
				case OperationStatus.FAILED:
					switch (plan.Notification.WhenToNotify)
					{
						default: throw new InvalidOperationException(string.Format("Invalid TriggerCondition: {0}", plan.Notification.WhenToNotify.ToString()));
						case Models.PlanNotification.TriggerCondition.ALWAYS:
						case Models.PlanNotification.TriggerCondition.FAILED:
							SendReportByEmail(report, plan.Notification);
							break;
					}
					break;
			}
		}

		private void SendReportByEmail(BackupOperationReport report, Models.PlanNotification notification)
		{
			string statusStr = "unknown";
			switch (report.OperationStatus)
			{
				case OperationStatus.COMPLETED:
					statusStr = report.HasErrorMessages ? "completed with warnings" : "completed";
					break;
				case OperationStatus.FAILED:
					statusStr = "failed";
					break;
				case OperationStatus.CANCELED:
					statusStr = "canceled";
					break;
			}

			string mailRecipientAddress = notification.EmailAddress;
			string mailRecipientName = notification.FullName;
			string mailSubject = notification.GetFormattedSubject(report.PlanName, report.PlanType, statusStr);

			logger.Info("Sending a report email for {0} with status {1}", report.PlanType, report.OperationStatus.ToString());

			try
			{
				BackupOperationReportSender reportSender = new BackupOperationReportSender(report);
				Task<bool> result = reportSender.Send(mailRecipientName, mailRecipientAddress, mailSubject);
				result.Wait();
				logger.Info("Report email {0}", result.Result ? "was successfully sent" : string.Format("failed to be sent: {0}", reportSender.ReasonMessage));
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Warn, ex, "Failed to send report: ", ex.Message);
			}
		}

		#endregion

		#region Transfer

		protected IncrementalFileVersioner Versioner; // IDisposable
		protected BackupOperationOptions Options;
		protected CustomBackupAgent BackupAgent; // IDisposable

		public override void Start()
		{
			Assert.IsFalse(IsRunning);
			Assert.IsNotNull(Backup);
			Assert.IsNotNull(Backup.BackupPlan);
			Assert.IsNotNull(Backup.BackupPlan.StorageAccount);
			Assert.AreEqual(Models.EStorageAccountType.AmazonS3, Backup.BackupPlan.StorageAccountType);
			Assert.AreEqual(CurrentState, BackupOperationState.UNKNOWN);

			AmazonS3AccountRepository dao = new AmazonS3AccountRepository();
			Models.AmazonS3Account s3account = dao.GetForReadOnly(Backup.BackupPlan.StorageAccount.Id);

			//
			// Dispose and recycle previous objects, if needed.
			//
			if (TransferAgent != null)
				TransferAgent.Dispose();
			if (TransferListControl != null)
				TransferListControl.ClearTransfers();
			if (BackupAgent != null)
				BackupAgent.Dispose();
			if (Versioner != null)
				Versioner.Dispose();

			//
			// Setup agents.
			//
			AWSCredentials awsCredentials = new BasicAWSCredentials(s3account.AccessKey, s3account.SecretKey);
			TransferAgentOptions options = new TransferAgentOptions
			{
				UploadChunkSizeInBytes = Teltec.Everest.Settings.Properties.Current.UploadChunkSize * 1024 * 1024,
			};
			TransferAgent = new S3TransferAgent(options, awsCredentials, s3account.BucketName, CancellationTokenSource.Token);
			TransferAgent.RemoteRootDir = TransferAgent.PathBuilder.CombineRemotePath("TELTEC_BKP",
				Backup.BackupPlan.StorageAccount.Hostname);

			BackupAgent = new CustomBackupAgent(TransferAgent);
			BackupAgent.Results.Monitor = TransferListControl;

			Versioner = new IncrementalFileVersioner(CancellationTokenSource.Token);

			RegisterResultsEventHandlers(Backup, BackupAgent.Results);

			Report.PlanType = "backup";
			Report.PlanName = Backup.BackupPlan.Name;
			Report.BucketName = s3account.BucketName;
			Report.HostName = Backup.BackupPlan.StorageAccount.Hostname;
			Report.TransferResults = BackupAgent.Results;

			Helper = new BaseOperationHelper(Backup.BackupPlan);

			//
			// Start the backup.
			//
			DoBackup(BackupAgent, Backup, Options);
		}

		public void DeleteVersionedFile(string sourcePath, IFileVersion version, object identifier)
		{
			TransferAgent.DeleteVersionedFile(sourcePath, version, identifier);
		}

		protected void RegisterResultsEventHandlers(Models.Backup backup, TransferResults results)
		{
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository();
			results.DeleteCompleted += (object sender, DeletionArgs e) =>
			{
				Int64? backupedFileId = (Int64?)e.UserData;
				// TODO(jweyrich): We could get rid of the SELECT and perform just the UPDATE.
				Models.BackupedFile backupedFile = daoBackupedFile.Get(backupedFileId.Value);
				backupedFile.TransferStatus = TransferStatus.PURGED;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				//var message = string.Format("Purged {0}", e.FilePath);
				//Info(message);
				//OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message });
			};
			results.Failed += (object sender, TransferFileProgressArgs args) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.TransferStatus = TransferStatus.FAILED;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Failed {0} - {1}", args.FilePath, args.Exception != null ? args.Exception.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message, TransferStatus = TransferStatus.FAILED });
			};
			results.Canceled += (object sender, TransferFileProgressArgs args) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.TransferStatus = TransferStatus.CANCELED;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Canceled {0} - {1}", args.FilePath, args.Exception != null ? args.Exception.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message, TransferStatus = TransferStatus.CANCELED });
			};
			results.Completed += (object sender, TransferFileProgressArgs args) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.TransferStatus = TransferStatus.COMPLETED;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Completed {0}", args.FilePath);
				Info(message);
				OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message, TransferStatus = TransferStatus.COMPLETED });

				Models.BackupPlan plan = Backup.BackupPlan; //backupedFile.Backup.BackupPlan;

				if (plan.PurgeOptions != null && plan.PurgeOptions.IsTypeCustom && plan.PurgeOptions.EnabledKeepNumberOfVersions)
				{
					// Purge the oldest versioned files if the count of versions exceeds the maximum specified for the Backup Plan.
					IList<Models.BackupedFile> previousVersions = daoBackupedFile.GetCompleteByPlanAndPath(plan, args.FilePath);
					int found = previousVersions.Count;
					int keep = plan.PurgeOptions.NumberOfVersionsToKeep;
					int diff = found - keep;
					if (diff > 0)
					{
						// Delete the oldest Count-N versions.
						List<Models.BackupedFile> versionsToPurge = previousVersions.Skip(keep).ToList();
						foreach (var vp in versionsToPurge)
						{
							DeleteVersionedFile(vp.File.Path, new FileVersion { Version = vp.Version }, vp.Id);
						}
					}
				}
			};
			results.Started += (object sender, TransferFileProgressArgs args) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.TransferStatus = TransferStatus.RUNNING;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Started {0}", args.FilePath);
				Info(message);
				OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message });
			};
			results.Progress += (object sender, TransferFileProgressArgs args) =>
			{
#if DEBUG
				var message = string.Format("Progress {0}% {1} ({2}/{3} bytes)",
					args.PercentDone, args.FilePath, args.TransferredBytes, args.TotalBytes);
				//Info(message);
#endif
				OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = null });
			};
		}

		protected abstract Task<PathScanResults<string>> GetFilesToProcess(Models.Backup backup);
		protected abstract Task<FileVersionerResults> DoVersionFiles(Models.Backup backup, LinkedList<string> filesToProcess);

		// Update specific `BackupPlanFile`s that exist and are NOT yet associated to a `BackupPlan`.
		protected void DoUpdateSyncedFiles(Models.Backup backup, LinkedList<string> filesToProcess)
		{
			BackupPlanFileRepository dao = new BackupPlanFileRepository();

			long totalUpdates = 0;
			foreach (var path in filesToProcess)
			{
				// There's NO NEED to SELECT and UPDATE if we can UPDATE directly using a WHERE clause.
				totalUpdates += dao.AssociateSyncedFileToBackupPlan(backup.BackupPlan, path);
			}

			if (totalUpdates > 0)
				logger.Info("Associated {0} synced files to Backup Plan {1}", totalUpdates, backup.BackupPlan.Name);
			else
				logger.Info("There are no synced files to associate to Backup Plan {0}", backup.BackupPlan.Name);
		}

		protected async void DoBackup(CustomBackupAgent agent, Models.Backup backup, BackupOperationOptions options)
		{
			try
			{
				CurrentState = BackupOperationState.STARTING;
				OnStart(agent, backup);

				// Mount all network mappings and abort if there is any network mapping failure.
				CurrentState = BackupOperationState.MAPPING_NETWORK_DRIVES;
				Helper.MountAllNetworkDrives();

				// Execute pre-actions
				CurrentState = BackupOperationState.EXECUTING_PRE_ACTIONS;
				Helper.ExecutePreActions();

				//
				// Scanning
				//

				CurrentState = BackupOperationState.SCANNING_FILES;
				LinkedList<string> filesToProcess = null;
				{
					Task<PathScanResults<string>> filesToProcessTask = GetFilesToProcess(backup);

					{
						var message = string.Format("Scanning files started.");
						Info(message);
						//StatusInfo.Update(BackupStatusLevel.INFO, message);
						OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.ScanningFilesStarted, Message = message });
					}

					try
					{
						await filesToProcessTask;
					}
					catch (Exception ex)
					{
						if (ex.IsCancellation())
						{
							string message = string.Format("Scanning files was canceled.");

							Report.AddErrorMessage(message);
							logger.Warn(message);
						}
						else
						{
							string message = string.Format("Caught exception during scanning files: {0}", ex.Message);

							Report.AddErrorMessage(message);
							logger.Log(LogLevel.Error, ex, message);
						}

						if (filesToProcessTask.IsFaulted || filesToProcessTask.IsCanceled)
						{
							if (filesToProcessTask.IsCanceled)
								OnCancelation(agent, backup, ex); // filesToProcessTask.Exception
							else
								OnFailure(agent, backup, ex); // filesToProcessTask.Exception
							return;
						}
					}

					filesToProcess = filesToProcessTask.Result.Files;

					{
						foreach (var entry in filesToProcessTask.Result.FailedFiles)
							Report.AddErrorMessage(entry.Value);

						if (filesToProcessTask.Result.FailedFiles.Count > 0)
						{
							StringBuilder sb = new StringBuilder();
							sb.AppendLine("Scanning failed for the following drives/files/directories:");
							foreach (var entry in filesToProcessTask.Result.FailedFiles)
								sb.AppendLine(string.Format("  Path: {0} - Reason: {1}", entry.Key, entry.Value));
							Warn(sb.ToString());
						}

						var message = string.Format("Scanning files finished.");
						Info(message);
						//StatusInfo.Update(BackupStatusLevel.INFO, message);
						OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.ScanningFilesFinished, Message = message });
					}
				}

				//
				// Update synced files
				//

				CurrentState = BackupOperationState.UPDATING_SYNCED_FILES;
				{
					Task updateSyncedFilesTask = ExecuteOnBackround(() =>
						{
							DoUpdateSyncedFiles(backup, filesToProcess);
						}, CancellationTokenSource.Token);

					{
						var message = string.Format("Update of synced files started.");
						Info(message);
					}

					try
					{
						await updateSyncedFilesTask;
					}
					catch (Exception ex)
					{
						if (ex.IsCancellation())
						{
							string message = string.Format("Update of synced files was canceled.");

							Report.AddErrorMessage(message);
							logger.Warn(message);
						}
						else
						{
							string message = string.Format("Caught exception during update of synced files: {0}", ex.Message);

							Report.AddErrorMessage(message);
							logger.Log(LogLevel.Error, ex, message);
						}

						if (updateSyncedFilesTask.IsFaulted || updateSyncedFilesTask.IsCanceled)
						{
							Versioner.Undo();
							if (updateSyncedFilesTask.IsCanceled)
								OnCancelation(agent, backup, ex); // updateSyncedFilesTask.Exception
							else
								OnFailure(agent, backup, ex); // updateSyncedFilesTask.Exception
							return;
						}
					}

					{
						var message = string.Format("Update of synced files finished.");
						Info(message);
					}
				}

				//
				// Versioning
				//

				CurrentState = BackupOperationState.VERSIONING_FILES;
				{
					Task<FileVersionerResults> versionerTask = DoVersionFiles(backup, filesToProcess);
					Report.VersionerResults = versionerTask.Result;

					{
						var message = string.Format("Processing files started.");
						Info(message);
						//StatusInfo.Update(BackupStatusLevel.INFO, message);
						OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.ProcessingFilesStarted, Message = message });
					}

					try
					{
						await versionerTask;
					}
					catch (Exception ex)
					{
						if (ex.IsCancellation())
						{
							string message = string.Format("Processing files was canceled.");

							Report.AddErrorMessage(message);
							logger.Warn(message);
						}
						else
						{
							string message = string.Format("Caught exception during processing files: {0}", ex.Message);

							Report.AddErrorMessage(message);
							logger.Log(LogLevel.Error, ex, message);
						}

						if (versionerTask.IsFaulted || versionerTask.IsCanceled)
						{
							Versioner.Undo();
							if (versionerTask.IsCanceled)
								OnCancelation(agent, backup, ex); // versionerTask.Exception
							else
								OnFailure(agent, backup, ex); // versionerTask.Exception
							return;
						}
					}

					agent.Files = Versioner.FilesToTransfer;

					{
						var message = string.Format("Processing files finished.");
						Info(message);
						//StatusInfo.Update(BackupStatusLevel.INFO, message);
						OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.ProcessingFilesFinished, Message = message });
					}

					{
						agent.Results.Stats.BytesTotal = agent.EstimatedTransferSize;

						var message = string.Format("Estimate backup size: {0} files, {1}",
							agent.Files.Count(), FileSizeUtils.FileSizeToString(agent.EstimatedTransferSize));
						Info(message);
					}
				}

				//
				// Transfer files
				//

				CurrentState = BackupOperationState.TRANSFERRING_FILES;
				{
					Task<TransferResults> transferTask = agent.Start();
					Report.TransferResults = transferTask.Result;

					{
						var message = string.Format("Transfer files started.");
						Info(message);
					}

					try
					{
						await transferTask;
					}
					catch (Exception ex)
					{
						if (ex.IsCancellation())
						{
							string message = string.Format("Transfer files was canceled.");

							Report.TransferResults.ErrorMessages.Add(message);
							logger.Warn(message);
						}
						else
						{
							string message = string.Format("Caught exception during transfer files: {0}", ex.Message);

							Report.TransferResults.ErrorMessages.Add(message);
							logger.Log(LogLevel.Error, ex, message);
						}

						if (transferTask.IsFaulted || transferTask.IsCanceled)
						{
							if (transferTask.IsCanceled)
								OnCancelation(agent, backup, ex); // transferTask.Exception
							else
								OnFailure(agent, backup, ex); // transferTask.Exception
							return;
						}
					}

					{
						var message = string.Format("Transfer files finished.");
						Info(message);
					}
				}

				CurrentState = BackupOperationState.EXECUTING_POST_ACTIONS;
				Helper.ExecutePostActions(Report.TransferResults);

				CurrentState = BackupOperationState.FINISHING;
				OnFinish(agent, backup);
			}
			catch (Exception ex)
			{
				OnFinish(agent, backup, ex);
			}
		}

		public override void Cancel()
		{
			base.Cancel();
			DoCancel(BackupAgent);
		}

		protected void DoCancel(CustomBackupAgent agent)
		{
			agent.Cancel();
			CancellationTokenSource.Cancel();
		}

		private Task ExecuteOnBackround(Action action, CancellationToken token)
		{
			return Task.Run(action, token);
			//return AsyncHelper.ExecuteOnBackround(action, token);
		}

		#endregion

		#region Event handlers

		public virtual void OnStart(CustomBackupAgent agent, Models.Backup backup)
		{
			IsRunning = true;

			backup.DidStart();
		}

		protected void OnUpdate(BackupOperationEvent e)
		{
			if (Updated != null)
				Updated(this, e);
		}

		public void OnCancelation(CustomBackupAgent agent, Models.Backup backup, Exception exception)
		{
			IsRunning = false;

			var message = string.Format("Backup canceled: {0}", exception != null ? exception.Message : "Exception not informed");

			Report.AddErrorMessage(message);
			Error(message);
			//StatusInfo.Update(BackupStatusLevel.ERROR, message);

			backup.WasCanceled();
			_daoBackup.Update(backup);

			OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Canceled, Message = message });
		}

		public void OnFailure(CustomBackupAgent agent, Models.Backup backup, Exception exception)
		{
			IsRunning = false;

			logger.Log(LogLevel.Error, exception, "Caught exception: {0}", exception.Message);

			var message = string.Format("Backup failed: {0}", exception != null ? exception.Message : "Exception not informed");

			Report.AddErrorMessage(message);
			Error(message);
			//StatusInfo.Update(BackupStatusLevel.ERROR, message);

			backup.DidFail();
			_daoBackup.Update(backup);

			OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Failed, Message = message });
		}

		public void OnFinish(CustomBackupAgent agent, Models.Backup backup, Exception ex = null)
		{
			IsRunning = false;

			switch (CurrentState)
			{
				default:
					{
						var message = string.Format("Backup failed: {0}", ex.Message);
						Warn(message);
						//StatusInfo.Update(BackupStatusLevel.WARN, message);
						Report.AddErrorMessage(ex.Message);

						Report.OperationStatus = OperationStatus.FAILED;
						backup.DidFail();
						_daoBackup.Update(backup);
						OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Failed, Message = message });

						break;
					}
				case BackupOperationState.FINISHING:
					{
						TransferResults.Statistics stats = agent.Results.Stats;

						var message = string.Format(
							"Backup finished! Stats: {0} completed, {1} failed, {2} canceled, {3} pending, {4} running",
							stats.Completed, stats.Failed, stats.Canceled, stats.Pending, stats.Running);
						Info(message);
						//StatusInfo.Update(BackupStatusLevel.OK, message);

						switch (agent.Results.OverallStatus)
						//switch (backup.Status)
						{
							default: throw new InvalidOperationException("Unexpected TransferStatus");
							case TransferStatus.CANCELED:
								Report.OperationStatus = OperationStatus.CANCELED;
								backup.WasCanceled();
								_daoBackup.Update(backup);
								OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Canceled, Message = message });
								break;
							case TransferStatus.FAILED:
								Report.OperationStatus = OperationStatus.FAILED;
								backup.DidFail();
								_daoBackup.Update(backup);
								OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Failed, Message = message });
								break;
							case TransferStatus.COMPLETED:
								Report.OperationStatus = OperationStatus.COMPLETED;
								backup.DidComplete();
								_daoBackup.Update(backup);
								OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Finished, Message = message });
								break;
						}

						break;
					}
			}

			Report.StartedAt = Backup.StartedAt;
			Report.FinishedAt = Backup.FinishedAt.Value;
		}

		#endregion

		#region Dispose Pattern Implementation

		bool _shouldDispose = true;
		bool _isDisposed;

		/// <summary>
		/// Implements the Dispose pattern
		/// </summary>
		/// <param name="disposing">Whether this object is being disposed via a call to Dispose
		/// or garbage collected.</param>
		protected override void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					if (BackupAgent != null)
					{
						BackupAgent.Dispose();
						BackupAgent = null;
					}

					if (Versioner != null)
					{
						Versioner.Dispose();
						Versioner = null;
					}
				}
				this._isDisposed = true;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
