using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Models;
using Teltec.Common.Extensions;
using Teltec.Storage;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.App.Versioning
{
	public sealed class FileVersioner : IDisposable
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		CancellationTokenSource CancellationTokenSource; // IDisposable

		public FileVersioner()
		{
			//_daoBackups = new BackupRepository();
			//_daoBackupPlanFiles = new BackupPlanFileRepository(_daoBackups.Session);
			CancellationTokenSource = new CancellationTokenSource();
		}

		public async Task NewVersion(Models.Backup backup, LinkedList<string> files)
		{
			Assert.IsNotNull(backup);
			Assert.AreEqual(backup.Status, BackupStatus.RUNNING);
			Assert.IsNotNull(files);

			Backup = backup;

			BackupPlanFileRepository daoBackupPlanFile = new BackupPlanFileRepository();
			AllFilesFromPlan = daoBackupPlanFile.GetAllByPlan(backup.BackupPlan).ToDictionary<BackupPlanFile, string>(p => p.Path);

			await ExecuteOnBackround(() =>
			{
				Execute(backup, files, true);
			}, CancellationTokenSource.Token);
		}

		//public async Task ResumeVersion(Models.Backup backup, LinkedList<CustomVersionedFile> files)
		//{
		//	Assert.IsNotNull(backup);
		//	Assert.AreEqual(backup.Status, BackupStatus.RUNNING);
		//	Assert.IsNotNull(files);
		//
		//	BackupPlanFileRepository daoBackupPlanFile = new BackupPlanFileRepository();
		//	AllFilesFromPlan = daoBackupPlanFile.GetAllByPlan(backup.BackupPlan).ToDictionary<BackupPlanFile, string>(p => p.Path);
		//
		//	Backup = backup;
		//
		//	await ExecuteOnBackround(() =>
		//	{
		//		Execute(agent, false);
		//	}, CancellationTokenSource.Token);
		//}

		public void Cancel()
		{
			CancellationTokenSource.Cancel();
		}

		private Task ExecuteOnBackround(Action action, CancellationToken token)
		{
			return Task.Run(action, token);
		}

		#region FileHandling

		private bool IsFileModified(BackupPlanFile file)
		{
			Assert.IsNotNull(file);
			
			DateTime dt1 = file.LastWrittenAt;
			DateTime dt2 = FileManager.GetLastWriteTimeUtc(file.Path).Value;
			
			// Strip milliseconds off from both dates!
			dt1 = dt1.AddTicks(-(dt1.Ticks % TimeSpan.TicksPerSecond));
			dt2 = dt2.AddTicks(-(dt2.Ticks % TimeSpan.TicksPerSecond));

			return DateTime.Compare(dt1, dt2) != 0;
		}

		#endregion

		bool IsSaved = false;
		Models.Backup Backup;
		Dictionary<string, BackupPlanFile> AllFilesFromPlan;
		LinkedList<BackupPlanFile> BackupPlanFiles;
		LinkedList<CustomVersionedFile> InternalFilesToBackup;

		public LinkedList<CustomVersionedFile> FilesToBackup
		{
			get
			{
				Assert.IsTrue(IsSaved);
				return InternalFilesToBackup;
			}
		}

		private void Execute(Models.Backup backup, LinkedList<string> files, bool newVersion)
		{
			LinkedList<CustomVersionedFile> filesToBeVersioned = files.ToLinkedListWithCtorConversion<CustomVersionedFile, string>();

			BackupPlanFiles = DoLoadOrCreateBackupPlanFiles(backup.BackupPlan, filesToBeVersioned);
			DoUpdateFilesStatus(backup, filesToBeVersioned);
			DoUpdateFilesDeletionStatus(BackupPlanFiles);
			DoUpdateFilesProperties(filesToBeVersioned);
			DoUpdateFilesVersion(backup, filesToBeVersioned);

			InternalFilesToBackup = filesToBeVersioned;
			//throw new Exception("Simulating failure.");
		}

		//
		// Loads or creates `BackupPlanFile`s for each file in `files`.
		// Returns the complete list of `BackupPlanFile`s that are related to `files`.
		// It modifies the `UserData` property for each file in `files`.
		// NOTE: Does not save to the database because this method is run by a secondary thread.
		//
		private LinkedList<BackupPlanFile> DoLoadOrCreateBackupPlanFiles(Models.BackupPlan plan, LinkedList<CustomVersionedFile> files)
		{
			Assert.IsNotNull(plan);
			Assert.IsNotNull(files);

			LinkedList<BackupPlanFile> result = new LinkedList<BackupPlanFile>();

			// Check all files.
			foreach (CustomVersionedFile entry in files)
			{
				// Throw if the operation was canceled.
				CancellationTokenSource.Token.ThrowIfCancellationRequested();

				//
				// Create or update `BackupPlanFile`.
				//
				BackupPlanFile backupPlanFile = null;
				bool backupPlanFileAlreadyExists = AllFilesFromPlan.TryGetValue(entry.Path, out backupPlanFile);

				if (!backupPlanFileAlreadyExists)
				{
					backupPlanFile = new BackupPlanFile(plan);
					backupPlanFile.Path = entry.Path;
					backupPlanFile.CreatedAt = DateTime.UtcNow;
				}
				else
				{
					backupPlanFile.UpdatedAt = DateTime.UtcNow;
				}

				// Associate both types so later we use it to remove unchanged/deleted
				// files from the resulting list, and also update their properties.
				entry.UserData = backupPlanFile;

				result.AddLast(backupPlanFile);
			}

			return result;
		}

		//
		// Summary:
		// 1. Add files to `backup.Files`;
		// 2. Insert these files into the database, if they aren't already - Refers to `BackupPlanFile`;
		// 3. Update the backup and add its files to the database - Refers to `Backup` and `BackupedFile`;
		// NOTE: Does not save to the database because this method is run by a secondary thread.
		//
		private void DoUpdateFilesStatus(Models.Backup backup, LinkedList<CustomVersionedFile> files)
		{
			Assert.IsNotNull(backup);
			Assert.IsNotNull(files);

			// Check all files.
			foreach (CustomVersionedFile entry in files)
			{
				// Throw if the operation was canceled.
				CancellationTokenSource.Token.ThrowIfCancellationRequested();

				string path = entry.Path;

				BackupPlanFile backupPlanFile = entry.UserData as BackupPlanFile;

				//
				// Check what happened to the file.
				//

				bool fileExistsOnFilesystem = File.Exists(path);

				if (backupPlanFile.Id.HasValue) // File was backed up at least once in the past?
				{
					if (fileExistsOnFilesystem) // Exists?
					{
						if (backupPlanFile.LastStatus == BackupFileStatus.DELETED) // File was marked as DELETED by a previous backup?
						{
							backupPlanFile.LastStatus = BackupFileStatus.ADDED;
						}
						else
						{
							if (IsFileModified(backupPlanFile)) // Modified?
							{
								backupPlanFile.LastStatus = BackupFileStatus.MODIFIED;
							}
							else // Not modified?
							{
								backupPlanFile.LastStatus = BackupFileStatus.UNCHANGED;
							}
						}
					}
					else // Deleted from filesystem?
					{
						backupPlanFile.LastStatus = BackupFileStatus.DELETED;
					}
				}
				else // Adding to this backup? We MUST NOT change the plan!
				{
					if (fileExistsOnFilesystem) // Exists?
					{
						backupPlanFile.LastStatus = BackupFileStatus.ADDED;
					}
					else
					{
						// Error?
					}
				}
			}
		}

		//
		// Summary:
		// 1. Check which files where deleted locally;
		// 2. Update their status;
		// NOTE: Does not save to the database because this method is run by a secondary thread.
		//
		private void DoUpdateFilesDeletionStatus(LinkedList<BackupPlanFile> files)
		{
			Assert.IsNotNull(files);
			Assert.IsNotNull(AllFilesFromPlan);

			// Find all files that were already backed up at least once, but are not present in this on-going backup.
			IEnumerable<BackupPlanFile> deletedFiles = AllFilesFromPlan.Values.Except(files);

			foreach (BackupPlanFile entry in deletedFiles)
			{
				// Throw if the operation was canceled.
				CancellationTokenSource.Token.ThrowIfCancellationRequested();

				// Skip files that were already marked as DELETED.
				if (entry.LastStatus == BackupFileStatus.DELETED)
					continue;

				entry.LastStatus = BackupFileStatus.DELETED;
				entry.UpdatedAt = DateTime.UtcNow;
			}
		}

		//
		// Summary:
		// Update all files' properties like size, last written date, etc, skipping files
		// marked as DELETED.
		// NOTE: Does not save to the database because this method is run by a secondary thread.
		//
		private void DoUpdateFilesProperties(LinkedList<CustomVersionedFile> files)
		{
			foreach (CustomVersionedFile entry in files)
			{
				// Throw if the operation was canceled.
				CancellationTokenSource.Token.ThrowIfCancellationRequested();

				BackupPlanFile backupPlanFile = entry.UserData as BackupPlanFile;

				// Skip deleted files.
				if (backupPlanFile.LastStatus == BackupFileStatus.DELETED)
					continue;

				// Update file related properties
				string path = entry.Path;
				entry.Size = backupPlanFile.LastSize = FileManager.GetFileSize(path).Value;
				entry.LastWriteTimeUtc = backupPlanFile.LastWrittenAt = FileManager.GetLastWriteTimeUtc(path).Value;

				if (backupPlanFile.Id.HasValue)
					backupPlanFile.UpdatedAt = DateTime.UtcNow;
			}
		}

		//
		// Summary:
		// ...
		//
		private void DoUpdateFilesVersion(Models.Backup backup, LinkedList<CustomVersionedFile> files)
		{
			IFileVersion version = new FileVersion { Version = backup.Id.Value.ToString() };

			// Update files version.
			foreach (CustomVersionedFile entry in files)
			{
				// Throw if the operation was canceled.
				CancellationTokenSource.Token.ThrowIfCancellationRequested();

				entry.Version = version;
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
		// 1. Insert/Update `BackupPlanFile`s into the database.
		// 2. Remove files that won't belong to this backup;
		// 3. Add `BackupedFile`s to `Backup.Files`.
		// 3. Update the backup and add its files to the database - Refers to `Backup` and `BackupedFile`;
		// 4. Insert/Update `Backup` and its `BackupedFile`s into the database.
		//
		public void Save()
		{
			Assert.IsFalse(IsSaved);
			BackupRepository daoBackup = new BackupRepository();
			BackupPlanFileRepository daoBackupPlanFile = new BackupPlanFileRepository();
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository();

			// 1. Insert or update `BackupPlanFile`s.
			foreach (BackupPlanFile file in BackupPlanFiles)
			{
				daoBackupPlanFile.InsertOrUpdate(file);
			}

			// 2. Remove files that won't belong to this backup.
			var node = InternalFilesToBackup.First;
			while (node != null)
			{
				var nextNode = node.Next;
				BackupPlanFile file = node.Value.UserData as BackupPlanFile;
				switch (file.LastStatus)
				{
					default:
						// Remove everything else.
						InternalFilesToBackup.Remove(node);
						break;
					case BackupFileStatus.ADDED:
					case BackupFileStatus.MODIFIED:
						// Keep these.
						break;
				}
				node = nextNode;
			}

			// 3. Add `BackupedFile`s to the `Backup`.
			foreach (CustomVersionedFile versionedFile in InternalFilesToBackup)
			{
				BackupPlanFile backupPlanFile = versionedFile.UserData as BackupPlanFile;
				//
				// Create `BackupedFile`.
				//
				BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(Backup, versionedFile.Path);
				if (backupedFile == null) // If we're resuming, this should already exist.
				{
					backupedFile = new BackupedFile(Backup, backupPlanFile);
				}
				backupedFile.FileStatus = backupPlanFile.LastStatus;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				Backup.Files.Add(backupedFile);
			}

			// 4. Insert/Update `Backup` and its `BackupedFile`s into the database.
			daoBackup.Update(Backup);
			IsSaved = true;
		}

		#region Dispose Pattern Implementation

		bool _shouldDispose = true;
		bool _isDisposed;

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
					if (CancellationTokenSource != null)
					{
						CancellationTokenSource.Dispose();
						CancellationTokenSource = null;
					}
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

		#endregion
	}
}
