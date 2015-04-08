using Amazon.Runtime;
using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Versioning;
using Teltec.Storage;
using Teltec.Storage.Agent;
using Teltec.Storage.Implementations.S3;
using Teltec.Storage.Utils;

namespace Teltec.Backup.App.Restore
{
	public enum RestoreOperationStatus
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
	}

	public sealed class RestoreOperationOptions
	{
		// ...
	}

	public abstract class RestoreOperation : BaseOperation
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected readonly RestoreRepository _daoRestore = new RestoreRepository();

		protected Models.Restore Restore;

		#region Properties

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

		#endregion

		#region Constructors

		public RestoreOperation(RestoreOperationOptions options)
		{
			Options = options;
		}

		#endregion

		#region Transfer

		protected IncrementalFileVersioner Versioner; // IDisposable
		protected RestoreOperationOptions Options;
		protected CustomRestoreAgent RestoreAgent;

		public override void Start(out TransferResults results)
		{
			Assert.IsFalse(IsRunning);
			Assert.IsNotNull(Restore);
			Assert.IsNotNull(Restore.RestorePlan.BackupPlan);
			Assert.IsNotNull(Restore.RestorePlan.BackupPlan.StorageAccount);
			Assert.AreEqual(Models.EStorageAccountType.AmazonS3, Restore.RestorePlan.BackupPlan.StorageAccountType);

			AmazonS3AccountRepository dao = new AmazonS3AccountRepository();
			Models.AmazonS3Account s3account = dao.Get(Restore.RestorePlan.BackupPlan.StorageAccount.Id);

			//
			// Dispose and recycle previous objects, if needed.
			//
			if (TransferAgent != null)
				TransferAgent.Dispose();
			if (TransferListControl != null)
				TransferListControl.ClearTransfers();
			if (Versioner != null)
				Versioner.Dispose();

			//
			// Setup agents.
			//
			AWSCredentials awsCredentials = new BasicAWSCredentials(s3account.AccessKey, s3account.SecretKey);
			TransferAgent = new S3AsyncTransferAgent(awsCredentials, s3account.BucketName);
			TransferAgent.RemoteRootDir = string.Format("backup-plan-{0}", Restore.RestorePlan.BackupPlan.Id);

			RestoreAgent = new CustomRestoreAgent(TransferAgent);
			RestoreAgent.Results.Monitor = TransferListControl;

			//Versioner = new IncrementalFileVersioner(CancellationTokenSource.Token);

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
			results.Failed += (object sender, TransferFileProgressArgs args, Exception ex) =>
			{
				Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(restore, args.FilePath);
				restoredFile.TransferStatus = TransferStatus.FAILED;
				restoredFile.UpdatedAt = DateTime.UtcNow;
				daoRestoredFile.Update(restoredFile);

				var message = string.Format("Failed {0} - {1}", args.FilePath, ex != null ? ex.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = message });
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
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = message });
			};
			results.Completed += (object sender, TransferFileProgressArgs args) =>
			{
				Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(restore, args.FilePath);
				restoredFile.TransferStatus = TransferStatus.COMPLETED;
				restoredFile.UpdatedAt = DateTime.UtcNow;
				daoRestoredFile.Update(restoredFile);

				var message = string.Format("Completed {0}", args.FilePath);
				Info(message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = message });
			};
			results.Started += (object sender, TransferFileProgressArgs args) =>
			{
				Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(restore, args.FilePath);
				restoredFile.TransferStatus = TransferStatus.RUNNING;
				restoredFile.UpdatedAt = DateTime.UtcNow;
				daoRestoredFile.Update(restoredFile);

				var message = string.Format("Started {0}", args.FilePath);
				//Info(message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = message });
			};
			results.Progress += (object sender, TransferFileProgressArgs args) =>
			{
				//var message = string.Format("Progress {0}% {1} ({2}/{3} bytes)",
				//	args.PercentDone, args.FilePath, args.TransferredBytes, args.TotalBytes);
				//Info(message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Updated, Message = null });
			};
		}

		protected abstract Task<LinkedList<CustomVersionedFile>> GetFilesToProcess(Models.Restore restore);

		protected async void DoRestore(CustomRestoreAgent agent, Models.Restore restore, RestoreOperationOptions options)
		{
			OnStart(agent, restore);

			//
			// Scanning
			//

			LinkedList<CustomVersionedFile> filesToProcess = null;
			{
				Task<LinkedList<CustomVersionedFile>> filesToProcessTask = GetFilesToProcess(restore);

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
					Debug.WriteLine("Exception Message: " + ex.Message);
				}

				if (filesToProcessTask.IsFaulted || filesToProcessTask.IsCanceled)
				{
					OnFailure(agent, restore, filesToProcessTask.Exception);
					return;
				}

				filesToProcess = filesToProcessTask.Result;

				{
					var message = string.Format("Scanning files finished.");
					Info(message);
					//StatusInfo.Update(BackupStatusLevel.INFO, message);
					OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.ScanningFilesFinished, Message = message });
				}
			}

			agent.Files = filesToProcess;

			{
				var message = string.Format("Estimate restore size: {0} files, {1}",
					agent.Files.Count(), FileSizeUtils.FileSizeToString(agent.EstimatedTransferSize));
				Info(message);
			}

			//
			// Transfer
			//

			{
				Task transferTask = agent.Start();
				await transferTask;
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

		public void OnFailure(CustomRestoreAgent agent, Models.Restore restore, Exception exception)
		{
			IsRunning = false;

			var message = string.Format("Restore failed: {0}", exception.Message);
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
					RestoreAgent = null;

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
