/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Common.Extensions;
using Teltec.Everest.Data.DAO;
using Teltec.Everest.PlanExecutor.Versioning;
using Teltec.Storage;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.PlanExecutor.Backup
{
	public sealed class ResumeBackupOperation : BackupOperation
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#region Constructors

		public ResumeBackupOperation(Models.Backup backup)
			: this(backup, new BackupOperationOptions())
		{
		}

		public ResumeBackupOperation(Models.Backup backup, BackupOperationOptions options)
			: base(options)
		{
			Assert.IsNotNull(backup);
			Assert.AreEqual(TransferStatus.RUNNING, backup.Status);

			Backup = backup;
		}

		#endregion

		#region Transfer

		private PathScanResults<string> DoWork(Models.Backup backup, CancellationToken cancellationToken)
		{
			PathScanResults<string> results = new PathScanResults<string>();

			BackupedFileRepository daoBackupedFile = new BackupedFileRepository();

			// Load pending `BackupedFiles` from `Backup`.
			IList<Models.BackupedFile> pendingFiles = daoBackupedFile.GetByBackupAndStatus(backup,
				// 1. We don't want to resume a file that FAILED.
				// 2. We don't want to resume a file that was CANCELED.
				// Theoretically, a CANCELED file should never be present in a backup that is still RUNNING,
				// unless the backup was being CANCELED, and the PlanExecutor was terminated by some reason
				// after it updated the files as CANCELED but before it got the chance to update the backup
				// itself as CANCELED.
				TransferStatus.STOPPED, // The transfer didn't begin.
				TransferStatus.RUNNING  // The transfer did begin but did not complete.
			);

			cancellationToken.ThrowIfCancellationRequested();

			// Convert them to a list of paths.
			results.Files = pendingFiles.ToLinkedList<string, Models.BackupedFile>(p => p.File.Path);

			return results;
		}

		protected override Task<PathScanResults<string>> GetFilesToProcess(Models.Backup backup)
		{
			return ExecuteOnBackround(() =>
			{
				return DoWork(backup, CancellationTokenSource.Token);
			}, CancellationTokenSource.Token);
		}

		protected override Task<FileVersionerResults> DoVersionFiles(Models.Backup backup, LinkedList<string> filesToProcess)
		{
			return Versioner.ResumeVersion(backup, filesToProcess);
		}

		#endregion

		#region Event handlers

		public override void OnStart(CustomBackupAgent agent, Models.Backup backup)
		{
			base.OnStart(agent, backup);

			_daoBackup.Update(backup);

			var message = string.Format("Backup resumed at {0}", StartedAt);
			Info(message);
			//StatusInfo.Update(BackupStatusLevel.OK, message);

			OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Resumed, Message = message });
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
					// Nop
				}
				this._isDisposed = true;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
