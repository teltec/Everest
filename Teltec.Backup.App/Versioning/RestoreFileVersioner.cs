﻿using NHibernate;
using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.Data.DAO.NH;
using Teltec.Backup.Data.Versioning;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.App.Versioning
{
	public sealed class RestoreFileVersioner : IDisposable
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		CancellationToken CancellationToken;

		public RestoreFileVersioner(CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
		}

		public async Task NewRestore(Models.Restore restore, LinkedList<CustomVersionedFile> files)
		{
			await DoRestore(restore, files, true);
		}

		public async Task ResumeRestore(Models.Restore restore, LinkedList<CustomVersionedFile> files)
		{
			await DoRestore(restore, files, false);
		}

		public async Task DoRestore(Models.Restore restore, LinkedList<CustomVersionedFile> files, bool newRestore)
		{
			Assert.IsNotNull(restore);
			Assert.AreEqual(TransferStatus.RUNNING, restore.Status);
			Assert.IsNotNull(files);

			Restore = restore;

			RestorePlanFileRepository daoRestorePlanFile = new RestorePlanFileRepository();
			AllFilesFromPlan = daoRestorePlanFile.GetAllByPlan(restore.RestorePlan).ToDictionary<Models.RestorePlanFile, string>(p => p.Path);

			await ExecuteOnBackround(() =>
			{
				Execute(restore, files, newRestore);
			}, CancellationToken);
		}

		private Task ExecuteOnBackround(Action action, CancellationToken token)
		{
			return Task.Run(action, token);
			//return AsyncHelper.ExecuteOnBackround(action, token);
		}

		bool IsSaved = false;
		Models.Restore Restore;

		// Contains ALL `RestorePlanFile`s that were registered at least once for the plan associated to this restore.
		// Fact 1: ALL of its items are also contained (distributed) in:
		//		`SuppliedFiles`
		Dictionary<string, Models.RestorePlanFile> AllFilesFromPlan;

		// Contains ALL `RestorePlanFile`s that were informed to be included in this restore.
		// Fact 1: ALL of its items are also contained in `AllFilesFromPlan`.
		LinkedList<Models.RestorePlanFile> SuppliedFiles;

		// After `Save()`, contains ALL `CustomVersionedFile`s that are eligible for transfer.
		TransferSet<CustomVersionedFile> TransferSet = new TransferSet<CustomVersionedFile>();

		public IEnumerable<CustomVersionedFile> FilesToTransfer
		{
			get
			{
				Assert.IsTrue(IsSaved);
				return TransferSet.Files;
			}
		}

		private void Execute(Models.Restore restore, LinkedList<CustomVersionedFile> files, bool isNewRestore)
		{
			// The `files` argument contains the filesystem paths and versions informed by the user for this restore.

			//
			// NOTE: The methods call ORDER is important!
			//

			SuppliedFiles = DoLoadOrCreateRestorePlanFiles(restore.RestorePlan, files);

			//throw new Exception("Simulating failure.");
		}

		//
		// Loads or creates `RestorePlanFile`s for each file in `files`.
		// Returns the complete list of `RestorePlanFile`s that are related to `files`.
		// NOTE: Does not save to the database because this method is run by a secondary thread.
		//
		private LinkedList<Models.RestorePlanFile> DoLoadOrCreateRestorePlanFiles(Models.RestorePlan plan, LinkedList<CustomVersionedFile> files)
		{
			Assert.IsNotNull(plan);
			Assert.IsNotNull(files);
			Assert.IsNotNull(AllFilesFromPlan);

			LinkedList<Models.RestorePlanFile> result = new LinkedList<Models.RestorePlanFile>();
			BackupPlanPathNodeRepository daoPathNode = new BackupPlanPathNodeRepository();

			// Check all files.
			foreach (CustomVersionedFile file in files)
			{
				// Throw if the operation was canceled.
				CancellationToken.ThrowIfCancellationRequested();

				//
				// Create or update `RestorePlanFile`.
				//
				Models.RestorePlanFile restorePlanFile = null;
				bool backupPlanFileAlreadyExists = AllFilesFromPlan.TryGetValue(file.Path, out restorePlanFile);

				if (!backupPlanFileAlreadyExists)
				{
					restorePlanFile = new Models.RestorePlanFile(plan, file.Path);
					restorePlanFile.CreatedAt = DateTime.UtcNow;
				}

				Models.BackupPlanPathNode pathNode = daoPathNode.GetByPlanAndTypeAndPath(plan.BackupPlan, Models.EntryType.FILE, file.Path);
				Assert.IsNotNull(pathNode, string.Format("{0} has no corresponding {1}", file.Path, typeof(Models.BackupPlanPathNode).Name));
				restorePlanFile.PathNode = pathNode;
				restorePlanFile.VersionedFile = file;
				result.AddLast(restorePlanFile);
			}

			return result;
		}

		//
		// Summary:
		// ...
		//
		private IEnumerable<CustomVersionedFile> GetFilesToTransfer(Models.Restore restore, LinkedList<Models.RestorePlanFile> files)
		{
			return files.Select(p => p.VersionedFile);
		}

		public void Undo()
		{
			Assert.IsFalse(IsSaved);
			RestoreRepository daoRestore = new RestoreRepository();
			daoRestore.Refresh(Restore);
		}

		//
		// Summary:
		// 1. Insert or update all `RestorePlanFile`s from the restore plan associated with this restore operation.
		// 2. Create `RestoredFile`s as necessary and add them to the `Restore`.
		// 3. Insert/Update `Restore` and its `RestoredFile`s into the database.
		// 4. Create versioned files and remove files that won't belong to this restore.
		//
		// IMPORTANT:
		//	Do not allow cancelation during database manipulations until we have a better
		//	transaction management.
		//
		public void Save()
		{
			Assert.IsFalse(IsSaved);

			ISession session = NHibernateHelper.GetSession();

			RestoreRepository daoRestore = new RestoreRepository(session);
			RestorePlanFileRepository daoRestorePlanFile = new RestorePlanFileRepository(session);
			RestoredFileRepository daoRestoredFile = new RestoredFileRepository(session);
			BackupPlanPathNodeRepository daoBackupPlanPathNode = new BackupPlanPathNodeRepository(session);
			
			var FilesToTrack = SuppliedFiles;
			var FilesToInsertOrUpdate = FilesToTrack;

			// 1. Create `RestorePlanFile`s and `RestoredFile`s as necessary and add them to the `Restore`.
			using (ITransaction tx = session.BeginTransaction())
			{
				foreach (Models.RestorePlanFile entry in FilesToInsertOrUpdate)
				{
					// Since we're running in the same thread that does update UI.
					Application.DoEvents();

					// 1.1 - Insert/Update RestorePlanFile's and RestoredFile's if they don't exist yet.

					// IMPORTANT: It's important that we guarantee the referenced `RestorePlanFile` has a valid `Id`
					// before we reference it elsewhere, otherwise NHibernate won't have a valid value to put on
					// the `restore_plan_file_id` column.
					daoRestorePlanFile.InsertOrUpdate(tx, entry); // Guarantee it's saved 

					Models.RestoredFile restoredFile = daoRestoredFile.GetByRestoreAndPath(Restore, entry.Path);
					if (restoredFile == null) // If we're resuming, this should already exist.
					{
						// Create `RestoredFile`.
						restoredFile = new Models.RestoredFile(Restore, entry);
					}
					restoredFile.UpdatedAt = DateTime.UtcNow;
					daoRestoredFile.InsertOrUpdate(tx, restoredFile);

					Restore.Files.Add(restoredFile);
					//daoRestore.Update(tx, Backup);
				}
				tx.Commit();
			}

			// 2. Insert/Update `Restore` and its `RestorededFile`s into the database, also saving
			//	  the `RestorePlanFile`s instances that may have been changed by step 1.2. 
			using (ITransaction tx = session.BeginTransaction())
			{
				daoRestore.Update(tx, Restore);
				tx.Commit();
			}

			IsSaved = true;

			// 3. Create versioned files and remove files that won't belong to this restore.
			TransferSet.Files = GetFilesToTransfer(Restore, SuppliedFiles);
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

		#endregion
	}
}

