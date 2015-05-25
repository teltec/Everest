using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teltec.Backup.Data.Versioning;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Restore
{
	public sealed class RemoteRestoreOperation : RestoreOperation
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#region Constructors

		public RemoteRestoreOperation(Models.RestorePlan plan)
			: this(plan, new RestoreOperationOptions())
		{
		}

		public Models.Restore RemoteRestore { get { return Restore; } }

		public RemoteRestoreOperation(Models.RestorePlan plan, RestoreOperationOptions options)
			: base(options)
		{
			Restore = new Models.Restore(plan);
		}

		#endregion

		#region Transfer

		protected override Task<LinkedList<CustomVersionedFile>> GetFilesToProcess(Models.Restore restore)
		{
			return null;
		}
		
		protected override Task DoVersionFiles(Models.Restore restore, LinkedList<CustomVersionedFile> files)
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
