using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Backup
{
	public sealed class RemoteBackupOperation : BackupOperation
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#region Constructors

		public RemoteBackupOperation(Models.BackupPlan plan)
			: this(plan, new BackupOperationOptions())
		{
		}

		public Models.Backup RemoteBackup { get { return Backup; } }

		public RemoteBackupOperation(Models.BackupPlan plan, BackupOperationOptions options)
			: base(options)
		{
			Backup = new Models.Backup(plan);
		}

		#endregion

		#region Transfer

		protected override Task<PathScanResults<string>> GetFilesToProcess(Models.Backup backup)
		{
			return null;
		}

		protected override Task DoVersionFiles(Models.Backup backup, LinkedList<string> filesToProcess)
		{
			return null;
		}

		#endregion

		#region Event handlers

		// ...

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
