/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

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
using Teltec.Common.Extensions;
using Teltec.Common.Utils;
using Teltec.Everest.Data.DAO;
using Teltec.Everest.Data.DAO.NH;
using Teltec.Everest.PlanExecutor.Serialization;
using Teltec.Stats;
using Teltec.Storage;
using Teltec.Storage.Backend;
using Teltec.Storage.Implementations.S3;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.PlanExecutor.Synchronize
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

	public enum SyncOperationState
	{
		UNKNOWN = 0,
		STARTING,
		SYNCHRONIZING_FILES,
		SAVING_TO_DATABASE,
		FINISHING,
	}

	public abstract class SyncOperation : BaseOperation<SyncOperationReport>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected readonly SynchronizationRepository _daoSynchronization = new SynchronizationRepository();

		protected Models.Synchronization Synchronization;
		protected SyncOperationState CurrentState = SyncOperationState.UNKNOWN;

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

		#region Report

		// ...

		#endregion

		#region Transfer

		protected SyncOperationOptions Options;
		protected CustomSynchronizationAgent SyncAgent; // IDisposable
		protected List<ListingObject> RemoteObjects;

		public string RemoteRootDirectory
		{
			get { return TransferAgent != null ? TransferAgent.RemoteRootDir : null; }
		}

		public string LocalRootDirectory
		{
			get { return TransferAgent != null ? TransferAgent.LocalRootDir : null; }
		}

		public override void Cancel()
		{
			CancellationTokenSource.Cancel();
		}

		public override void Start()
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
				UploadChunkSizeInBytes = Teltec.Everest.Settings.Properties.Current.UploadChunkSize * 1024 * 1024,
			};
			TransferAgent = new S3TransferAgent(options, awsCredentials, s3account.BucketName, CancellationTokenSource.Token);
			TransferAgent.RemoteRootDir = TransferAgent.PathBuilder.CombineRemotePath("TELTEC_BKP", s3account.Hostname);

			RemoteObjects = new List<ListingObject>(4096); // Avoid small resizes without compromising memory.
			SyncAgent = new CustomSynchronizationAgent(TransferAgent);

			RegisterEventHandlers(Synchronization);

			Report.PlanType = "synchronization";
			Report.PlanName = "No plan";
			Report.BucketName = s3account.BucketName;
			Report.HostName = Synchronization.StorageAccount.Hostname;
			Report.SyncResults = SyncAgent.Results;

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

#if DEBUG
				foreach (var obj in e.Objects)
					Info("Found {0}", obj.Key);
#endif

				int filesCount = e.Objects.Count;
				long filesSize = e.Objects.Sum(x => x.Size);

				SyncAgent.Results.Stats.FileCount += filesCount;
				SyncAgent.Results.Stats.TotalSize += filesSize;

				var message = string.Format("Found {0} files ({1} bytes)", filesCount, filesSize);
				OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.ListingUpdated, Message = message });
			};
			TransferAgent.ListingCanceled += (object sender, ListingProgressArgs e) =>
			{
				var message = string.Format("Canceled: {0}", e.Exception != null ? e.Exception.Message : "Unknown reason");
				Info(message);
				//StatusInfo.Update(SyncStatusLevel.INFO, message);
				OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.ListingUpdated, Message = message });
			};
			TransferAgent.ListingFailed += (object sender, ListingProgressArgs e) =>
			{
				var message = string.Format("Failed: {0}", e.Exception != null ? e.Exception.Message : "Unknown reason");
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

		// Summary:
		//    Saves all instances from RemoteObjects list to the database.
		//    Also removes them from RemoteObjects list to free memory.
		private void Save(CancellationToken CancellationToken)
		{
			ISession session = NHibernateHelper.GetSession();

			BatchProcessor batchProcessor = new BatchProcessor(250);
			StorageAccountRepository daoStorageAccount = new StorageAccountRepository(session);
			BackupPlanFileRepository daoBackupPlanFile = new BackupPlanFileRepository(session);
			BackupPlanPathNodeRepository daoBackupPlanPathNode = new BackupPlanPathNodeRepository(session);
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository(session);

			BlockPerfStats stats = new BlockPerfStats();

			using (BatchTransaction tx = batchProcessor.BeginTransaction(session))
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
					for (int i = RemoteObjects.Count - 1; i >= 0; i--)
					{
						ListingObject obj = RemoteObjects[i]; // Get instance of object.
						//RemoteObjects[i] = null;
						RemoteObjects.RemoveAt(i); // Remove to free memory. RemoveAt(int) is O(N).

						// Throw if the operation was canceled.
						CancellationToken.ThrowIfCancellationRequested();

						Models.EntryType type;
						string path = string.Empty;
						string versionString = string.Empty;

						try
						{
							// Parse obj.Key into its relevant parts.
							bool ok = ParseS3Key(obj.Key, out type, out path, out versionString);
						}
						catch (Exception ex)
						{
							if (ex is ArgumentException || ex is IndexOutOfRangeException)
							{
								// Report error.
								logger.Warn("Failed to parse S3 key: {0} -- Skipping.", obj.Key);
								//logger.Log(LogLevel.Warn, ex, "Failed to parse S3 key: {0}", obj.Key);

								//SyncAgent.Results.Stats.FailedSavedFileCount += 1;

								// Report save progress
								//ReportSaveProgress(SyncAgent.Results.Stats.SavedFileCount);

								continue; // Skip this file.
							}

							throw;
						}

						path = StringUtils.NormalizeUsingPreferredForm(path);

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
							version = new Models.BackupedFile(null, entry, Synchronization);
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
							catch (FormatException)
							{
								// Report error.
								logger.Warn("Failed to parse versionString: {0} -- Skipping.", versionString);

								//SyncAgent.Results.Stats.FailedSavedFileCount += 1;

								continue; // TODO(jweyrich): Should we abort?
							}

							// Check whether our database already contains this exact file + version.
							if (versions == null || (versions != null && versions.Count == 0))
							{
								// Create `BackupedFile`.
								version = new Models.BackupedFile(null, entry, Synchronization);
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

						try
						{
							// Create path nodes and INSERT them, if they don't exist yet.
							entry.PathNode = pathNodeCreator.CreateOrUpdatePathNodes(account, entry);

							// Create or update `BackupPlanFile`.
							daoBackupPlanFile.InsertOrUpdate(tx, entry);
						}
						catch (Exception ex)
						{
							logger.Log(LogLevel.Error, ex, "BUG: Failed to insert/update {0} => {1}",
									typeof(Models.BackupPlanFile).Name,
									CustomJsonSerializer.SerializeObject(entry, 1));

							logger.Error("Dump of failed object: {0}", entry.DumpMe());
							throw;
						}

						bool didCommit = batchProcessor.ProcessBatch(tx);

						SyncAgent.Results.Stats.SavedFileCount += 1;

						// Report save progress
						ReportSaveProgress(SyncAgent.Results.Stats.SavedFileCount);
					}

					batchProcessor.ProcessBatch(tx, true);

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

		private readonly int ReportSaveBatchSize = 50;

		private void ReportSaveProgress(int totalSaved, bool force = false)
		{
			if ((totalSaved % ReportSaveBatchSize == 0) || force)
			{
				var message = string.Format("Saved {0} files", totalSaved);
				//var message = string.Format("Saved {0} @ {1}", entry.Path, version.VersionName);
#if DEBUG
				Info(message);
#endif
				OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.SavingUpdated, Message = message });
			}
		}

		protected async void DoSynchronization(CustomSynchronizationAgent agent, Models.Synchronization sync, SyncOperationOptions options)
		{
			try
			{
				CurrentState = SyncOperationState.STARTING;
				OnStart(agent, sync);

				//
				// Synchronization
				//

				CurrentState = SyncOperationState.SYNCHRONIZING_FILES;
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

				CurrentState = SyncOperationState.SAVING_TO_DATABASE;
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

				CurrentState = SyncOperationState.FINISHING;
				OnFinish(agent, sync);
			}
			catch (Exception ex)
			{
				OnFinish(agent, sync, ex);
			}
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

		public void OnFinish(CustomSynchronizationAgent agent, Models.Synchronization sync, Exception ex = null)
		{
			IsRunning = false;

			switch (CurrentState)
			{
				default:
					{
						var message = string.Format("Synchronization failed: {0}", ex.Message);
						Warn(message);
						//StatusInfo.Update(SyncStatusLevel.WARN, message);
						Report.AddErrorMessage(ex.Message);

						Report.OperationStatus = OperationStatus.FAILED;
						sync.DidFail();
						_daoSynchronization.Update(sync);
						OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Failed, Message = message });

						break;
					}
				case SyncOperationState.FINISHING:
					{
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
								Report.OperationStatus = OperationStatus.CANCELED;
								sync.WasCanceled();
								_daoSynchronization.Update(sync);
								OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Canceled, Message = message });
								break;
							case TransferStatus.FAILED:
								Report.OperationStatus = OperationStatus.FAILED;
								sync.DidFail();
								_daoSynchronization.Update(sync);
								OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Failed, Message = message });
								break;
							case TransferStatus.COMPLETED:
								Report.OperationStatus = OperationStatus.COMPLETED;
								sync.DidComplete();
								_daoSynchronization.Update(sync);
								OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Finished, Message = message });
								break;
						}
						*/

						break;
					}
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
					if (SyncAgent != null)
					{
						logger.Info("DISPOSING SyncAgent.");

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
