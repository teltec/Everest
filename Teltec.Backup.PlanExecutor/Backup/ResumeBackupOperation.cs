using NLog;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Backup.Data.DAO;
using Teltec.Common.Extensions;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Backup
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

		private LinkedList<string> DoWork(Models.Backup backup, CancellationToken cancellationToken)
		{
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository();

			// Load pending `BackupedFiles` from `Backup`.
			IList<Models.BackupedFile> pendingFiles = daoBackupedFile.GetByBackupAndStatus(backup,
				TransferStatus.STOPPED, TransferStatus.RUNNING);

			cancellationToken.ThrowIfCancellationRequested();

			// Convert them to a list of paths.
			LinkedList<string> files = pendingFiles.ToLinkedList<string, Models.BackupedFile>(p => p.File.Path);

			return files;
		}

		protected override Task<LinkedList<string>> GetFilesToProcess(Models.Backup backup)
		{
			return ExecuteOnBackround(() =>
			{
				return DoWork(backup, CancellationTokenSource.Token);
			}, CancellationTokenSource.Token);
		}

		protected override Task DoVersionFiles(Models.Backup backup, LinkedList<string> filesToProcess)
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
