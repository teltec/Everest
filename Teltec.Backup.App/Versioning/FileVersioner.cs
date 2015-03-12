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

		private bool IsFileModified(CustomVersionedFile file)
		{
			return true;
		}

		#endregion

		private void Execute(Models.Backup backup, LinkedList<string> files, bool newVersion)
		{
			LinkedList<CustomVersionedFile> filesToBeVersioned = files.ToLinkedListWithCtorConversion<CustomVersionedFile, string>();

			DoPrepareBackup(backup, filesToBeVersioned);
			DoUpdateFilesDeletionStatus(backup);
			DoUpdateFilesProperties(filesToBeVersioned);
			DoUpdateFilesVersion(backup, filesToBeVersioned);
			
			FilesToBackup = filesToBeVersioned;
			//throw new Exception("Simulating failure.");
		}
		
		Models.Backup Backup;
		Dictionary<string, BackupPlanFile> AllFilesFromPlan;
		IEnumerable<BackupPlanFile> AllDeletedFilesFromPlan;
		LinkedList<BackupPlanFile> BackupPlanFiles = new LinkedList<BackupPlanFile>();
		public LinkedList<CustomVersionedFile> FilesToBackup { get; private set; }

		public void Undo()
		{
			BackupRepository daoBackup = new BackupRepository();
			daoBackup.Refresh(Backup);
		}

		public void Save()
		{
			BackupRepository daoBackup = new BackupRepository();
			BackupPlanFileRepository daoBackupPlanFile = new BackupPlanFileRepository();
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository();

			// 1. Update all files that were deleted from the filesystem.
			foreach (var entry in AllDeletedFilesFromPlan)
			{
				if (entry.LastStatus == BackupPlanFileStatus.DELETED)
					continue;
				daoBackupPlanFile.Update(entry);
			}

			// 2. Create or update `BackupPlanFile`s.
			foreach (BackupPlanFile file in BackupPlanFiles)
			{
				daoBackupPlanFile.InsertOrUpdate(file);
			}

			// 3. Remove files that won't belong to this backup.
			var node = FilesToBackup.First;
			while (node != null)
			{
				var nextNode = node.Next;
				BackupPlanFile file = node.Value.UserData as BackupPlanFile;
				switch (file.LastStatus)
				{
					default:
						// Remove everything else.
						FilesToBackup.Remove(node);
						break;
					case BackupPlanFileStatus.ADDED:
					case BackupPlanFileStatus.MODIFIED:
						// Keep these.
						break;
				}
				node = nextNode;
			}

			// 4. Add `BackupedFile`s to the `Backup`.
			foreach (CustomVersionedFile versionedFile in FilesToBackup)
			{
				//
				// Create `BackupedFile`.
				//
				BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(Backup, versionedFile.Path);
				if (backupedFile == null) // If we're resuming, this should already exist.
				{
					backupedFile = new BackupedFile(Backup, versionedFile.UserData as BackupPlanFile);
				}
				backupedFile.UpdatedAt = DateTime.UtcNow;
				Backup.Files.Add(backupedFile);
			}

			// 5. Insert `Backup` and its `BackupedFile`s into the database.
			daoBackup.Update(Backup);
		}

		//
		// Summary:
		// 1. Add files to `backup.Files`;
		// 2. Insert these files into the database, if they aren't already - Refers to `BackupPlanFile`;
		// 3. Update the backup and add its files to the database - Refers to `Backup` and `BackupedFile`;
		// 4. Do not save to the database because this method is run by another thread.
		//
		private void DoPrepareBackup(Models.Backup backup, LinkedList<CustomVersionedFile> files)
		{
			// Check all files.
			foreach (CustomVersionedFile entry in files)
			{
				// Throw if the operation was canceled.
				CancellationTokenSource.Token.ThrowIfCancellationRequested();

				string path = entry.Path;

				//
				// Create or update `BackupPlanFile`.
				//
				BackupPlanFile backupPlanFile = null;
				bool backupPlanFileAlreadyExists = AllFilesFromPlan.TryGetValue(path, out backupPlanFile);

				if (!backupPlanFileAlreadyExists)
				{
					backupPlanFile = new BackupPlanFile(backup.BackupPlan);
					backupPlanFile.Path = path;
					backupPlanFile.CreatedAt = DateTime.UtcNow;
				}
				else
				{
					backupPlanFile.UpdatedAt = DateTime.UtcNow;
				}

				// Associate both types so later we use it to remove unchanged/deleted files from the resulting list,
				// and also update their properties.
				entry.UserData = backupPlanFile;

				BackupPlanFiles.AddLast(backupPlanFile);

				//
				// Check what happened to the file.
				//

				bool fileExistsOnFilesystem = File.Exists(path);

				if (backupPlanFileAlreadyExists) // File was backed up at least once in the past?
				{
					if (fileExistsOnFilesystem) // Exists?
					{
						if (IsFileModified(entry)) // Modified?
						{
							backupPlanFile.LastStatus = BackupPlanFileStatus.MODIFIED;
						}
						else // Not modified?
						{
							backupPlanFile.LastStatus = BackupPlanFileStatus.UNCHANGED;
						}
					}
					else // Deleted from filesystem?
					{
						backupPlanFile.LastStatus = BackupPlanFileStatus.DELETED;
					}
				}
				else // Adding to this backup? We MUST NOT change the plan!
				{
					if (fileExistsOnFilesystem) // Exists?
					{
						backupPlanFile.LastStatus = BackupPlanFileStatus.ADDED;
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
		// 3. Do not save to the database because this method is run by another thread.
		//
		private void DoUpdateFilesDeletionStatus(Models.Backup backup)
		{
			Assert.IsNotNull(AllFilesFromPlan);

			// Find all files that were already backed up at least once, but are not present in this on-going backup.
			AllDeletedFilesFromPlan = AllFilesFromPlan.Values.Except(backup.BackupPlan.Files);

			foreach (BackupPlanFile entry in AllDeletedFilesFromPlan)
			{
				// Throw if the operation was canceled.
				CancellationTokenSource.Token.ThrowIfCancellationRequested();

				// Skip deleted files.
				if (entry.LastStatus == BackupPlanFileStatus.DELETED)
					continue;

				entry.LastStatus = BackupPlanFileStatus.DELETED;
				entry.UpdatedAt = DateTime.UtcNow;
			}
		}

		//
		// Summary:
		// 1. Update all files' properties: size, last written date, etc.
		//
		private void DoUpdateFilesProperties(LinkedList<CustomVersionedFile> files)
		{
			foreach (CustomVersionedFile entry in files)
			{
				// Throw if the operation was canceled.
				CancellationTokenSource.Token.ThrowIfCancellationRequested();

				BackupPlanFile backupPlanFile = entry.UserData as BackupPlanFile;

				// Skip deleted files.
				if (backupPlanFile.LastStatus == BackupPlanFileStatus.DELETED)
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
		// 1. ...
		// 2. ...
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
