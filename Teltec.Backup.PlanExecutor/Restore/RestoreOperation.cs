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
using Teltec.Backup.Data.Versioning;
using Teltec.Backup.PlanExecutor.Versioning;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;
using Teltec.FileSystem;
using Teltec.Storage;
using Teltec.Storage.Backend;
using Teltec.Storage.Implementations.S3;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Restore
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

	public abstract class RestoreOperation : BaseOperation<TransferResults>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected readonly RestoreRepository _daoRestore = new RestoreRepository();

		protected Models.Restore Restore;

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

		#region Transfer

		protected RestoreFileVersioner Versioner; // IDisposable
		protected RestoreOperationOptions Options;
		protected CustomRestoreAgent RestoreAgent;

		public override void Start(out TransferResults results)
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
				UploadChunkSizeInBytes = Teltec.Backup.Settings.Properties.Current.UploadChunkSize * 1024 * 1024,
			};
			TransferAgent = new S3TransferAgent(options, awsCredentials, s3account.BucketName, CancellationTokenSource.Token);
			TransferAgent.RemoteRootDir = TransferAgent.PathBuilder.CombineRemotePath("TELTEC_BKP",
				Restore.RestorePlan.StorageAccount.Hostname);

			RestoreAgent = new CustomRestoreAgent(TransferAgent);
			RestoreAgent.Results.Monitor = TransferListControl;

			Versioner = new RestoreFileVersioner(CancellationTokenSource.Token);

			RegisterResultsEventHandlers(Restore, RestoreAgent.Results);

			results = RestoreAgent.Results;

			//
			// Start the backup.
			//
			DoRestore(RestoreAgent, Restore, Options);
		}

		protected void RegisterResultsEventHandlers(Models.Restore restore, TransferResults results)
		{
			RestoredFileRepository daoRestoredFile = new RestoredFileRepository();
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository();
			results.Failed += (object sender, TransferFileProgressArgs args, Exception ex) =>
			{
				Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(restore, args.FilePath);
				restoredFile.TransferStatus = TransferStatus.FAILED;
				restoredFile.UpdatedAt = DateTime.UtcNow;
				daoRestoredFile.Update(restoredFile);

				var message = string.Format("Failed {0} - {1}", args.FilePath, ex != null ? ex.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = message, TransferStatus = TransferStatus.FAILED });
			};
			results.Canceled += (object sender, TransferFileProgressArgs args, Exception ex) =>
			{
				Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(restore, args.FilePath);
				restoredFile.TransferStatus = TransferStatus.CANCELED;
				restoredFile.UpdatedAt = DateTime.UtcNow;
				daoRestoredFile.Update(restoredFile);

				var message = string.Format("Canceled {0} - {1}", args.FilePath, ex != null ? ex.Message : "Unknown reason");
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
			OnStart(agent, restore);

			//
			// Scanning
			//

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
						logger.Warn("Scanning files was canceled.");
					}
					else
					{
						logger.Log(LogLevel.Error, ex, "Caught exception during scanning files");
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

					var message = string.Format("Estimate restore size: {0} files, {1}",
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

			OnFinish(agent, restore);
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

			Error(message);
			//StatusInfo.Update(RestoreStatusLevel.ERROR, message);

			restore.DidFail();
			_daoRestore.Update(restore);

			OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Failed, Message = message });
		}

		public void OnFinish(CustomRestoreAgent agent, Models.Restore restore)
		{
			IsRunning = false;

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
					restore.WasCanceled();
					_daoRestore.Update(restore);
					OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Canceled, Message = message });
					break;
				case TransferStatus.FAILED:
					restore.DidFail();
					_daoRestore.Update(restore);
					OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Failed, Message = message });
					break;
				case TransferStatus.COMPLETED:
					restore.DidComplete();
					_daoRestore.Update(restore);
					OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Finished, Message = message });
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
