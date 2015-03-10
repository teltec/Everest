using Amazon.Runtime;
using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Versioning;
using Teltec.Common;
using Teltec.Storage;
using Teltec.Storage.Agent;
using Teltec.Storage.Implementations.S3;

namespace Teltec.Backup.App
{
	public enum BackupOperationStatus
	{
		Unknown					= 0,
		Started					= 1,
		ProcessingFilesStarted	= 2,
		ProcessingFilesFinished = 3,
		Updated					= 4,
		Canceled				= 5,
		Failed					= 6,
		Finished				= 7,
	}

	public static class Extensions
	{
		public static bool IsEnded(this BackupOperationStatus status)
		{
			return status == BackupOperationStatus.Canceled
				|| status == BackupOperationStatus.Failed
				|| status == BackupOperationStatus.Finished;
		}
	}

	public sealed class BackupOperationEvent : EventArgs
	{
		public BackupOperationStatus Status;
		public string Message;
	}

	public sealed class BackupOperationOptions
	{
		// ...
	}

	public sealed class BackupOperation : ObservableObject, IDisposable
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		private readonly BackupRepository _daoBackup = new BackupRepository();

		public delegate void UpdateEventHandler(object sender, BackupOperationEvent e);
		public event UpdateEventHandler Updated;

		private Models.Backup Backup;

		private bool _IsRunning = false;
		public bool IsRunning
		{
			get { return _IsRunning; }
			private set { _IsRunning = value; }
		}

		public DateTime? StartedAt
		{
			get { Assert.IsNotNull(Backup); return Backup.StartedAt; }
		}

		public DateTime? FinishedAt
		{
			get { Assert.IsNotNull(Backup); return Backup.FinishedAt; }
		}

		public string Sources
		{
			get
			{
				Debug.Assert(BackupAgent != null || Backup != null);
				const string delimiter = ", ", trail = "...";
				const int maxLength = 50;
				return BackupAgent != null
					? BackupAgent.FilesAsDelimitedString(delimiter, maxLength, trail)
					: Backup.BackupPlan.SelectedSourcesAsDelimitedString(delimiter, maxLength, trail);
			}
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

		public BackupOperation(Models.BackupPlan plan)
			: this(plan, new BackupOperationOptions())
		{
		}

		public BackupOperation(Models.BackupPlan plan, BackupOperationOptions options)
		{
			Backup = new Models.Backup(plan);
			Options = options;
		}

		#region Transfer

		public Teltec.Storage.Monitor.TransferListControl TransferListControl; // IDisposable, but an external reference.
		BackupOperationOptions Options;
		IAsyncTransferAgent TransferAgent; // IDisposable
		CustomBackupAgent BackupAgent;

		public void Start(out BackupResults results)
		{
			Assert.IsFalse(IsRunning);
			Assert.IsNotNull(Backup);
			Assert.IsNotNull(Backup.BackupPlan);
			Assert.IsNotNull(Backup.BackupPlan.StorageAccount);
			Assert.AreEqual(Backup.BackupPlan.StorageAccountType, Models.EStorageAccountType.AmazonS3);

			IsRunning = true;

			AmazonS3AccountRepository dao = new AmazonS3AccountRepository();
			Models.AmazonS3Account s3account = dao.Get(Backup.BackupPlan.StorageAccount.Id);

			//
			// Dispose and recycle previous objects, if needed.
			//
			if (TransferAgent != null)
				TransferAgent.Dispose();
			if (TransferListControl != null)
				TransferListControl.ClearTransfers();

			//
			// Setup agents.
			//
			AWSCredentials awsCredentials = new BasicAWSCredentials(s3account.AccessKey, s3account.SecretKey);
			TransferAgent = new S3AsyncTransferAgent(awsCredentials, s3account.BucketName);
			TransferAgent.RemoteRootDir = string.Format("backup-plan-{0}", Backup.BackupPlan.Id);

			BackupAgent = new CustomBackupAgent(TransferAgent);
			BackupAgent.Results.Monitor = TransferListControl;
			
			results = BackupAgent.Results;

			//
			// Start the backup.
			//
			DoBackup(BackupAgent, Backup, Options);
		}

		private async void DoBackup(CustomBackupAgent agent, Models.Backup backup, BackupOperationOptions options)
		{
			BackupedFileRepository daoBackupedFile = new BackupedFileRepository();
			agent.Results.Failed += (object sender, TransferFileProgressArgs args, Exception ex) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.Status = BackupStatus.FAILED;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Failed {0} - {1}", args.FilePath, ex != null ? ex.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message });
			};
			agent.Results.Canceled += (object sender, TransferFileProgressArgs args, Exception ex) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.Status = BackupStatus.CANCELED;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Canceled {0} - {1}", args.FilePath, ex != null ? ex.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message });
			};
			agent.Results.Completed += (object sender, TransferFileProgressArgs args) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.Status = BackupStatus.COMPLETED;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Completed {0}", args.FilePath);
				Info(message);
				Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message });
			};
			agent.Results.Started += (object sender, TransferFileProgressArgs args) =>
			{
				Models.BackupedFile backupedFile = daoBackupedFile.GetByBackupAndPath(backup, args.FilePath);
				backupedFile.Status = BackupStatus.RUNNING;
				backupedFile.UpdatedAt = DateTime.UtcNow;
				daoBackupedFile.Update(backupedFile);

				var message = string.Format("Started {0}", args.FilePath);
				//Info(message);
				Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = message });
			};
			agent.Results.Progress += (object sender, TransferFileProgressArgs args) =>
			{
				//var message = string.Format("Progress {0}% {1} ({2}/{3} bytes)",
				//	args.PercentDone, args.FilePath, args.TransferredBytes, args.TotalBytes);
				//Info(message);
				Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Updated, Message = null });
			};

			OnStart(agent, backup);

			// Scan files.
			DefaultPathScanner scanner = new DefaultPathScanner(backup.BackupPlan);
			scanner.FileAdded += (object sender, CustomVersionedFile file) =>
			{
				Console.WriteLine("ADDED: File {0}", file.Path);
			};
			LinkedList<CustomVersionedFile> files = scanner.Scan();

			// Version files.
			FileVersioner versioner = new FileVersioner();
			Task versionerTask = versioner.NewVersion(backup, files);

			{
				var message = string.Format("Processing files started.");
				Info(message);
				//StatusInfo.Update(BackupStatusLevel.INFO, message);
				Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.ProcessingFilesStarted, Message = message });
			}

			try
			{
				await versionerTask;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception Message: " + ex.Message);
			}

			agent.Files = versioner.FilesToBackup;

			{
				var message = string.Format("Processing files finished.");
				Info(message);
				//StatusInfo.Update(BackupStatusLevel.INFO, message);
				Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.ProcessingFilesFinished, Message = message });
			}

			//Task theTask = versionerTask;
			//Debug.WriteLine("Task IsCanceled: " + theTask.IsCanceled);
			//Debug.WriteLine("Task IsFaulted:  " + theTask.IsFaulted);
			//if (theTask.Exception != null)
			//{
			//	Debug.WriteLine("Task Exception Message: "
			//		+ theTask.Exception.Message);
			//	Debug.WriteLine("Task Inner Exception Message: "
			//		+ theTask.Exception.InnerException.Message);
			//}

			if (versionerTask.IsFaulted || versionerTask.IsCanceled)
			{
				versioner.Undo();
				OnFailure(agent, backup, versionerTask.Exception);
				return;
			}
			else
			{
				versioner.Save();
			}
			
			Task transferTask = agent.StartBackup();
			await transferTask;

			OnFinish(agent, backup);
		}

		public void Cancel()
		{
			Assert.IsTrue(IsRunning);
			DoCancel(BackupAgent);
		}

		private void DoCancel(CustomBackupAgent agent)
		{
			BackupAgent.Cancel();
		}

		#endregion

		#region Event handlers

		public void OnStart(CustomBackupAgent agent, Models.Backup backup)
		{
			backup.DidStart();
			_daoBackup.Insert(backup);
			
			//var message = string.Format("Estimate backup size: {0} files, {1}",
			//	BackupAgent.FileCount, FileSizeUtils.FileSizeToString(BackupAgent.EstimatedLength));
			var message = string.Format("Backup started at {0}", StartedAt);
			Info(message);
			//StatusInfo.Update(BackupStatusLevel.OK, message);
			Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Started, Message = message });
		}

		public void OnFailure(CustomBackupAgent agent, Models.Backup backup, Exception exception)
		{
			IsRunning = false;

			var message = string.Format("Backup failed: {0}", exception.Message);
			Error(message);
			//StatusInfo.Update(BackupStatusLevel.ERROR, message);

			backup.DidFail();
			_daoBackup.Update(Backup);

			Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Failed, Message = message });
		}

		public void OnFinish(CustomBackupAgent agent, Models.Backup backup)
		{
			IsRunning = false;

			BackupResults.Statistics stats = agent.Results.Stats;

			var message = string.Format(
				"Backup finished! Stats: {0} completed, {1} failed, {2} canceled, {3} pending, {4} running",
				stats.Completed, stats.Failed, stats.Canceled, stats.Pending, stats.Running);
			Info(message);
			//StatusInfo.Update(BackupStatusLevel.OK, message);

			switch (agent.Results.OverallStatus)
			{
				default: throw new InvalidOperationException("Unexpected OverallStatus");
				case BackupStatus.CANCELED:
					backup.WasCanceled();
					_daoBackup.Update(backup);
					Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Canceled, Message = message });
					break;
				case BackupStatus.FAILED:
					backup.DidFail();
					_daoBackup.Update(backup);
					Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Failed, Message = message });
					break;
				case BackupStatus.COMPLETED:
					backup.DidComplete();
					_daoBackup.Update(backup);
					Updated(this, new BackupOperationEvent { Status = BackupOperationStatus.Finished, Message = message });
					break;
			}
		}

		#endregion

		#region Logging

		public System.Diagnostics.EventLog EventLog;

		private void Log(System.Diagnostics.EventLogEntryType type, string format, params object[] args)
		{
			string message = string.Format(format, args);
			Console.WriteLine(message);
			if (EventLog != null)
				EventLog.WriteEntry(message, type);
		}

		private void Warn(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Warning, format, args);
		}

		private void Error(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Error, format, args);
		}

		private void Info(string format, params object[] args)
		{
			Log(System.Diagnostics.EventLogEntryType.Information, format, args);
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
		private void Dispose(bool disposing)
		{
			if (!this._isDisposed)
			{
				if (disposing && _shouldDispose)
				{
					BackupAgent = null;
					if (TransferAgent != null)
					{
						TransferAgent.Dispose();
						TransferAgent = null;
					}
				}
				this._isDisposed = true;
			}
		}

		/// <summary>
		/// Disposes of all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
