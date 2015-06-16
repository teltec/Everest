using NLog;
using NUnit.Framework;
using System;
using Teltec.Backup.Data.DAO;
using Teltec.Backup.PlanExecutor.Backup;
using Teltec.Backup.PlanExecutor.Versioning;
using Teltec.Storage;
using Models = Teltec.Backup.Data.Models;

namespace Teltec.Backup.PlanExecutor.Synchronize
{
	public enum SyncOperationStatus : byte
	{
		Unknown = 0,
		Started = 1,
		Resumed = 2,
		ScanningFilesStarted = 3,
		ScanningFilesFinished = 4,
		ProcessingFilesStarted = 5,
		ProcessingFilesFinished = 6,
		Updated = 7,
		Canceled = 8,
		Failed = 9,
		Finished = 10,
	}

	public static class Extensions
	{
		public static bool IsEnded(this SyncOperationStatus status)
		{
			return status == SyncOperationStatus.Canceled
				|| status == SyncOperationStatus.Failed
				|| status == SyncOperationStatus.Finished;
		}
	}

	public sealed class SyncOperationEvent : EventArgs
	{
		public SyncOperationStatus Status;
		public string Message;
	}

	public sealed class SyncOperationOptions
	{
		// ...
	}

	public abstract class SyncOperation : BaseOperation
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected readonly BackupRepository _daoBackup = new BackupRepository();

		protected Models.Synchronization Synchronization;

		#region Properties

		public override Int32? OperationId
		{
			get { return Synchronization.Id; }
		}

		public delegate void UpdateEventHandler(object sender, SyncOperationEvent e);
		public event UpdateEventHandler Updated;

		public DateTime? StartedAt
		{
			get { Assert.IsNotNull(Synchronization); return Synchronization.StartedAt; }
		}

		public DateTime? FinishedAt
		{
			get { Assert.IsNotNull(Synchronization); return Synchronization.FinishedAt; }
		}

		//private string _RootDir;
		//public string RootDir
		//{
		//	get
		//	{
		//		if (_RootDir == null)
		//			_RootDir = string.Format("backup-{0}", Plan.Id);
		//		return _RootDir;
		//	}
		//}

		#endregion

		#region Constructors

		public SyncOperation(SyncOperationOptions options)
		{
			Options = options;
		}

		#endregion

		#region Transfer

		protected IncrementalFileVersioner Versioner; // IDisposable
		protected SyncOperationOptions Options;
		protected CustomBackupAgent BackupAgent;

		public override void Start(out TransferResults results)
		{
			Assert.IsFalse(IsRunning);
			Assert.IsNotNull(Synchronization);
			results = new TransferResults();
/*
			Assert.IsNotNull(Backup.BackupPlan);
			Assert.IsNotNull(Backup.BackupPlan.StorageAccount);
			Assert.AreEqual(Models.EStorageAccountType.AmazonS3, Backup.BackupPlan.StorageAccountType);

			AmazonS3AccountRepository dao = new AmazonS3AccountRepository();
			Models.AmazonS3Account s3account = dao.GetForReadOnly(Backup.BackupPlan.StorageAccount.Id);

			//
			// Dispose and recycle previous objects, if needed.
			//
			if (TransferAgent != null)
				TransferAgent.Dispose();
			if (TransferListControl != null)
				TransferListControl.ClearTransfers();
			if (Versioner != null)
				Versioner.Dispose();

			//
			// Setup agents.
			//
			AWSCredentials awsCredentials = new BasicAWSCredentials(s3account.AccessKey, s3account.SecretKey);
			TransferAgent = new S3AsyncTransferAgent(awsCredentials, s3account.BucketName);
			TransferAgent.RemoteRootDir = string.Format("backup-plan-{0}", Backup.BackupPlan.Id);

			BackupAgent = new CustomBackupAgent(TransferAgent);
			BackupAgent.Results.Monitor = TransferListControl;

			Versioner = new IncrementalFileVersioner(CancellationTokenSource.Token);

			RegisterResultsEventHandlers(Synchronization, BackupAgent.Results);

			results = BackupAgent.Results;

			//
			// Start the sync.
			//
			DoSync(BackupAgent, Backup, Options);
 */
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
					BackupAgent = null;

					if (Versioner != null)
					{
						Versioner.Dispose();
						Versioner = null;
					}
				}
				this._isDisposed = true;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
