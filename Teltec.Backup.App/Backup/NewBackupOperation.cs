using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teltec.Backup.App.Versioning;

namespace Teltec.Backup.App.Backup
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

		protected override LinkedList<string> GetFilesToProcess(Models.Backup backup)
		{
			// Scan files.
			DefaultPathScanner scanner = new DefaultPathScanner(backup.BackupPlan);

			scanner.FileAdded += (object sender, string file) =>
			{
				logger.Debug("ADDED: File {0}", file);
			};
			
			LinkedList<string> files = scanner.Scan();
			
			return files;
		}

		protected override Task DoVersionFiles(Models.Backup backup, LinkedList<string> filesToProcess)
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
					// NOP
				}
				this._isDisposed = true;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
