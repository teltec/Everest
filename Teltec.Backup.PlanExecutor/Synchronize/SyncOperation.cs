using Amazon.Runtime;
using NHibernate;
using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.Data.DAO.NH;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;
using Teltec.Stats;
using Teltec.Storage;
using Teltec.Storage.Backend;
using Teltec.Storage.Implementations.S3;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Synchronize
{
	public enum SyncOperationStatus : byte
	{
		Unknown = 0,
		Started = 1,
		//Resumed = 2,
		ListingUpdated = 3,
		SavingUpdated = 4,
		Canceled = 5,
		Failed = 6,
		Finished = 7,
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
		protected List<ListingObject> RemoteObjects;

		public string RemoteRootDirectory
		{
			get { return TransferAgent != null ? TransferAgent.RemoteRootDir : null; }
		}

		public string LocalRootDirectory
		{
			get { return TransferAgent != null ? TransferAgent.LocalRootDir : null; }
		}

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
			if (SyncAgent != null)
				SyncAgent.Dispose();

			//
			// Setup agents.
			//
			AWSCredentials awsCredentials = new BasicAWSCredentials(s3account.AccessKey, s3account.SecretKey);
			TransferAgentOptions options = new TransferAgentOptions
			{
				UploadChunkSizeInBytes = Teltec.Backup.Settings.Properties.Current.UploadChunkSize * 1024 * 1024,
			};
			TransferAgent = new S3TransferAgent(options, awsCredentials, s3account.BucketName, CancellationTokenSource.Token);
			TransferAgent.RemoteRootDir = TransferAgent.PathBuilder.CombineRemotePath("TELTEC_BKP", s3account.Hostname);

			RemoteObjects = new List<ListingObject>(4096); // Avoid small resizes without compromising memory.
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
				RemoteObjects.AddRange(e.Objects);

				foreach (var obj in e.Objects)
				{
					SyncAgent.Results.Stats.FileCount += 1;
					SyncAgent.Results.Stats.TotalSize += obj.Size;

					var message = string.Format("Found {0}", obj.Key);
					Info(message);
					OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.ListingUpdated, Message = message });
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
				OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.ListingUpdated, Message = message });
			};
		}

		private bool ParseS3Key(string inKey, out Models.EntryType outType, out string outPath, out string outVersion)
		{
			outType = Models.EntryType.FILE_VERSION;
			S3PathBuilder builder = new S3PathBuilder();
			builder.LocalRootDirectory = this.LocalRootDirectory;
			builder.RemoteRootDirectory = this.RemoteRootDirectory;
			outPath = builder.BuildLocalPath(inKey, out outVersion);
			return true;
		}

		private void Save(CancellationToken CancellationToken)
		{
			ISession session = NHibernateHelper.GetSession();

			BatchProcessor batchProcessor = new BatchProcessor();
			StorageAccountRepository daoStorageAccount = new StorageAccountRepository(session);
			BackupPlanFileRepository daoBackupPlanFile = new BackupPlanFileRepository(session);
			BackupPlanPathNodeRepository daoBackupPlanPathNode = new BackupPlanPathNodeRepository(session);
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository(session);

			BlockPerfStats stats = new BlockPerfStats();

			using (ITransaction tx = session.BeginTransaction())
			{
				try
				{
					// ------------------------------------------------------------------------------------

					Models.StorageAccount account = daoStorageAccount.Get(Synchronization.StorageAccount.Id);

					// ------------------------------------------------------------------------------------

					stats.Begin("STEP 1");

					BackupPlanPathNodeCreator pathNodeCreator = new BackupPlanPathNodeCreator(daoBackupPlanPathNode, tx);

					// Report save progress
					ReportSaveProgress(SyncAgent.Results.Stats.SavedFileCount, true);

					// Saving loop
					foreach (var obj in RemoteObjects)
					{
						// Throw if the operation was canceled.
						CancellationToken.ThrowIfCancellationRequested();

						Models.EntryType type;
						string path = string.Empty;
						string versionString = string.Empty;

						// Parse obj.Key into its relevant parts.
						bool ok = ParseS3Key(obj.Key, out type, out path, out versionString);

						DateTime lastWrittenAt = DateTime.ParseExact(versionString, Models.BackupedFile.VersionFormat, CultureInfo.InvariantCulture);

						// Create/Update BackupPlanFile, but do not SAVE it.
						Models.BackupPlanFile entry = daoBackupPlanFile.GetByStorageAccountAndPath(account, path);
						Models.BackupedFile version = null;

						if (entry == null)
						{
							// Create `BackupPlanFile`.
							entry = new Models.BackupPlanFile();
							entry.BackupPlan = null;
							entry.StorageAccountType = account.Type;
							entry.StorageAccount = account;
							entry.Path = path;
							entry.LastSize = obj.Size;
							entry.LastWrittenAt = lastWrittenAt;
							//entry.LastChecksum = ;
							entry.LastStatus = Models.BackupFileStatus.UNCHANGED;
							entry.CreatedAt = DateTime.UtcNow;

							// Create `BackupedFile`.
							version = new Models.BackupedFile(null, entry);
							version.StorageAccountType = account.Type;
							version.StorageAccount = account;
							version.FileLastWrittenAt = lastWrittenAt;
							version.FileLastChecksum = entry.LastChecksum;
							version.FileSize = entry.LastSize;
							version.FileStatus = Models.BackupFileStatus.MODIFIED;
							version.TransferStatus = TransferStatus.COMPLETED;
							version.UpdatedAt = DateTime.UtcNow;

							entry.Versions.Add(version);
							//daoBackupedFile.Insert(tx, version);
						}
						else
						{
							// Update `BackupPlanFile`.
							entry.LastSize = obj.Size;
							entry.LastWrittenAt = lastWrittenAt;
							//entry.LastChecksum =
							//entry.LastStatus = Models.BackupFileStatus.MODIFIED;
							entry.UpdatedAt = DateTime.UtcNow;

							IList<Models.BackupedFile> versions = null;
							try
							{
								versions = daoBackupedFile.GetCompletedByStorageAccountAndPath(account, path, versionString);
							}
							catch (FormatException ex)
							{
								logger.Log(LogLevel.Error, ex, "Invalid date format?");
								continue; // TODO(jweyrich): Should we abort?
							}

							// Check whether our database already contains this exact file + version.
							if (versions != null && versions.Count == 0)
							{
								// Create `BackupedFile`.
								version = new Models.BackupedFile(null, entry);
								version.StorageAccountType = account.Type;
								version.StorageAccount = account;
								version.FileLastWrittenAt = entry.LastWrittenAt;
								version.FileLastChecksum = entry.LastChecksum;
								version.FileSize = entry.LastSize;
								version.FileStatus = Models.BackupFileStatus.MODIFIED;
								version.TransferStatus = TransferStatus.COMPLETED;
								version.UpdatedAt = DateTime.UtcNow;

								entry.Versions.Add(version);
								//daoBackupedFile.Insert(tx, version);
							}
							else
							{
								// Update `BackupedFile`.
								version = versions.First();
								version.FileLastWrittenAt = entry.LastWrittenAt;
								version.FileLastChecksum = entry.LastChecksum;
								version.FileSize = entry.LastSize;
								version.UpdatedAt = DateTime.UtcNow;
								//daoBackupedFile.Update(tx, version);
							}
						}

						// Create path nodes and INSERT them, if they don't exist yet.
						entry.PathNode = pathNodeCreator.CreateOrUpdatePathNodes(account, entry);

						// Create or update `BackupPlanFile`.
						daoBackupPlanFile.InsertOrUpdate(tx, entry);

						SyncAgent.Results.Stats.SavedFileCount += 1;

						bool didFlush = batchProcessor.ProcessBatch(session);

						// Report save progress
						ReportSaveProgress(SyncAgent.Results.Stats.SavedFileCount);
					}

					batchProcessor.ProcessBatch(session, true);

					// Report save progress
					ReportSaveProgress(SyncAgent.Results.Stats.SavedFileCount, true);

					stats.End();

					// ------------------------------------------------------------------------------------

					tx.Commit();
				}
				catch (OperationCanceledException)
				{
					tx.Rollback(); // Rollback the transaction
					throw;
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, ex, "Caught exception");
					tx.Rollback(); // Rollback the transaction
					throw;
				}
				finally
				{
					//session.Close();
					if (session.IsConnected)
						session.Disconnect();
				}
			}
		}

		private readonly int ReportSaveBatchSize = 5;

		private void ReportSaveProgress(int totalSaved, bool force = false)
		{
			if ((totalSaved % ReportSaveBatchSize == 0) || force)
			{
				var message = string.Format("Saved {0} more files", totalSaved);
				//var message = string.Format("Saved {0} @ {1}", entry.Path, version.VersionName);
				Info(message);
				OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.SavingUpdated, Message = message });
			}
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
					if (ex.IsCancellation())
					{
						logger.Warn("Synchronizing files was canceled.");
					}
					else
					{
						logger.Log(LogLevel.Error, ex, "Caught exception during synchronizing files");
					}

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
					//OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Finished, Message = message });
				}

				{
					var message = string.Format("Estimated synchronization size: {0} files, {1}",
						RemoteObjects.Count(), FileSizeUtils.FileSizeToString(agent.Results.Stats.TotalSize));
					Info(message);
				}
			}

			//
			// Database files saving
			//

			{
				Task saveTask = ExecuteOnBackround(() =>
					{
						// Save everything.
						Save(CancellationTokenSource.Token);
					}, CancellationTokenSource.Token);

				{
					var	message = string.Format("Database files saving started.");
					Info(message);
				}

				try
				{
					await saveTask;
				}
				catch (Exception ex)
				{
					if (ex.IsCancellation())
					{
						logger.Warn("Database files saving was canceled.");
					}
					else
					{
						logger.Log(LogLevel.Error, ex, "Caught exception during database files saving");
					}

					if (saveTask.IsFaulted || saveTask.IsCanceled)
					{
						if (saveTask.IsCanceled)
							OnCancelation(agent, sync, ex); // saveTask.Exception
						else
							OnFailure(agent, sync, ex); // saveTask.Exception
						return;
					}
				}

				{
					var message = string.Format("Database files saving finished.");
					Info(message);
				}
			}

			OnFinish(agent, sync);
		}

		private Task ExecuteOnBackround(Action action, CancellationToken token)
		{
			return Task.Run(action, token);
			//return AsyncHelper.ExecuteOnBackround(action, token);
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

			// TODO(jweyrich): Handle overall failure and cancelation during Sync?
			sync.DidComplete();
			_daoSynchronization.Update(sync);
			OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Finished, Message = message });

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
					if (SyncAgent != null)
					{
						SyncAgent.Dispose();
						SyncAgent = null;
					}
				}
				this._isDisposed = true;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
