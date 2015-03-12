using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Versioning;
using Teltec.Storage;
using Teltec.Storage.Utils;
using Teltec.Common.Extensions;
using System.Collections;
using System.Linq;

namespace Teltec.Backup.App
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
			Assert.AreEqual(backup.Status, BackupStatus.RUNNING);

			Backup = backup;
		}

		#endregion

		#region Transfer

		protected override LinkedList<string> GetFilesToProcess(Models.Backup backup)
		{
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository();

			// Load pending `BackupedFiles` from `Backup`.
			IList<Models.BackupedFile> pendingFiles = daoBackupedFile.GetByBackupAndStatus(backup, BackupStatus.RUNNING);

			// Convert them to a list of paths.
			LinkedList<string> files = pendingFiles.ToLinkedList<string, Models.BackupedFile>(p => p.File.Path);

			return files;
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
					// NOP
				}
				this._isDisposed = true;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
