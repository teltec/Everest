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
using Teltec.Backup.Data.DAO;
using Teltec.Backup.PlanExecutor.Versioning;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;
using Teltec.Storage;
using Teltec.Storage.Backend;
using Teltec.Storage.Implementations.S3;
using Teltec.Storage.Versioning;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Backup
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

	public abstract class BackupOperation : BaseOperation<TransferResults>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected readonly BackupRepository _daoBackup = new BackupRepository();

		protected Models.Backup Backup;

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

		#region Transfer

		protected IncrementalFileVersioner Versioner; // IDisposable
		protected BackupOperationOptions Options;
		protected CustomBackupAgent BackupAgent; // IDisposable

		public override void Start(out TransferResults results)
		{
			Assert.IsFalse(IsRunning);
			Assert.IsNotNull(Backup);
			Assert.IsNotNull(Backup.BackupPlan);
			Assert.IsNotNull(Backup.BackupPlan.StorageAccount);
			Assert.AreEqual(Models.EStorageAccountType.AmazonS3, Backup.BackupPlan.StorageAccountType);

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
				UploadChunkSizeInBytes = Teltec.Backup.Settings.Properties.Current.UploadChunkSize * 1024 * 1024,
			};
			TransferAgent = new S3TransferAgent(options, awsCredentials, s3account.BucketName, CancellationTokenSource.Token);
			TransferAgent.RemoteRootDir = TransferAgent.PathBuilder.CombineRemotePath("TELTEC_BKP",
				Backup.BackupPlan.StorageAccount.Hostname);

			BackupAgent = new CustomBackupAgent(TransferAgent);
			BackupAgent.Results.Monitor = TransferListControl;

			Versioner = new IncrementalFileVersioner(CancellationTokenSource.Token);

			RegisterResultsEventHandlers(Backup, BackupAgent.Results);

			results = BackupAgent.Results;

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
			results.Failed += (object sender, TransferFileProgressArgs args, Exception ex) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.TransferStatus = TransferStatus.FAILED;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Failed {0} - {1}", args.FilePath, ex != null ? ex.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message, TransferStatus = TransferStatus.FAILED });
			};
			results.Canceled += (object sender, TransferFileProgressArgs args, Exception ex) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.TransferStatus = TransferStatus.CANCELED;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Canceled {0} - {1}", args.FilePath, ex != null ? ex.Message : "Unknown reason");
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
		protected abstract Task DoVersionFiles(Models.Backup backup, LinkedList<string> filesToProcess);

		// Update specific `BackupPlanFile`s that exist and are NOT yet associated to a `BackupPlan`.
		protected void DoUpdateSyncedFiles(Models.Backup backup, LinkedList<string> filesToProcess)
		{
			BackupPlanFileRepository dao = new BackupPlanFileRepository();

			int totalUpdates = 0;
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
			OnStart(agent, backup);

			//
			// Scanning
			//

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
						logger.Warn("Scanning files was canceled.");
					}
					else
					{
						logger.Log(LogLevel.Error, ex, "Caught exception during scanning files");
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
						logger.Warn("Update of synced files was canceled.");
					}
					else
					{
						logger.Log(LogLevel.Error, ex, "Caught exception during update of synced files");
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

			{
				Task versionerTask = DoVersionFiles(backup, filesToProcess);

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
						logger.Warn("Processing files was canceled.");
					}
					else
					{
						logger.Log(LogLevel.Error, ex, "Caught exception during processing files");
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

			{
				Task transferTask = agent.Start();

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
						logger.Warn("Transfer files was canceled.");
					}
					else
					{
						logger.Log(LogLevel.Error, ex, "Caught exception during transfer files");
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

			OnFinish(agent, backup);
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
			Error(message);
			//StatusInfo.Update(BackupStatusLevel.ERROR, message);

			backup.DidFail();
			_daoBackup.Update(backup);

			OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Failed, Message = message });
		}

		public void OnFinish(CustomBackupAgent agent, Models.Backup backup)
		{
			IsRunning = false;

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
					backup.WasCanceled();
					_daoBackup.Update(backup);
					OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Canceled, Message = message });
					break;
				case TransferStatus.FAILED:
					backup.DidFail();
					_daoBackup.Update(backup);
					OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Failed, Message = message });
					break;
				case TransferStatus.COMPLETED:
					backup.DidComplete();
					_daoBackup.Update(backup);
					OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Finished, Message = message });
					break;
			}
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
