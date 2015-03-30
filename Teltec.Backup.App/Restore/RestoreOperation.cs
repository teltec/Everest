using Amazon.Runtime;
using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Versioning;
using Teltec.Storage;
using Teltec.Storage.Agent;
using Teltec.Storage.Implementations.S3;
using Teltec.Storage.Utils;

namespace Teltec.Backup.App.Restore
{
	public enum RestoreOperationStatus
	{
		Unknown = 0,
		Started = 1,
		Resumed = 2,
		ProcessingFilesStarted = 3,
		ProcessingFilesFinished = 4,
		Updated = 5,
		Canceled = 6,
		Failed = 7,
		Finished = 8,
	}

	public static class Extensions
	{
		public static bool IsEnded(this RestoreOperationStatus status)
		{
			return status == RestoreOperationStatus.Canceled
				|| status == RestoreOperationStatus.Failed
				|| status == RestoreOperationStatus.Finished;
		}
	}

	public sealed class RestoreOperationEvent : EventArgs
	{
		public RestoreOperationStatus Status;
		public string Message;
	}

	public sealed class RestoreOperationOptions
	{
		// ...
	}

	public class RestoreOperation : BaseOperation
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected readonly RestoreRepository _daoRestore = new RestoreRepository();

		protected Models.Restore Restore;

		#region Properties

		public delegate void UpdateEventHandler(object sender, RestoreOperationEvent e);
		public event UpdateEventHandler Updated;

		public bool IsRunning { get; protected set; }

		public DateTime? StartedAt
		{
			get { Assert.IsNotNull(Restore); return Restore.StartedAt; }
		}

		public DateTime? FinishedAt
		{
			get { Assert.IsNotNull(Restore); return Restore.FinishedAt; }
		}

		#endregion

		#region Constructors

		public RestoreOperation(RestoreOperationOptions options)
		{
			Options = options;
		}

		#endregion

		#region Transfer

		public Teltec.Storage.Monitor.TransferListControl TransferListControl; // IDisposable, but an external reference.
		protected RestoreOperationOptions Options;
		protected IAsyncTransferAgent TransferAgent; // IDisposable
		protected CustomRestoreAgent RestoreAgent;
		protected IncrementalFileVersioner Versioner; // IDisposable

		public void Start(out TransferResults results)
		{
			// TODO: Implement.
			throw new NotImplementedException();
		}

		protected void RegisterResultsEventHandlers(Models.Restore restore, TransferResults results)
		{
			// TODO: Implement.
			throw new NotImplementedException();
		}

		//private void RestoreFile(string path, IFileVersion version)
		//{
		//	// ...
		//}

		protected LinkedList<string> GetFilesToProcess(Models.Restore restore)
		{
			LinkedList<string> files = null;
			
			return files;
		}

		protected Task DoVersionFiles(Models.Restore restore, LinkedList<string> filesToProcess)
		{
			//return Versioner.NewVersion(restore, filesToProcess);
			throw new NotImplementedException();
		}

		protected async void DoRestore(CustomRestoreAgent agent, Models.Restore restore, RestoreOperationOptions options)
		{
			OnStart(agent, restore);

			// Version files.
			LinkedList<string> filesToProcess = GetFilesToProcess(restore);
			Task versionerTask = DoVersionFiles(restore, filesToProcess);

			{
				var message = string.Format("Processing files started.");
				Info(message);
				//StatusInfo.Update(BackupStatusLevel.INFO, message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.ProcessingFilesStarted, Message = message });
			}

			try
			{
				await versionerTask;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception Message: " + ex.Message);
			}

			if (versionerTask.IsFaulted || versionerTask.IsCanceled)
			{
				Versioner.Undo();
				OnFailure(agent, restore, versionerTask.Exception);
				return;
			}

			// IMPORTANT: Must happen before any attempt to get `FileVersioner.FilesToBackup`.
			Versioner.Save();

			agent.Files = Versioner.FilesToTransfer;

			{
				var message = string.Format("Processing files finished.");
				Info(message);
				//StatusInfo.Update(BackupStatusLevel.INFO, message);
				OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.ProcessingFilesFinished, Message = message });
			}
			{
				var message = string.Format("Estimate restore size: {0} files, {1}",
					agent.Files.Count(), FileSizeUtils.FileSizeToString(agent.EstimatedTransferSize));
				Info(message);
			}

			Task transferTask = agent.Start();
			await transferTask;

			OnFinish(agent, restore);
		}

		public void Cancel()
		{
			Assert.IsTrue(IsRunning);
			DoCancel(RestoreAgent);
		}

		protected void DoCancel(CustomRestoreAgent agent)
		{
			agent.Cancel();
		}

		#endregion

		#region Event handlers

		public virtual void OnStart(CustomRestoreAgent agent, Models.Restore restore)
		{
			IsRunning = true;

			restore.DidStart();
		}

		protected void OnUpdate(RestoreOperationEvent e)
		{
			if (Updated != null)
				Updated(this, e);
		}

		public void OnFailure(CustomRestoreAgent agent, Models.Restore restore, Exception exception)
		{
			IsRunning = false;

			var message = string.Format("Restore failed: {0}", exception.Message);
			Error(message);
			//StatusInfo.Update(RestoreStatusLevel.ERROR, message);

			restore.DidFail();
			_daoRestore.Update(restore);

			OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Failed, Message = message });
		}

		public void OnFinish(CustomRestoreAgent agent, Models.Restore restore)
		{
			IsRunning = false;

			TransferResults.Statistics stats = agent.Results.Stats;

			var message = string.Format(
				"Restore finished! Stats: {0} completed, {1} failed, {2} canceled, {3} pending, {4} running",
				stats.Completed, stats.Failed, stats.Canceled, stats.Pending, stats.Running);
			Info(message);
			//StatusInfo.Update(RestoreStatusLevel.OK, message);

			switch (agent.Results.OverallStatus)
			//switch (backup.Status)
			{
				default: throw new InvalidOperationException("Unexpected TransferStatus");
				case TransferStatus.CANCELED:
					restore.WasCanceled();
					_daoRestore.Update(restore);
					OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Canceled, Message = message });
					break;
				case TransferStatus.FAILED:
					restore.DidFail();
					_daoRestore.Update(restore);
					OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Failed, Message = message });
					break;
				case TransferStatus.COMPLETED:
					restore.DidComplete();
					_daoRestore.Update(restore);
					OnUpdate(new RestoreOperationEvent { Status = RestoreOperationStatus.Finished, Message = message });
					break;
			}
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
					RestoreAgent = null;

					if (Versioner != null)
					{
						Versioner.Dispose();
						Versioner = null;
					}

					if (TransferAgent != null)
					{
						TransferAgent.Dispose();
						TransferAgent = null;
					}
				}
				this._isDisposed = true;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
