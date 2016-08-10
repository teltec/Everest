using NHibernate;
using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.Data.DAO.NH;
using Teltec.Backup.Data.Versioning;
using Teltec.Backup.PlanExecutor.Serialization;
using Teltec.Common.Extensions;
using Teltec.Common.Utils;
using Teltec.FileSystem;
using Teltec.Stats;
using Teltec.Storage;
using Teltec.Storage.Versioning;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Versioning
{
	public sealed class IncrementalFileVersioner : IDisposable
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private CancellationToken CancellationToken;
		public FileVersionerResults Results { get; private set; }

		public IncrementalFileVersioner(CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
			Results = new FileVersionerResults();
		}

		public async Task<FileVersionerResults> NewVersion(Models.Backup backup, LinkedList<string> files)
		{
			return await DoVersion(backup, files, true);
		}

		public async Task<FileVersionerResults> ResumeVersion(Models.Backup backup, LinkedList<string> files)
		{
			return await DoVersion(backup, files, false);
		}

		public async Task<FileVersionerResults> DoVersion(Models.Backup backup, LinkedList<string> filePaths, bool newVersion)
		{
			Assert.IsNotNull(backup);
			Assert.AreEqual(TransferStatus.RUNNING, backup.Status);
			Assert.IsNotNull(filePaths);

			Results.Reset();

			await ExecuteOnBackround(() =>
			{
				ISession session = NHibernateHelper.GetSession();
				try
				{
					BackupRepository daoBackup = new BackupRepository(session);
					BackupPlanFileRepository daoBackupPlanFile = new BackupPlanFileRepository(session);

					Backup = daoBackup.Get(backup.Id);

					IList<Models.BackupPlanFile> list = newVersion
						? daoBackupPlanFile.GetAllByBackupPlan(Backup.BackupPlan)
						: daoBackupPlanFile.GetAllPendingByBackup(Backup);

					AllFilesFromPlan = list.ToDictionary<Models.BackupPlanFile, string>(p => p.Path);

					Execute(Backup, filePaths, newVersion);

					Save(session);
				}
				catch (Exception ex)
				{
					string message = string.Format("File versioning FAILED with an exception: {0}", ex.Message);

					Results.OnError(this, message);
					logger.Log(LogLevel.Error, ex, message);

					throw ex;
				}
				finally
				{
					//session.Close();
					if (session.IsConnected)
						session.Disconnect();
				}
			}, CancellationToken);

			return Results;
		}

		private Task ExecuteOnBackround(Action action, CancellationToken token)
		{
			return Task.Run(action, token);
			//return AsyncHelper.ExecuteOnBackround(action, token);
		}

		#region File change detection

		private readonly BigInteger MAX_FILESIZE_TO_HASH = 10 * BigInteger.Pow(2, 20); // 10 MB
		private readonly HashAlgorithm HashAlgo = new SHA1CryptoServiceProvider(); // SHA-1 is 160 bits long (20 bytes)

		private byte[] CalculateHashForFile(string filePath)
		{
			Microsoft.Win32.SafeHandles.SafeFileHandle handle = ZetaLongPaths.ZlpIOHelper.CreateFileHandle(
				filePath,
				ZetaLongPaths.Native.CreationDisposition.OpenExisting,
				ZetaLongPaths.Native.FileAccess.GenericRead,
				ZetaLongPaths.Native.FileShare.Read);

			using (Stream inputStream = new FileStream(handle, FileAccess.Read))
			{
				return HashAlgo.ComputeHash(inputStream);
			}
		}

		private bool AreEqual(DateTime dt1, DateTime dt2)
		{
			if (object.ReferenceEquals(dt1, dt2)) // true if `dt1` is the same instance as `dt2` or if both are null; otherwise, false.
				return true;

			// Strip milliseconds off from both dates because we don't care - currently.
			dt1 = dt1.AddTicks(-(dt1.Ticks % TimeSpan.TicksPerSecond));
			dt2 = dt2.AddTicks(-(dt2.Ticks % TimeSpan.TicksPerSecond));

			return DateTime.Compare(dt1, dt2) == 0;
		}

		private bool AreEqual(byte[] h1, byte[] h2)
		{
			return
				object.ReferenceEquals(h1, h2) // true if `h1` is the same instance as `h2` or if both are null; otherwise, false.
				|| (h1 != null && h1.SequenceEqual(h2));
		}

		private bool IsFileModified(Models.BackupPlanFile file, Models.BackupedFile lastVersion)
		{
			bool didChange = !AreEqual(file.LastWrittenAt, lastVersion.FileLastWrittenAt);
			if (!didChange)
				return false; // If the last write dates are equal, we ASSUME it's not modified.

			// If one of the checksums doesn't exist, assume the file changed.
			//if (file.LastChecksum == null || lastVersion.FileLastChecksum == null)
			//	return true;

			didChange = !AreEqual(file.LastChecksum, lastVersion.FileLastChecksum);
			return didChange;
		}

		#endregion File change detection

		private bool IsSaved = false;
		private Models.Backup Backup;

		// Contains ALL `BackupPlanFile`s that were registered at least once for the plan associated to this backup.
		// Fact 1: ALL of its items are also contained (distributed) in:
		//		`ChangeSet.RemovedFiles`
		//		`ChangeSet.DeletedFiles`
		//		`SuppliedFiles`
		private Dictionary<string, Models.BackupPlanFile> AllFilesFromPlan;

		// Contains ALL `BackupPlanFile`s that were informed to be included in this backup.
		// Fact 1: ALL of its items are also contained in `AllFilesFromPlan`.
		// Fact 2: SOME of its items may also be contained in `ChangeSet.AddedFiles`.
		// Fact 3: SOME of its items may also be contained in `ChangeSet.ModifiedFiles`.
		// Fact 4: SOME of its items may also be contained in `ChangeSet.RemovedFiles`.
		// Fact 5: SOME of its items may also be contained in `ChangeSet.DeletedFiles`.
		// Fact 5: SOME of its items may also be contained in `ChangeSet.FailedFiles`.
		private LinkedList<Models.BackupPlanFile> SuppliedFiles;

		// Contains the relation of all `BackupPlanFile`s that will be listed on this backup,
		// be it an addition, deletion, modification, or removal.
		private ChangeSet<Models.BackupPlanFile> ChangeSet = new ChangeSet<Models.BackupPlanFile>();

		// After `Save()`, contains ALL `CustomVersionedFile`s that are eligible for transfer - those whose status is ADDED or MODIFIED.
		private TransferSet<CustomVersionedFile> TransferSet = new TransferSet<CustomVersionedFile>();

		public IEnumerable<CustomVersionedFile> FilesToTransfer
		{
			get
			{
				Assert.IsTrue(IsSaved);
				return TransferSet.Files;
			}
		}

		private void Execute(Models.Backup backup, LinkedList<string> filePaths, bool isNewVersion)
		{
			// The `filePaths` argument contains the filesystem paths informed by the user for this backup.

			//
			// NOTE: The methods call ORDER is important!
			//

			SuppliedFiles = DoLoadOrCreateBackupPlanFiles(backup.BackupPlan, filePaths);
			DoUpdateBackupPlanFilesStatus(SuppliedFiles, isNewVersion);

			ChangeSet.AddedFiles = GetAddedFiles(SuppliedFiles);
#if false
			foreach (var item in ChangeSet.AddedFiles)
				Console.WriteLine("BackupPlanAddedFiles: {0}", item.Path);
#endif
			ChangeSet.ModifiedFiles = GetModifiedFiles(SuppliedFiles);
#if false
			foreach (var item in changeset.modifiedfiles)
				console.writeline("backupplanmodifiedfiles: {0}", item.path);
#endif
			// DO NOT update files removal and deletion status for `ResumeBackupOperation`,
			// only for `NewBackupOperation`.
			if (isNewVersion)
			{
				ChangeSet.RemovedFiles = GetRemovedFiles(AllFilesFromPlan.Values);
#if false
				foreach (var item in ChangeSet.RemovedFiles)
					Console.WriteLine("BackupPlanRemovedFiles: {0}", item.Path);
#endif
				ChangeSet.DeletedFiles = GetDeletedFilesAndUpdateTheirStatus(SuppliedFiles);
			}

			//throw new Exception("Simulating failure.");
		}

		//
		// Loads or creates `BackupPlanFile`s for each file in `filePaths`.
		// Returns the complete list of `BackupPlanFile`s that are related to `filePaths`.
		// If a `BackupPlanFile` does not exist for a given filePath, one will be created.
		//
		// NOTE: This method does NOT change the database.
		//
		private LinkedList<Models.BackupPlanFile> DoLoadOrCreateBackupPlanFiles(Models.BackupPlan plan, LinkedList<string> filePaths)
		{
			Assert.IsNotNull(plan);
			Assert.IsNotNull(filePaths);
			Assert.IsNotNull(AllFilesFromPlan);

			BlockPerfStats stats = new BlockPerfStats();
			stats.Begin();

			Dictionary<string, Models.BackupPlanFile> processed = new Dictionary<string, Models.BackupPlanFile>();

			// Check all files.
			foreach (string path in filePaths)
			{
				// Throw if the operation was canceled.
				CancellationToken.ThrowIfCancellationRequested();

				string normalizedPath = StringUtils.NormalizeUsingPreferredForm(path);

				//
				// Create or update `BackupPlanFile`.
				//
				Models.BackupPlanFile backupPlanFile = null;
				// The complexity of Dictionary<TKey,TValue>.TryGetValue(TKey,TValue) approaches O(1)
				bool backupPlanFileAlreadyExists = AllFilesFromPlan.TryGetValue(normalizedPath, out backupPlanFile);

				if (!backupPlanFileAlreadyExists)
				{
					backupPlanFile = new Models.BackupPlanFile(plan, normalizedPath);
					backupPlanFile.CreatedAt = DateTime.UtcNow;
				}

				// This avoids duplicates in the list.
				// The complexity of setting Dictionary<TKey,TValue>[TKey] is amortized O(1)
				processed[normalizedPath] = backupPlanFile;
			}

			LinkedList<Models.BackupPlanFile> result =
				processed.ToLinkedList<Models.BackupPlanFile, KeyValuePair<string, Models.BackupPlanFile>>(p => p.Value);

			stats.End();

			return result;
		}

		//
		// Summary:
		// Returns true if:
		// - File transfer didn't begin;
		// - File transfer didn't complete;
		// - File transfer failed;
		// - File transfer was canceled;
		//
		private bool NeedsToRetryFile(Models.BackupPlanFile file, Models.BackupedFile lastVersion)
		{
			return lastVersion.TransferStatus != TransferStatus.COMPLETED
				&& lastVersion.TransferStatus != TransferStatus.PURGED;
		}

		//
		// Summary:
		// Update the `LastWrittenAt`,`LastSize`,`LastStatus`,`LastUpdatedAt`,`LastChecksum`
		// properties of each file in `files` according to the actual state of the file in the filesystem.
		//
		// NOTE: This function has a side effect - It updates properties of items from `files`.
		//
		private void DoUpdateBackupPlanFilesStatus(LinkedList<Models.BackupPlanFile> files, bool isNewVersion)
		{
			Assert.IsNotNull(files);

			ISession session = NHibernateHelper.GetSession();
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository(session);

			BlockPerfStats stats = new BlockPerfStats();
			stats.Begin();

			// Check all files.
			LinkedListNode<Models.BackupPlanFile> node = files.First;
			while (node != null)
			{
				var next = node.Next;
				Models.BackupPlanFile entry = node.Value;
				// TODO(jweyrich): Measure whether `daoBackupedFile.GetLatestVersion(entry)` is faster or not,
				//                 and whether "entry.Versions.anything" would cause all related version to be fetched.
#if false
				Models.BackupedFile lastVersion = entry.Versions != null && entry.Versions.Count > 0
					? entry.Versions.Last() : null;
#else
				// This may be a version that has not COMPLETED the transfer.
				Models.BackupedFile lastVersion = entry.Id.HasValue ? daoBackupedFile.GetLatestVersion(entry) : null;
#endif

				// Throw if the operation was canceled.
				CancellationToken.ThrowIfCancellationRequested();

				//
				// Check what happened to the file.
				//

				bool fileExistsOnFilesystem = FileManager.FileExists(entry.Path);
				Models.BackupFileStatus? changeStatusTo = null;

				try
				{
					//
					// Update file properties
					//
					if (fileExistsOnFilesystem)
					{
						try
						{
							DateTime fileLastWrittenAt = FileManager.UnsafeGetFileLastWriteTimeUtc(entry.Path);
							long fileLength = FileManager.UnsafeGetFileSize(entry.Path);

							entry.LastWrittenAt = fileLastWrittenAt;
							entry.LastSize = fileLength;
						}
						catch (Exception ex)
						{
							string message = string.Format("Caught an exception while retrieving file properties: {0}", ex.Message);

							Results.OnFileFailed(this, new FileVersionerEventArgs { FilePath = entry.Path, FileSize = 0 }, message);
							logger.Warn(message);

							throw ex;
						}

						try
						{
							// Skip files larger than `MAX_FILESIZE_TO_HASH`.
							int result = BigInteger.Compare(entry.LastSize, MAX_FILESIZE_TO_HASH);
							if (result < 0)
								entry.LastChecksum = CalculateHashForFile(entry.Path);
						}
						catch (Exception ex)
						{
							string message = string.Format("Caught an exception while calculating the file hash: {0}", ex.Message);

							Results.OnFileFailed(this, new FileVersionerEventArgs { FilePath = entry.Path, FileSize = 0 }, message);
							logger.Warn(message);

							throw ex;
						}

						Results.OnFileCompleted(this, new FileVersionerEventArgs { FilePath = entry.Path, FileSize = entry.LastSize });
					}

					//
					// Update file status
					//
					if (lastVersion != null) // File was backed up at least once in the past?
					{
						switch (entry.LastStatus)
						{
							case Models.BackupFileStatus.DELETED: // File was marked as DELETED by a previous backup?
								if (fileExistsOnFilesystem) // Exists?
									changeStatusTo = Models.BackupFileStatus.ADDED;
								break;

							case Models.BackupFileStatus.REMOVED: // File was marked as REMOVED by a previous backup?
								if (fileExistsOnFilesystem) // Exists?
									changeStatusTo = Models.BackupFileStatus.ADDED;
								else
									// QUESTION: Do we really care to transition REMOVED to DELETED?
									changeStatusTo = Models.BackupFileStatus.DELETED;
								break;

							default: // ADDED, MODIFIED, UNCHANGED
								if (fileExistsOnFilesystem) // Exists?
								{
									// DO NOT verify whether the file changed for a `ResumeBackupOperation`,
									// only for `NewBackupOperation`.
									if (isNewVersion)
									{
										if (IsFileModified(entry, lastVersion)) // Modified?
										{
											changeStatusTo = Models.BackupFileStatus.MODIFIED;
										}
										else if (NeedsToRetryFile(entry, lastVersion)) // Didn't complete last file transfer?
										{
											changeStatusTo = Models.BackupFileStatus.MODIFIED;
										}
										else // Not modified?
										{
											changeStatusTo = Models.BackupFileStatus.UNCHANGED;
										}
									}
								}
								else // Deleted from filesystem?
								{
									changeStatusTo = Models.BackupFileStatus.DELETED;
								}
								break;
						}
					}
					else // Adding to this backup?
					{
						if (fileExistsOnFilesystem) // Exists?
						{
							changeStatusTo = Models.BackupFileStatus.ADDED;
						}
						else
						{
							// Error? Can't add a non-existent file to the plan.
						}
					}

					if (changeStatusTo.HasValue)
					{
						entry.LastStatus = changeStatusTo.Value;
						entry.UpdatedAt = DateTime.UtcNow;
					}
				}
				catch (Exception ex)
				{
					FailedFile<Models.BackupPlanFile> failedEntry = new FailedFile<Models.BackupPlanFile>(entry, ex.Message, ex);
					ChangeSet.FailedFiles.AddLast(failedEntry);

					// Remove this entry from `files` as it clearly failed.
					files.Remove(node); // Complexity is O(1)
				}

				node = next;
			}

			stats.End();
		}

		//
		// Summary:
		// Return all files from `files` which are marked as `ADDED`;
		//
		// NOTE: This function has no side effects.
		//
		private IEnumerable<Models.BackupPlanFile> GetAddedFiles(IEnumerable<Models.BackupPlanFile> files)
		{
			Assert.IsNotNull(files);

			// Find all `BackupPlanFile`s from `files` that are marked as ADDED.
			return files.Where(p => p.LastStatus == Models.BackupFileStatus.ADDED);
		}

		//
		// Summary:
		// Return all files from `files` which are marked as `MODIFIED`;
		//
		// NOTE: This function has no side effects.
		//
		private IEnumerable<Models.BackupPlanFile> GetModifiedFiles(IEnumerable<Models.BackupPlanFile> files)
		{
			Assert.IsNotNull(files);

			// Find all `BackupPlanFile`s from `files` that are marked as MODIFIED.
			return files.Where(p => p.LastStatus == Models.BackupFileStatus.MODIFIED);
		}

		//
		// Summary:
		// Return all files from `files` which are marked as `REMOVED`;
		//
		// NOTE: This function has no side effects.
		//
		private IEnumerable<Models.BackupPlanFile> GetRemovedFiles(IEnumerable<Models.BackupPlanFile> files)
		{
			Assert.IsNotNull(files);

			// Find all `BackupPlanFile`s from `files` that are marked as REMOVED.
			return files.Where(p => p.LastStatus == Models.BackupFileStatus.REMOVED);
		}

		//
		// Summary:
		// 1. Find all files from `files` that are not marked as REMOVED or DELETED;
		// 2. Check which files from `AllFilesFromPlan` are not in the result of 1, meaning
		//    they have been deleted from the filesystem;
		// 3. Mark them as DELETED;
		// 4. Return all deleted files (all from 2);
		//
		// NOTE: This function has a side effect - It updates properties of items from `files`.
		//
		private IEnumerable<Models.BackupPlanFile> GetDeletedFilesAndUpdateTheirStatus(LinkedList<Models.BackupPlanFile> files)
		{
			Assert.IsNotNull(files);
			Assert.IsNotNull(AllFilesFromPlan);

			// 1. Find all files from `files` that are not marked as REMOVED or DELETED.
			IEnumerable<Models.BackupPlanFile> filesThatExist = files.Where(p => p.LastStatus != Models.BackupFileStatus.DELETED && p.LastStatus != Models.BackupFileStatus.REMOVED);
#if false
			foreach (var item in deletedFiles)
				Console.WriteLine("GetDeletedFilesAndUpdateTheirStatus: filesThatExist: {0}", item.Path);
#endif

			// 2. Check which files from `AllFilesFromPlan` are not in the result of 1, meaning
			//    they have been deleted from the filesystem.
			IEnumerable<Models.BackupPlanFile> deletedFilesToBeUpdated = AllFilesFromPlan.Values.Except(filesThatExist);
#if false
			foreach (var item in deletedFilesToBeUpdated)
				Console.WriteLine("GetDeletedFilesAndUpdateTheirStatus: deletedFilesToBeUpdated: {0}", item.Path);
#endif

			// 3. Mark them as DELETED;
			foreach (Models.BackupPlanFile entry in deletedFilesToBeUpdated)
			{
				// Throw if the operation was canceled.
				CancellationToken.ThrowIfCancellationRequested();

				entry.LastStatus = Models.BackupFileStatus.DELETED;
				entry.UpdatedAt = DateTime.UtcNow;
			}

			// 4. Return all deleted files;
			List<Models.BackupPlanFile> result = new List<Models.BackupPlanFile>(deletedFilesToBeUpdated);
#if false
			foreach (var item in result)
				Console.WriteLine("GetDeletedFilesAndUpdateTheirStatus: result: {0}", item.Path);
#endif

			return result;
		}

		//
		// Summary:
		// ...
		//
		private IEnumerable<CustomVersionedFile> GetFilesToTransfer(Models.Backup backup, LinkedList<Models.BackupPlanFile> files)
		{
			// Update files version.
			foreach (Models.BackupPlanFile entry in files)
			{
				// Throw if the operation was canceled.
				CancellationToken.ThrowIfCancellationRequested();

				switch (entry.LastStatus)
				{
					// Skip REMOVED, DELETED, and UNCHANGED files.
					default:
						break;

					case Models.BackupFileStatus.ADDED:
					case Models.BackupFileStatus.MODIFIED:
						{
							IFileVersion version = new FileVersion { Version = entry.LastWrittenAt.ToString(Models.BackupedFile.VersionFormat) };
							yield return new CustomVersionedFile
							{
								Path = entry.Path,
								Size = entry.LastSize,
								Checksum = null,
								//Checksum = entry.LastChecksum,
								Version = version,
								LastWriteTimeUtc = entry.LastWrittenAt,
							};
							break; // YES, it's required!
						}
				}
			}
		}

		public void Undo()
		{
			if (Backup != null && !IsSaved)
			{
				BackupRepository daoBackup = new BackupRepository();
				daoBackup.Refresh(Backup);
			}
		}

		//
		// Summary:
		// 1 - Split path into its components and INSERT new path nodes if they don't exist yet.
		// 2 - Insert/Update `BackupPlanFile`s as necessary.
		// 3 - Insert/Update `BackupedFile`s as necessary.
		// 4 - Update all `BackupPlanFile`s that already exist for the backup plan associated
		//     with this backup operation.
		// 5 - Insert/Update `Backup` and its `BackupedFile`s into the database, also saving
		//     the `BackupPlanFile`s instances that may have been changed by step 2.
		// 6 - Create versioned files and remove files that won't belong to this backup.
		//
		[MethodImpl(MethodImplOptions.NoInlining)]
		private void Save(ISession session)
		{
			Assert.IsFalse(IsSaved);

			BatchProcessor batchProcessor = new BatchProcessor();
			BackupRepository daoBackup = new BackupRepository(session);
			BackupPlanFileRepository daoBackupPlanFile = new BackupPlanFileRepository(session);
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository(session);
			BackupPlanPathNodeRepository daoBackupPlanPathNode = new BackupPlanPathNodeRepository(session);

#if false
			var FilesToTrack = SuppliedFiles.Union(ChangeSet.DeletedFiles);
			var FilesToInsertOrUpdate =
				from f in FilesToTrack
				where
					// Keep it so we'll later add or update a `BackupedFile`.
					((f.LastStatus == Models.BackupFileStatus.ADDED || f.LastStatus == Models.BackupFileStatus.MODIFIED))
					// Keep it if `LastStatus` is different from `PreviousLastStatus`.
					|| ((f.LastStatus == Models.BackupFileStatus.REMOVED || f.LastStatus == Models.BackupFileStatus.DELETED) && (f.LastStatus != f.PreviousLastStatus))
				// Skip all UNCHANGED files.
				select f;
#else
			var FilesToTrack = SuppliedFiles;
			var FilesToInsertOrUpdate =
				from f in FilesToTrack
				where
					// Keep it so we'll later add or update a `BackupedFile`.
					((f.LastStatus == Models.BackupFileStatus.ADDED || f.LastStatus == Models.BackupFileStatus.MODIFIED))
				// Skip all UNCHANGED/DELETED/REMOVED files.
				select f;
#endif

			BlockPerfStats stats = new BlockPerfStats();

			using (ITransaction tx = session.BeginTransaction())
			{
				try
				{
					// ------------------------------------------------------------------------------------

					stats.Begin("STEP 1");

					BackupPlanPathNodeCreator pathNodeCreator = new BackupPlanPathNodeCreator(daoBackupPlanPathNode, tx);

					// 1 - Split path into its components and INSERT new path nodes if they don't exist yet.
					foreach (Models.BackupPlanFile entry in FilesToInsertOrUpdate)
					{
						// Throw if the operation was canceled.
						CancellationToken.ThrowIfCancellationRequested();

						try
						{
							entry.PathNode = pathNodeCreator.CreateOrUpdatePathNodes(Backup.BackupPlan.StorageAccount, entry);
						}
						catch (Exception ex)
						{
							string message = string.Format("BUG: Failed to create/update {0} => {1}",
								typeof(Models.BackupPlanPathNode).Name,
								CustomJsonSerializer.SerializeObject(entry, 1));

							Results.OnError(this, message);
							logger.Log(LogLevel.Error, ex, message);

							throw ex;
						}

						batchProcessor.ProcessBatch(session);
					}

					batchProcessor.ProcessBatch(session, true);
					stats.End();

					// ------------------------------------------------------------------------------------

					stats.Begin("STEP 2");

					// 2 - Insert/Update `BackupPlanFile`s as necessary.
					foreach (Models.BackupPlanFile entry in FilesToInsertOrUpdate)
					{
						// Throw if the operation was canceled.
						CancellationToken.ThrowIfCancellationRequested();

						// IMPORTANT: It's important that we guarantee the referenced `BackupPlanFile` has a valid `Id`
						// before we reference it elsewhere, otherwise NHibernate won't have a valid value to put on
						// the `backup_plan_file_id` column.
						try
						{
							daoBackupPlanFile.InsertOrUpdate(tx, entry); // Guarantee it's saved
						}
						catch (Exception ex)
						{
							string message = string.Format("BUG: Failed to insert/update {0} => {1}",
								typeof(Models.BackupPlanFile).Name,
								CustomJsonSerializer.SerializeObject(entry, 1));

							Results.OnError(this, message);
							logger.Log(LogLevel.Error, ex, message);

							logger.Error("Dump of failed object: {0}", entry.DumpMe());
							throw ex;
						}

						batchProcessor.ProcessBatch(session);
					}

					batchProcessor.ProcessBatch(session, true);
					stats.End();

					// ------------------------------------------------------------------------------------

					stats.Begin("STEP 3");

					// 3 - Insert/Update `BackupedFile`s as necessary and add them to the `Backup`.
					//List<Models.BackupedFile> backupedFiles = new List<Models.BackupedFile>(FilesToInsertOrUpdate.Count());

					foreach (Models.BackupPlanFile entry in FilesToInsertOrUpdate)
					{
						// Throw if the operation was canceled.
						CancellationToken.ThrowIfCancellationRequested();

						Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(Backup, entry.Path);
						if (backupedFile == null) // If we're resuming, this should already exist.
						{
							// Create `BackupedFile`.
							backupedFile = new Models.BackupedFile(Backup, entry);
						}
						backupedFile.FileSize = entry.LastSize;
						backupedFile.FileStatus = entry.LastStatus;
						backupedFile.FileLastWrittenAt = entry.LastWrittenAt;
						backupedFile.FileLastChecksum = entry.LastChecksum;
						switch (entry.LastStatus)
						{
							default:
								backupedFile.TransferStatus = default(TransferStatus);
								break;

							case Models.BackupFileStatus.REMOVED:
							case Models.BackupFileStatus.DELETED:
								backupedFile.TransferStatus = TransferStatus.COMPLETED;
								break;
						}
						backupedFile.UpdatedAt = DateTime.UtcNow;

						try
						{
							daoBackupedFile.InsertOrUpdate(tx, backupedFile);
						}
						catch (Exception ex)
						{
							logger.Log(LogLevel.Error, ex, "BUG: Failed to insert/update {0} => {1}",
								typeof(Models.BackupedFile).Name,
								CustomJsonSerializer.SerializeObject(backupedFile, 1));
							throw ex;
						}

						//backupedFiles.Add(backupedFile);

						batchProcessor.ProcessBatch(session);
					}

					batchProcessor.ProcessBatch(session, true);
					stats.End();

					// ------------------------------------------------------------------------------------

					stats.Begin("STEP 4");

					// 4 - Update all `BackupPlanFile`s that already exist for the backup plan associated with this backup operation.
					{
						var AllFilesFromPlanThatWerentUpdatedYet = AllFilesFromPlan.Values.Except(FilesToInsertOrUpdate);
						foreach (Models.BackupPlanFile file in AllFilesFromPlanThatWerentUpdatedYet)
						{
							// Throw if the operation was canceled.
							CancellationToken.ThrowIfCancellationRequested();

							//Console.WriteLine("2: {0}", file.Path);
							try
							{
								daoBackupPlanFile.Update(tx, file);
							}
							catch (Exception ex)
							{
								string message = string.Format("BUG: Failed to update {0} => {1} ",
									typeof(Models.BackupPlanFile).Name,
									CustomJsonSerializer.SerializeObject(file, 1));

								Results.OnError(this, message);
								logger.Log(LogLevel.Error, ex, message);

								throw ex;
							}

							batchProcessor.ProcessBatch(session);
						}
					}

					batchProcessor.ProcessBatch(session, true);
					stats.End();

					// ------------------------------------------------------------------------------------

					stats.Begin("STEP 5");

					// 5 - Insert/Update `Backup` and its `BackupedFile`s into the database, also saving
					//     the `BackupPlanFile`s instances that may have been changed by step 2.
					{
						//foreach (var bf in backupedFiles)
						//{
						//	// Throw if the operation was canceled.
						//	CancellationToken.ThrowIfCancellationRequested();
						//
						//	Backup.Files.Add(bf);
						//
						//	ProcessBatch(session);
						//}

						try
						{
							daoBackup.Update(tx, Backup);
						}
						catch (Exception ex)
						{
							string message = string.Format("BUG: Failed to update {0} => {1}",
								typeof(Models.Backup).Name,
								CustomJsonSerializer.SerializeObject(Backup, 1));

							Results.OnError(this, message);
							logger.Log(LogLevel.Error, ex, message);

							throw ex;
						}
					}

					batchProcessor.ProcessBatch(session, true);
					stats.End();

					// ------------------------------------------------------------------------------------

					tx.Commit();
				}
				catch (OperationCanceledException)
				{
					string message = "Operation cancelled";

					Results.OnError(this, message);
					logger.Warn(message);

					tx.Rollback(); // Rollback the transaction
					throw;
				}
				catch (Exception ex)
				{
					string message = string.Format("Caught Exception: {0}", ex.Message);

					Results.OnError(this, message);
					logger.Log(LogLevel.Error, ex, message);

					tx.Rollback(); // Rollback the transaction
					throw;
				}
				finally
				{
					// ...
				}
			}

			IsSaved = true;

			// 6 - Create versioned files and remove files that won't belong to this backup.
			TransferSet.Files = GetFilesToTransfer(Backup, SuppliedFiles);

			// Test to see if things are okay!
			{
				var transferCount = TransferSet.Files.Count();
				var filesCount = ChangeSet.AddedFiles.Count() + ChangeSet.ModifiedFiles.Count();

				Assert.IsTrue(transferCount == filesCount, "TransferSet.Files must be equal (ChangeSet.AddedFiles + ChangeSet.ModifiedFiles)");
			}
		}

		#region Dispose Pattern Implementation

		private bool _shouldDispose = true;
		private bool _isDisposed;

		/// <summary>
		/// Implements the Dispose pattern
		/// </summary>
		/// <param name="disposing">Whether this object is being disposed via a call to Dispose
		/// or garbage collected.</param>
		private void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					// Nop.
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

		#endregion Dispose Pattern Implementation
	}

	public sealed class FailedFile<T>
	{
		public T File { get; private set; }
		public string Message { get; private set; }
		public Exception Exception { get; private set; }

		public FailedFile(T file, string message, Exception exception)
		{
			File = file;
			Message = message;
			Exception = Exception;
		}
	}

	public sealed class ChangeSet<T>
	{
		// Contains ALL `BackupPlanFile`s that were marked as ADDED to the plan associated to this backup.
		// Fact 1: SOME of its items may also be contained in `AllFilesFromPlan`.
		// Fact 2: ALL of its items are also contained in `SuppliedFiles`.
		internal IEnumerable<T> AddedFiles = new LinkedList<T>();

		// Contains ALL `BackupPlanFile`s that were marked as MODIFIED from the plan associated to this backup.
		// Fact 1: ALL of its items are also contained in `AllFilesFromPlan`.
		// Fact 2: ALL of its items are also contained in `SuppliedFiles`.
		internal IEnumerable<T> ModifiedFiles = new LinkedList<T>();

		// Contains ALL `BackupPlanFile`s that were marked as DELETED from the plan associated to this backup.
		// Fact 1: ALL of its items are also contained in `AllFilesFromPlan`.
		// Fact 2: SOME of its items may also be contained in `SuppliedFiles`.
		internal IEnumerable<T> DeletedFiles = new LinkedList<T>();

		// Contains ALL `BackupPlanFile`s that were marked as REMOVED from the plan associated to this backup.
		// Fact 1: ALL of its items are also contained in `AllFilesFromPlan`.
		internal IEnumerable<T> RemovedFiles = new LinkedList<T>();

		// Contains ALL `BackupPlanFile`s that failed due to errors including permissions, etc.
		// Fact 1: SOME of its items may also be contained in `AllFilesFromPlan`.
		// Fact 2: ALL of its items are also contained in `SuppliedFiles`.
		internal LinkedList<FailedFile<T>> FailedFiles = new LinkedList<FailedFile<T>>();
	}

	public sealed class TransferSet<T>
	{
		internal IEnumerable<T> Files;
	}
}
