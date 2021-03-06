/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Everest.PlanExecutor.Versioning;
using Teltec.Storage;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.PlanExecutor.Backup
{
	public sealed class NewBackupOperation : BackupOperation
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#region Constructors

		public NewBackupOperation(Models.BackupPlan plan)
			: this(plan, new BackupOperationOptions())
		{
		}

		public NewBackupOperation(Models.BackupPlan plan, BackupOperationOptions options)
			: base(options)
		{
			Backup = new Models.Backup(plan);
		}

		#endregion

		#region Transfer

		private PathScanResults<string> DoWork(Models.Backup backup, CancellationToken cancellationToken)
		{
			// Scan files.
			DefaultPathScanner scanner = new DefaultPathScanner(backup.BackupPlan, cancellationToken);

#if DEBUG
			scanner.FileAdded += (object sender, string file) =>
			{
				logger.Debug("ADDED: File {0}", file);
			};
			scanner.EntryScanFailed += (object sender, string path, string message, Exception ex) =>
			{
				logger.Debug("FAILED: {0} - Reason: {1}", path, message);
			};
#endif

			scanner.Scan();

			return scanner.Results;
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
			return Versioner.NewVersion(backup, filesToProcess);
		}

		#endregion

		#region Event handlers

		public override void OnStart(CustomBackupAgent agent, Models.Backup backup)
		{
			base.OnStart(agent, backup);

			_daoBackup.Insert(backup);

			var message = string.Format("Backup started at {0}", StartedAt);
			Info(message);
			//StatusInfo.Update(BackupStatusLevel.OK, message);

			OnUpdate(new BackupOperationEvent { Status = BackupOperationStatus.Started, Message = message });
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
