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

		public IncrementalFileVersioner(CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
		}

		public async Task NewVersion(Models.Backup backup, LinkedList<string> files)
		{
			await DoVersion(backup, files, true);
		}

		public async Task ResumeVersion(Models.Backup backup, LinkedList<string> files)
		{
			await DoVersion(backup, files, false);
		}

		public async Task DoVersion(Models.Backup backup, LinkedList<string> filePaths, bool newVersion)
		{
			Assert.IsNotNull(backup);
			Assert.AreEqual(TransferStatus.RUNNING, backup.Status);
			Assert.IsNotNull(filePaths);

			await ExecuteOnBackround(() =>
			{
				ISession session = NHibernateHelper.GetSession();
				try
				{
					BackupRepository daoBackup = new BackupRepository(session);
					Backup = daoBackup.Get(backup.Id);

					BackupPlanFileRepository daoBackupPlanFile = new BackupPlanFileRepository(session);
					AllFilesFromPlan = daoBackupPlanFile.GetAllByBackupPlan(backup.BackupPlan).ToDictionary<Models.BackupPlanFile, string>(p => p.Path);

					Execute(backup, filePaths, newVersion);

					Save(session);
				}
				finally
				{
					//session.Close();
					if (session.IsConnected)
						session.Disconnect();
				}
			}, CancellationToken);
		}

		private Task ExecuteOnBackround(Action action, CancellationToken token)
		{
			return Task.Run(action, token);
			//return AsyncHelper.ExecuteOnBackround(action, token);
		}

		#region File change detection

		private readonly BigInteger MAX_FILESIZE_TO_HASH = 10 * BigInteger.Pow(2, 20); // 10 MB
		private readonly HashAlgorithm HashAlgo = new SHA1CryptoServiceProvider(); // SHA-1 is 160 bits long (20 bytes)

		private bool IsFileModified(Models.BackupPlanFile file)
		{
			try
			{
				bool byDate = IsFileModifiedByDate(file);
				if (!byDate)
					return false;

				long fileLength = FileManager.UnsafeGetFileSize(file.Path);

				// Skip files larger than `MAX_FILESIZE_TO_HASH`.
				int result = BigInteger.Compare(fileLength, MAX_FILESIZE_TO_HASH);
				if (result > 0)
					return byDate;

				byte[] checksum;
				bool byHash = IsFileModifiedByHash(file, out checksum);
				if (byHash)
					file.LastChecksum = checksum;
				return byHash;
			}
			catch (Exception ex)
			{
				logger.Warn("Caught an exception while checking file modification status \"{0}\": {1}", file.Path, ex.Message);
				throw ex;
			}
		}

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

		private bool IsFileModifiedByHash(Models.BackupPlanFile file, out byte[] checksum)
		{
			checksum = CalculateHashForFile(file.Path);
			return file.LastChecksum == null
				? true
				: !checksum.SequenceEqual(file.LastChecksum);
		}

		private bool IsFileModifiedByDate(Models.BackupPlanFile file)
		{
			Assert.IsNotNull(file);

			DateTime dt1 = file.LastWrittenAt;
			DateTime dt2 = FileManager.UnsafeGetFileLastWriteTimeUtc(file.Path);

			// Strip milliseconds off from both dates because we don't care - currently.
			dt1 = dt1.AddTicks(-(dt1.Ticks % TimeSpan.TicksPerSecond));
			dt2 = dt2.AddTicks(-(dt2.Ticks % TimeSpan.TicksPerSecond));

			return DateTime.Compare(dt1, dt2) != 0;
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

			ChangeSet.AddedFiles = GetAddedFiles();
#if false
			foreach (var item in ChangeSet.AddedFiles)
				Console.WriteLine("BackupPlanAddedFiles: {0}", item.Path);
#endif
			ChangeSet.ModifiedFiles = GetModifiedFiles();
#if false
			foreach (var item in changeset.modifiedfiles)
				console.writeline("backupplanmodifiedfiles: {0}", item.path);
#endif
			// DO NOT update files removal and deletion status for `ResumeBackupOperation`,
			// only for `NewBackupOperation`.
			if (isNewVersion)
			{
				ChangeSet.RemovedFiles = GetRemovedFiles();
#if false
				foreach (var item in ChangeSet.RemovedFiles)
					Console.WriteLine("BackupPlanRemovedFiles: {0}", item.Path);
#endif
				ChangeSet.DeletedFiles = GetDeletedFilesAndUpdateTheirStatus(SuppliedFiles);
			}

			DoUpdateFilesProperties(SuppliedFiles);
			//throw new Exception("Simulating failure.");
		}

		//
		// Loads or creates `BackupPlanFile`s for each file in `files`.
		// Returns the complete list of `BackupPlanFile`s that are related to `files`.
		// It modifies the `UserData` property for each file in `files`.
		// NOTE: Does not save to the database because this method is run by a secondary thread.
		//
		private LinkedList<Models.BackupPlanFile> DoLoadOrCreateBackupPlanFiles(Models.BackupPlan plan, LinkedList<string> filePaths)
		{
			Assert.IsNotNull(plan);
			Assert.IsNotNull(filePaths);
			Assert.IsNotNull(AllFilesFromPlan);

			BlockPerfStats stats = new BlockPerfStats();
			stats.Begin();

			LinkedList<Models.BackupPlanFile> result = new LinkedList<Models.BackupPlanFile>();

			// Check all files.
			foreach (string path in filePaths)
			{
				// Throw if the operation was canceled.
				CancellationToken.ThrowIfCancellationRequested();

				//
				// Create or update `BackupPlanFile`.
				//
				Models.BackupPlanFile backupPlanFile = null;
				bool backupPlanFileAlreadyExists = AllFilesFromPlan.TryGetValue(path, out backupPlanFile);

				if (!backupPlanFileAlreadyExists)
				{
					backupPlanFile = new Models.BackupPlanFile(plan, path);
					backupPlanFile.CreatedAt = DateTime.UtcNow;
				}

				result.AddLast(backupPlanFile);
			}

			stats.End();

			return result;
		}

		private bool NeedsToRetryFile(Models.BackupPlanFile planFile)
		{
			Models.BackupedFile lastVersion = planFile.Versions != null ? planFile.Versions.Last() : null;
			if (lastVersion == null)
				return false;

			return lastVersion.TransferStatus == TransferStatus.FAILED // File transfer failed.
				|| lastVersion.TransferStatus == TransferStatus.CANCELED // File transfer was canceled.
				|| lastVersion.Backup.Status == TransferStatus.CANCELED; // Whole previous backup was canceled.
		}

		//
		// Summary:
		// Update the `LastStatus` property of each file in `files` according to the actual
		// state of the file in the filesystem.
		// NOTE: This function has a side effect - It updates properties of items from `files`.
		//
		private void DoUpdateBackupPlanFilesStatus(LinkedList<Models.BackupPlanFile> files, bool isNewVersion)
		{
			Assert.IsNotNull(files);

			BlockPerfStats stats = new BlockPerfStats();
			stats.Begin();

			// Check all files.
			LinkedListNode<Models.BackupPlanFile> node = files.First;
			while (node != null)
			{
				var next = node.Next;
				Models.BackupPlanFile entry = node.Value;

				// Throw if the operation was canceled.
				CancellationToken.ThrowIfCancellationRequested();

				//
				// Check what happened to the file.
				//

				bool fileExistsOnFilesystem = FileManager.FileExists(entry.Path);
				Models.BackupFileStatus? changeStatusTo = null;

				if (entry.Id.HasValue) // File was backed up at least once in the past?
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

						default: // ADDED, MODIFIED, UNMODIFIED
							if (fileExistsOnFilesystem) // Exists?
							{
								// DO NOT verify whether the file changed for a `ResumeBackupOperation`,
								// only for `NewBackupOperation`.
								if (isNewVersion)
								{
									try
									{
										if (IsFileModified(entry)) // Modified?
										{
											changeStatusTo = Models.BackupFileStatus.MODIFIED;
										}
										else if (NeedsToRetryFile(entry)) // Failed in the last backup?
										{
											changeStatusTo = Models.BackupFileStatus.MODIFIED;
										}
										else // Not modified?
										{
											changeStatusTo = Models.BackupFileStatus.UNCHANGED;
										}
									}
									catch (Exception ex)
									{
										FailedFile<Models.BackupPlanFile> failedEntry = new FailedFile<Models.BackupPlanFile>(entry, ex.Message, ex);
										ChangeSet.FailedFiles.AddLast(failedEntry);

										// Remove this entry from `files` as it clearly failed.
										files.Remove(node); // Complexity is O(1)
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

				node = next;
			}

			stats.End();
		}

		//
		// Summary:
		// Return all files from `SuppliedFiles` which are marked as `ADDED`;
		// NOTE: This function has no side effects.
		//
		private IEnumerable<Models.BackupPlanFile> GetAddedFiles()
		{
			Assert.IsNotNull(SuppliedFiles);

			// Find all `BackupPlanFile`s from this `BackupPlan` that are marked as ADDED.
			return SuppliedFiles.Where(p => p.LastStatus == Models.BackupFileStatus.ADDED);
		}

		//
		// Summary:
		// Return all files from `SuppliedFiles` which are marked as `MODIFIED`;
		// NOTE: This function has no side effects.
		//
		private IEnumerable<Models.BackupPlanFile> GetModifiedFiles()
		{
			Assert.IsNotNull(SuppliedFiles);

			// Find all `BackupPlanFile`s from this `BackupPlan` that are marked as MODIFIED.
			return SuppliedFiles.Where(p => p.LastStatus == Models.BackupFileStatus.MODIFIED);
		}

		//
		// Summary:
		// Return all files from `AllFilesFromPlan` which are marked as `REMOVED`;
		// NOTE: This function has no side effects.
		//
		private IEnumerable<Models.BackupPlanFile> GetRemovedFiles()
		{
			Assert.IsNotNull(AllFilesFromPlan);

			// Find all `BackupPlanFile`s from this `BackupPlan` that are marked as REMOVED.
			return AllFilesFromPlan.Values.Where(p => p.LastStatus == Models.BackupFileStatus.REMOVED);
		}

		//
		// Summary:
		// 1. Check which files from `files` are not marked as DELETED;
		// 2. Check which files from `files` are not marked as REMOVED;
		// 3. Check which files from `AllFilesFromPlan` are not in the results of 1 AND 2, meaning
		//    they have been deleted from the filesystem.
		// 4. Mark them as DELETED;
		// 5. Return the union of all files from 1 and 3;
		// NOTE: This function has a side effect - It updates properties of items from `files`.
		//
		private IEnumerable<Models.BackupPlanFile> GetDeletedFilesAndUpdateTheirStatus(LinkedList<Models.BackupPlanFile> files)
		{
			Assert.IsNotNull(files);
			Assert.IsNotNull(AllFilesFromPlan);

			// 1. Find all files from `files` that were previously marked as DELETED.
			IEnumerable<Models.BackupPlanFile> deletedFiles = files.Where(p => p.LastStatus == Models.BackupFileStatus.DELETED);
#if false
			foreach (var item in deletedFiles)
				Console.WriteLine("GetDeletedFilesAndUpdateTheirStatus: deletedFiles: {0}", item.Path);
#endif

			// 2. Find all files from `files` that were previously marked as REMOVED.
			IEnumerable<Models.BackupPlanFile> nonRemovedFiles = files.Where(p => p.LastStatus != Models.BackupFileStatus.REMOVED);
#if false
			foreach (var item in nonRemovedFiles)
				Console.WriteLine("GetDeletedFilesAndUpdateTheirStatus: nonRemovedFiles: {0}", item.Path);
#endif

			// 3. Check which files from `AllFilesFromPlan` are not in the results of 1 AND 2, meaning
			//    they have been deleted from the filesystem.
			IEnumerable<Models.BackupPlanFile> deletedFilesToBeUpdated = AllFilesFromPlan.Values.Except(nonRemovedFiles).Except(deletedFiles);
#if false
			foreach (var item in deletedFilesToBeUpdated)
				Console.WriteLine("GetDeletedFilesAndUpdateTheirStatus: deletedFilesToBeUpdated: {0}", item.Path);
#endif

			// 4. Mark them as DELETED;
			foreach (Models.BackupPlanFile entry in deletedFilesToBeUpdated)
			{
				// Throw if the operation was canceled.
				CancellationToken.ThrowIfCancellationRequested();

				entry.LastStatus = Models.BackupFileStatus.DELETED;
				entry.UpdatedAt = DateTime.UtcNow;
			}

			// 5. Return all files from 1 and 3;
			List<Models.BackupPlanFile> result = new List<Models.BackupPlanFile>(deletedFiles.Count() + deletedFilesToBeUpdated.Count());
			result.AddRange(deletedFiles);
			result.AddRange(deletedFilesToBeUpdated);
#if false
			foreach (var item in result)
				Console.WriteLine("GetDeletedFilesAndUpdateTheirStatus: result: {0}", item.Path);
#endif

			return result;
		}

		//
		// Summary:
		// Update all files' properties like size, last written date, etc, skipping files
		// marked as REMOVED, DELETED or UNCHANGED.
		// NOTE: This function has a side effect - It updates properties of items from `files`.
		//
		private void DoUpdateFilesProperties(LinkedList<Models.BackupPlanFile> files)
		{
			BatchProcessor batchProcessor = new BatchProcessor();

			LinkedListNode<Models.BackupPlanFile> node = files.First;
			while (node != null)
			{
				var next = node.Next;
				Models.BackupPlanFile entry = node.Value;

				batchProcessor.ProcessBatch(null);

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
							// Update file related properties
							string path = entry.Path;
							try
							{
								long lastSize = FileManager.UnsafeGetFileSize(path);
								DateTime lastWrittenAt = FileManager.UnsafeGetFileLastWriteTimeUtc(path);

								entry.LastSize = lastSize;
								entry.LastWrittenAt = lastWrittenAt;

								if (entry.Id.HasValue)
									entry.UpdatedAt = DateTime.UtcNow;
							}
							catch (Exception ex)
							{
								FailedFile<Models.BackupPlanFile> failedEntry = new FailedFile<Models.BackupPlanFile>(entry, ex.Message, ex);
								ChangeSet.FailedFiles.AddLast(failedEntry);

								// Remove this entry from `files` as it clearly failed.
								files.Remove(node); // Complexity is O(1)
							}
							break;
						}
				}

				node = next;
			}
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
			Assert.IsFalse(IsSaved);
			BackupRepository daoBackup = new BackupRepository();
			daoBackup.Refresh(Backup);
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
							logger.Log(LogLevel.Error, ex, "BUG: Failed to create/update {0} => {1}",
								typeof(Models.BackupPlanPathNode).Name,
								CustomJsonSerializer.SerializeObject(entry, 1));
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
							logger.Log(LogLevel.Error, ex, "BUG: Failed to insert/update {0} => {1}",
								typeof(Models.BackupPlanFile).Name,
								CustomJsonSerializer.SerializeObject(entry, 1));
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
								logger.Log(LogLevel.Error, ex, "BUG: Failed to update {0} => {1} ",
									typeof(Models.BackupPlanFile).Name,
									CustomJsonSerializer.SerializeObject(file, 1));
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
							logger.Log(LogLevel.Error, ex, "BUG: Failed to update {0} => {1}",
								typeof(Models.Backup).Name,
								CustomJsonSerializer.SerializeObject(Backup, 1));
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
					logger.Warn("Operation cancelled");
					tx.Rollback(); // Rollback the transaction
					throw;
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, ex, "Caught Exception");
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
