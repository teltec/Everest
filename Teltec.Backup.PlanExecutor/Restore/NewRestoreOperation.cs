using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Backup.Data.Versioning;
using Teltec.Backup.PlanExecutor.Versioning;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Restore
{
	public sealed class NewRestoreOperation : RestoreOperation
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#region Constructors

		public NewRestoreOperation(Models.RestorePlan plan)
			: this(plan, new RestoreOperationOptions())
		{
		}

		public NewRestoreOperation(Models.RestorePlan plan, RestoreOperationOptions options)
			: base(options)
		{
			Restore = new Models.Restore(plan);
		}

		#endregion

		#region Transfer

		private PathScanResults<CustomVersionedFile> DoWork(Models.Restore restore, CancellationToken cancellationToken)
		{
			// Scan files.
			DefaultRestoreScanner scanner = new DefaultRestoreScanner(restore.RestorePlan, cancellationToken);

#if DEBUG
			scanner.FileAdded += (object sender, CustomVersionedFile file) =>
			{
				logger.Debug("ADDED: File {0} @ {1}", file.Path, file.Version);
			};
			scanner.EntryScanFailed += (object sender, string path, string message, Exception ex) =>
			{
				logger.Debug("FAILED: {0} - Reason: {1}", path, message);
			};
#endif

			scanner.Scan();

			return scanner.Results;
		}

		protected override Task<PathScanResults<CustomVersionedFile>> GetFilesToProcess(Models.Restore restore)
		{
			return ExecuteOnBackround(() =>
			{
				return DoWork(restore, CancellationTokenSource.Token);
			}, CancellationTokenSource.Token);
		}

		protected override Task DoVersionFiles(Models.Restore restore, LinkedList<CustomVersionedFile> files)
		{
			return Versioner.NewRestore(restore, files);
		}

		#endregion

		#region Event handlers

		public override void OnStart(CustomRestoreAgent agent, Models.Restore restore)
		{
			base.OnStart(agent, restore);

			_daoRestore.Insert(restore);

			var message = string.Format("Restore started at {0}", StartedAt);
			Info(message);
			//StatusInfo.Update(BackupStatusLevel.OK, message);

			OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Started, Message = message });
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
