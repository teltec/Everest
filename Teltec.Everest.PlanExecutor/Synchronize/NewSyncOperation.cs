using NLog;
using System;
using Models = Teltec.Everest.Data.Models;

namespace Teltec.Everest.PlanExecutor.Synchronize
{
	public sealed class NewSyncOperation : SyncOperation
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#region Constructors

		public NewSyncOperation(Models.Synchronization sync)
			: this(sync, new SyncOperationOptions())
		{
		}

		public NewSyncOperation(Models.Synchronization sync, SyncOperationOptions options)
			: base(options)
		{
			Synchronization = sync;
		}

		#endregion

		#region Report

		public override void SendReport()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Sync

		// ...

		#endregion

		#region Event handlers

		public override void OnStart(CustomSynchronizationAgent agent, Models.Synchronization sync)
		{
			base.OnStart(agent, sync);

			_daoSynchronization.Insert(sync);

			var message = string.Format("Synchronization started at {0}", StartedAt);
			Info(message);
			//StatusInfo.Update(SyncStatusLevel.OK, message);

			OnUpdate(new SyncOperationEvent { Status = SyncOperationStatus.Started, Message = message });
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
