using Amazon.Runtime;
using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teltec.Backup.Data.DAO;
using Teltec.Common.Utils;
using Teltec.Storage;
using Teltec.Storage.Agent;
using Teltec.Storage.Implementations.S3;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Synchronize
{
	public enum SyncOperationStatus : byte
	{
		Unknown = 0,
		Started = 1,
		Updated = 2,
		Canceled = 3,
		Failed = 4,
		Finished = 5,
	}

	public static class Extensions
	{
		public static bool IsEnded(this SyncOperationStatus status)
		{
			return status == SyncOperationStatus.Canceled
				|| status == SyncOperationStatus.Failed
				|| status == SyncOperationStatus.Finished;
		}
	}

	public sealed class SyncOperationEvent : EventArgs
	{
		public SyncOperationStatus Status;
		public string Message;
	}

	public sealed class SyncOperationOptions
	{
		// ...
	}

	public abstract class SyncOperation : BaseOperation<SyncResults>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected readonly SynchronizationRepository _daoSynchronization = new SynchronizationRepository();

		protected Models.Synchronization Synchronization;

		#region Properties

		public override Int32? OperationId
		{
			get { return Synchronization.Id; }
		}

		public delegate void UpdateEventHandler(object sender, SyncOperationEvent e);
		public event UpdateEventHandler Updated;

		public DateTime? StartedAt
		{
			get { Assert.IsNotNull(Synchronization); return Synchronization.StartedAt; }
		}

		public DateTime? FinishedAt
		{
			get { Assert.IsNotNull(Synchronization); return Synchronization.FinishedAt; }
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

		public SyncOperation(SyncOperationOptions options)
		{
			Options = options;
		}

		#endregion

		#region Transfer

		protected SyncOperationOptions Options;
		protected CustomSynchronizationAgent SyncAgent;
		protected List<Models.SynchronizationFile> Files;

		public override void Start(out SyncResults results)
		{
			Assert.IsFalse(IsRunning);
			Assert.IsNotNull(Synchronization);
			Assert.IsNotNull(Synchronization.StorageAccount);
			Assert.AreEqual(Models.EStorageAccountType.AmazonS3, Synchronization.StorageAccountType);

			AmazonS3AccountRepository dao = new AmazonS3AccountRepository();
			Models.AmazonS3Account s3account = dao.GetForReadOnly(Synchronization.StorageAccount.Id);

			//
			// Dispose and recycle previous objects, if needed.
			//
			if (TransferAgent != null)
				TransferAgent.Dispose();
			if (TransferListControl != null)
				TransferListControl.ClearTransfers();
			if (Files != null)
				Files = new List<Models.SynchronizationFile>(4096); // Avoid small resizes without compromising memory.

			//
			// Setup agents.
			//
			AWSCredentials awsCredentials = new BasicAWSCredentials(s3account.AccessKey, s3account.SecretKey);
			TransferAgent = new S3AsyncTransferAgent(awsCredentials, s3account.BucketName);
			TransferAgent.RemoteRootDir = TransferAgent.PathBuilder.CombineRemotePath("TELTEC_BKP", s3account.Hostname);

			SyncAgent = new CustomSynchronizationAgent(TransferAgent);

			results = SyncAgent.Results;

			RegisterEventHandlers(Synchronization);

			//
			// Start the sync.
			//
			DoSynchronization(SyncAgent, Synchronization, Options);
		}

		protected void RegisterEventHandlers(Models.Synchronization sync)
		{
			SynchronizationFileRepository daoSynchronizationFile = new SynchronizationFileRepository();

			TransferAgent.ListingStarted += (object sender, ListingProgressArgs e) =>
			{
				// ...
			};
			TransferAgent.ListingCompleted += (object sender, ListingProgressArgs e) =>
			{
				// ...
			};
			TransferAgent.ListingProgress += (object sender, ListingProgressArgs e) =>
			{
				foreach (var obj in e.Objects)
				{
					Models.SynchronizationFile syncFile = daoSynchronizationFile.GetByURL(obj.Key);
					if (syncFile.Id.HasValue)
					{
						syncFile.Synchronization = sync;
						syncFile.FileSize = obj.Size;
						syncFile.LastWrittenAt = obj.LastModified;
						syncFile.UpdatedAt = DateTime.UtcNow;
					}
					else
					{
						syncFile = new Models.SynchronizationFile();
						syncFile.Synchronization = sync;
						syncFile.Path = obj.Key;
						syncFile.FileSize = obj.Size;
						syncFile.LastWrittenAt = obj.LastModified;
						syncFile.CreatedAt = DateTime.UtcNow;
					}

					SyncAgent.Results.Stats.FileCount += 1;
					SyncAgent.Results.Stats.TotalSize += syncFile.FileSize;

					Files.Add(syncFile);

					var message = string.Format("Found {0}", obj.Key);
					Info(message);
					OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Updated, Message = message });
				}
			};
			TransferAgent.ListingCanceled += (object sender, ListingProgressArgs e, Exception ex) =>
			{
				throw new NotImplementedException();
			};
			TransferAgent.ListingFailed += (object sender, ListingProgressArgs e, Exception ex) =>
			{
				var message = string.Format("Failed: {0}", ex != null ? ex.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(SyncStatusLevel.ERROR, message);
				OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Updated, Message = message });
			};
		}

		protected async void DoSynchronization(CustomSynchronizationAgent agent, Models.Synchronization sync, SyncOperationOptions options)
		{
			OnStart(agent, sync);

			//
			// Synchronization
			//

			{
				Task syncTask = agent.Start(TransferAgent.RemoteRootDir, true);

				{
					var message = string.Format("Synchronizing files started.");
					Info(message);
					//StatusInfo.Update(SyncStatusLevel.INFO, message);
					OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Started, Message = message });
				}

				try
				{
					await syncTask;
				}
				catch (Exception ex)
				{
					logger.ErrorException("Caught Exception", ex);

					if (syncTask.IsFaulted || syncTask.IsCanceled)
					{
						if (syncTask.IsCanceled)
							OnCancelation(agent, sync, ex); // syncTask.Exception
						else
							OnFailure(agent, sync, ex); // syncTask.Exception
						return;
					}
				}

				{
					var message = string.Format("Synchronizing files finished.");
					Info(message);
					//StatusInfo.Update(SyncStatusLevel.INFO, message);
					OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Finished, Message = message });
				}

				{
					var message = string.Format("Estimated synchronization size: {0} files, {1}",
						Files.Count(), FileSizeUtils.FileSizeToString(agent.Results.Stats.TotalSize));
					Info(message);
				}
			}

			OnFinish(agent, sync);
		}

		#endregion

		#region Event handlers

		public virtual void OnStart(CustomSynchronizationAgent agent, Models.Synchronization sync)
		{
			IsRunning = true;

			sync.DidStart();
		}

		protected void OnUpdate(SyncOperationEvent e)
		{
			if (Updated != null)
				Updated(this, e);
		}

		public void OnCancelation(CustomSynchronizationAgent agent, Models.Synchronization sync, Exception exception)
		{
			IsRunning = false;

			var message = string.Format("Synchronization canceled: {0}", exception != null ? exception.Message : "Exception not informed");
			Error(message);
			//StatusInfo.Update(SyncStatusLevel.ERROR, message);

			sync.WasCanceled();
			_daoSynchronization.Update(sync);

			OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Canceled, Message = message });
		}

		public void OnFailure(CustomSynchronizationAgent agent, Models.Synchronization sync, Exception exception)
		{
			IsRunning = false;

			var message = string.Format("Synchronization failed: {0}", exception != null ? exception.Message : "Exception not informed");
			Error(message);
			//StatusInfo.Update(SyncStatusLevel.ERROR, message);

			sync.DidFail();
			_daoSynchronization.Update(sync);

			OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Failed, Message = message });
		}

		public void OnFinish(CustomSynchronizationAgent agent, Models.Synchronization sync)
		{
			IsRunning = false;

			SyncResults.Statistics stats = agent.Results.Stats;

			var message = string.Format("Synchronization finished! Stats: {0} files", stats.FileCount);
			Info(message);
			//StatusInfo.Update(SyncStatusLevel.OK, message);
/*
			switch (agent.Results.OverallStatus)
			//switch (sync.Status)
			{
				default: throw new InvalidOperationException("Unexpected TransferStatus");
				case TransferStatus.CANCELED:
					sync.WasCanceled();
					_daoSynchronization.Update(sync);
					OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Canceled, Message = message });
					break;
				case TransferStatus.FAILED:
					sync.DidFail();
					_daoSynchronization.Update(sync);
					OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Failed, Message = message });
					break;
				case TransferStatus.COMPLETED:
					sync.DidComplete();
					_daoSynchronization.Update(sync);
					OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Finished, Message = message });
					break;
			}
*/
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
					SyncAgent = null;
				}
				this._isDisposed = true;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
