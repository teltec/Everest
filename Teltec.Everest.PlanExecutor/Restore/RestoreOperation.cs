/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

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
using Teltec.Everest.Data.Versioning;
using Teltec.Everest.PlanExecutor.Versioning;
using Teltec.FileSystem;
using Teltec.Storage;
using Teltec.Storage.Backend;
using Teltec.Storage.Implementations.S3;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.PlanExecutor.Restore
{
	public enum RestoreOperationStatus : byte
	{
		Unknown = 0,
		Started = 1,
		Resumed = 2,
		ScanningFilesStarted = 3,
		ScanningFilesFinished = 4,
		ProcessingFilesStarted = 5,
		ProcessingFilesFinished = 6,
		Updated = 7,
		Canceled = 8,
		Failed = 9,
		Finished = 10,
	}

	public static class Extensions
	{
		public static bool IsEnded(this RestoreOperationStatus status)
		{
			return status == RestoreOperationStatus.Canceled
				|| status == RestoreOperationStatus.Failed
				|| status == RestoreOperationStatus.Finished;
		}
	}

	public sealed class RestoreOperationEvent : EventArgs
	{
		public RestoreOperationStatus Status;
		public string Message;
		public TransferStatus TransferStatus; // Only matters when Status == BackupOperationStatus.UPDATE
	}

	public sealed class RestoreOperationOptions
	{
		// ...
	}

	public enum RestoreOperationState
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

	public abstract class RestoreOperation : BaseOperation<RestoreOperationReport>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected readonly RestoreRepository _daoRestore = new RestoreRepository();

		protected BaseOperationHelper Helper;
		protected Models.Restore Restore;
		protected RestoreOperationState CurrentState = RestoreOperationState.UNKNOWN;

		#region Properties

		public override Int32? OperationId
		{
			get { return Restore.Id; }
		}

		public delegate void UpdateEventHandler(object sender, RestoreOperationEvent e);
		public event UpdateEventHandler Updated;

		public DateTime? StartedAt
		{
			get { Assert.IsNotNull(Restore); return Restore.StartedAt; }
		}

		public DateTime? FinishedAt
		{
			get { Assert.IsNotNull(Restore); return Restore.FinishedAt; }
		}

		public string Sources
		{
			get
			{
				Debug.Assert(RestoreAgent != null || Restore != null);
				const string delimiter = ", ", trail = "...";
				const int maxLength = 50;
				return RestoreAgent != null
					? RestoreAgent.FilesAsDelimitedString(delimiter, maxLength, trail)
					: Restore.RestorePlan.SelectedSourcesAsDelimitedString(delimiter, maxLength, trail);
			}
		}

		#endregion

		#region Constructors

		public RestoreOperation(RestoreOperationOptions options)
		{
			Options = options;
		}

		#endregion

		#region Report

		public override void SendReport()
		{
			var plan = Restore.RestorePlan;
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

		private void SendReportByEmail(RestoreOperationReport report, Models.PlanNotification notification)
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
				RestoreOperationReportSender reportSender = new RestoreOperationReportSender(report);
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

		protected RestoreFileVersioner Versioner; // IDisposable
		protected RestoreOperationOptions Options;
		protected CustomRestoreAgent RestoreAgent;

		public override void Start()
		{
			Assert.IsFalse(IsRunning);
			Assert.IsNotNull(Restore);
			Assert.IsNotNull(Restore.RestorePlan.StorageAccount);
			Assert.AreEqual(Models.EStorageAccountType.AmazonS3, Restore.RestorePlan.StorageAccountType);

			AmazonS3AccountRepository dao = new AmazonS3AccountRepository();
			Models.AmazonS3Account s3account = dao.GetForReadOnly(Restore.RestorePlan.StorageAccount.Id);

			//
			// Dispose and recycle previous objects, if needed.
			//
			if (TransferAgent != null)
				TransferAgent.Dispose();
			if (TransferListControl != null)
				TransferListControl.ClearTransfers();
			if (RestoreAgent != null)
				RestoreAgent.Dispose();
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
				Restore.RestorePlan.StorageAccount.Hostname);

			RestoreAgent = new CustomRestoreAgent(TransferAgent);
			RestoreAgent.Results.Monitor = TransferListControl;

			Versioner = new RestoreFileVersioner(CancellationTokenSource.Token);

			RegisterResultsEventHandlers(Restore, RestoreAgent.Results);

			Report.PlanType = "restore";
			Report.PlanName = Restore.RestorePlan.Name;
			Report.BucketName = s3account.BucketName;
			Report.HostName = Restore.RestorePlan.StorageAccount.Hostname;
			Report.TransferResults = RestoreAgent.Results;

			Helper = new BaseOperationHelper(Restore.RestorePlan);

			//
			// Start the restore.
			//
			DoRestore(RestoreAgent, Restore, Options);
		}

		protected void RegisterResultsEventHandlers(Models.Restore restore, TransferResults results)
		{
			RestoredFileRepository daoRestoredFile = new RestoredFileRepository();
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository();
			results.Failed += (object sender, TransferFileProgressArgs args) =>
			{
				Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(restore, args.FilePath);
				restoredFile.TransferStatus = TransferStatus.FAILED;
				restoredFile.UpdatedAt = DateTime.UtcNow;
				daoRestoredFile.Update(restoredFile);

				var message = string.Format("Failed {0} - {1}", args.FilePath, args.Exception != null ? args.Exception.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = message, TransferStatus = TransferStatus.FAILED });
			};
			results.Canceled += (object sender, TransferFileProgressArgs args) =>
			{
				Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(restore, args.FilePath);
				restoredFile.TransferStatus = TransferStatus.CANCELED;
				restoredFile.UpdatedAt = DateTime.UtcNow;
				daoRestoredFile.Update(restoredFile);

				var message = string.Format("Canceled {0} - {1}", args.FilePath, args.Exception != null ? args.Exception.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = message, TransferStatus = TransferStatus.CANCELED });
			};
			results.Completed += (object sender, TransferFileProgressArgs args) =>
			{
				Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(restore, args.FilePath);
				restoredFile.TransferStatus = TransferStatus.COMPLETED;
				restoredFile.UpdatedAt = DateTime.UtcNow;
				daoRestoredFile.Update(restoredFile);

				// Only set original modified date if the restored file is the latest version whose transfer is completed,
				// otherwise, keep the date the OS/filesystem gave it.
				bool isLatestVersion = daoBackupedFile.IsLatestVersion(restoredFile.BackupedFile);
				if (isLatestVersion)
				{
					// Set original LastWriteTime so this file won't be erroneously included in the next Backup.
					FileManager.SafeSetFileLastWriteTimeUtc(restoredFile.File.Path, restoredFile.BackupedFile.FileLastWrittenAt);
				}
				else
				{
					// Keep the original LastWriteTime so this file will be included in the next backup.
				}

				var message = string.Format("Completed {0}", args.FilePath);
				Info(message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = message, TransferStatus = TransferStatus.COMPLETED });
			};
			results.Started += (object sender, TransferFileProgressArgs args) =>
			{
				Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(restore, args.FilePath);
				restoredFile.TransferStatus = TransferStatus.RUNNING;
				restoredFile.UpdatedAt = DateTime.UtcNow;
				daoRestoredFile.Update(restoredFile);

				var message = string.Format("Started {0}", args.FilePath);
				Info(message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = message });
			};
			results.Progress += (object sender, TransferFileProgressArgs args) =>
			{
#if DEBUG
				var message = string.Format("Progress {0}% {1} ({2}/{3} bytes)",
					args.PercentDone, args.FilePath, args.TransferredBytes, args.TotalBytes);
				//Info(message);
#endif
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = null });
			};
		}

		protected abstract Task<PathScanResults<CustomVersionedFile>> GetFilesToProcess(Models.Restore restore);
		protected abstract Task DoVersionFiles(Models.Restore restore, LinkedList<CustomVersionedFile> files);

		protected async void DoRestore(CustomRestoreAgent agent, Models.Restore restore, RestoreOperationOptions options)
		{
			try
			{
				CurrentState = RestoreOperationState.STARTING;
				OnStart(agent, restore);

				// Mount all network mappings and abort if there is any network mapping failure.
				CurrentState = RestoreOperationState.MAPPING_NETWORK_DRIVES;
				Helper.MountAllNetworkDrives();

				// Execute pre-actions
				CurrentState = RestoreOperationState.EXECUTING_PRE_ACTIONS;
				Helper.ExecutePreActions();

				//
				// Scanning
				//

				CurrentState = RestoreOperationState.SCANNING_FILES;
				LinkedList<CustomVersionedFile> filesToProcess = null;
				{
					Task<PathScanResults<CustomVersionedFile>> filesToProcessTask = GetFilesToProcess(restore);

					{
						var message = string.Format("Scanning files started.");
						Info(message);
						//StatusInfo.Update(BackupStatusLevel.INFO, message);
						OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.ScanningFilesStarted, Message = message });
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
								OnCancelation(agent, restore, ex); // filesToProcessTask.Exception
							else
								OnFailure(agent, restore, ex); // filesToProcessTask.Exception
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
						OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.ScanningFilesFinished, Message = message });
					}
				}

				//
				// Versioning
				//

				CurrentState = RestoreOperationState.VERSIONING_FILES;
				{
					Task versionerTask = DoVersionFiles(restore, filesToProcess);

					{
						var message = string.Format("Processing files started.");
						Info(message);
						//StatusInfo.Update(RestoreStatusLevel.INFO, message);
						OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.ProcessingFilesStarted, Message = message });
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
								OnCancelation(agent, restore, ex); // versionerTask.Exception
							else
								OnFailure(agent, restore, ex); // versionerTask.Exception
							return;
						}
					}

					agent.Files = Versioner.FilesToTransfer;

					{
						var message = string.Format("Processing files finished.");
						Info(message);
						//StatusInfo.Update(BackupStatusLevel.INFO, message);
						OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.ProcessingFilesFinished, Message = message });
					}

					{
						agent.Results.Stats.BytesTotal = agent.EstimatedTransferSize;

						var message = string.Format("Estimated restore size: {0} files, {1}",
							agent.Files.Count(), FileSizeUtils.FileSizeToString(agent.EstimatedTransferSize));
						Info(message);
					}
				}

				//
				// Transfer files
				//

				CurrentState = RestoreOperationState.TRANSFERRING_FILES;
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

							Report.AddErrorMessage(message);
							logger.Warn(message);
						}
						else
						{
							string message = string.Format("Caught exception during transfer files: {0}", ex.Message);

							Report.AddErrorMessage(message);
							logger.Log(LogLevel.Error, ex, message);
						}

						if (transferTask.IsFaulted || transferTask.IsCanceled)
						{
							if (transferTask.IsCanceled)
								OnCancelation(agent, restore, ex); // transferTask.Exception
							else
								OnFailure(agent, restore, ex); // transferTask.Exception
							return;
						}
					}

					{
						var message = string.Format("Transfer files finished.");
						Info(message);
					}
				}

				CurrentState = RestoreOperationState.EXECUTING_POST_ACTIONS;
				Helper.ExecutePostActions(Report.TransferResults);

				CurrentState = RestoreOperationState.FINISHING;
				OnFinish(agent, restore);
			}
			catch (Exception ex)
			{
				OnFinish(agent, restore, ex);
			}
		}

		public override void Cancel()
		{
			base.Cancel();
			DoCancel(RestoreAgent);
		}

		protected void DoCancel(CustomRestoreAgent agent)
		{
			agent.Cancel();
			CancellationTokenSource.Cancel();
		}

		#endregion

		#region Event handlers

		public virtual void OnStart(CustomRestoreAgent agent, Models.Restore restore)
		{
			IsRunning = true;

			restore.DidStart();
		}

		protected void OnUpdate(RestoreOperationEvent e)
		{
			if (Updated != null)
				Updated(this, e);
		}

		public void OnCancelation(CustomRestoreAgent agent, Models.Restore restore, Exception exception)
		{
			IsRunning = false;

			var message = string.Format("Restore canceled: {0}", exception != null ? exception.Message : "Exception not informed");
			Report.AddErrorMessage(message);
			Error(message);
			//StatusInfo.Update(RestoreStatusLevel.ERROR, message);

			restore.DidFail();
			_daoRestore.Update(restore);

			OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Failed, Message = message });
		}

		public void OnFailure(CustomRestoreAgent agent, Models.Restore restore, Exception exception)
		{
			IsRunning = false;

			var message = string.Format("Restore failed: {0}", exception != null ? exception.Message : "Canceled?");

			Report.AddErrorMessage(message);
			Error(message);
			//StatusInfo.Update(RestoreStatusLevel.ERROR, message);

			restore.DidFail();
			_daoRestore.Update(restore);

			OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Failed, Message = message });
		}

		public void OnFinish(CustomRestoreAgent agent, Models.Restore restore, Exception ex = null)
		{
			IsRunning = false;

			switch (CurrentState)
			{
				default:
					{
						var message = string.Format("Restore failed: {0}", ex.Message);
						Warn(message);
						//StatusInfo.Update(RestoreStatusLevel.WARN, message);
						Report.AddErrorMessage(ex.Message);

						Report.OperationStatus = OperationStatus.FAILED;
						restore.DidFail();
						_daoRestore.Update(restore);
						OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Failed, Message = message });

						break;
					}
				case RestoreOperationState.FINISHING:
					{
						TransferResults.Statistics stats = agent.Results.Stats;

						var message = string.Format(
							"Restore finished! Stats: {0} completed, {1} failed, {2} canceled, {3} pending, {4} running",
							stats.Completed, stats.Failed, stats.Canceled, stats.Pending, stats.Running);
						Info(message);
						//StatusInfo.Update(RestoreStatusLevel.OK, message);

						switch (agent.Results.OverallStatus)
						//switch (backup.Status)
						{
							default: throw new InvalidOperationException("Unexpected TransferStatus");
							case TransferStatus.CANCELED:
								Report.OperationStatus = OperationStatus.CANCELED;
								restore.WasCanceled();
								_daoRestore.Update(restore);
								OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Canceled, Message = message });
								break;
							case TransferStatus.FAILED:
								Report.OperationStatus = OperationStatus.FAILED;
								restore.DidFail();
								_daoRestore.Update(restore);
								OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Failed, Message = message });
								break;
							case TransferStatus.COMPLETED:
								Report.OperationStatus = OperationStatus.COMPLETED;
								restore.DidComplete();
								_daoRestore.Update(restore);
								OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Finished, Message = message });
								break;
						}

						break;
					}
			}

			Report.StartedAt = Restore.StartedAt;
			Report.FinishedAt = Restore.FinishedAt.Value;
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
					if (RestoreAgent != null)
					{
						RestoreAgent.Dispose();
						RestoreAgent = null;
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
