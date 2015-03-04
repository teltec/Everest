using Amazon.Runtime;
using App;
using NLog;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Teltec.Backup.App.DAO;
using Teltec.Backup.App.Versioning;
using Teltec.Common;
using Teltec.Storage;
using Teltec.Storage.Agent;
using Teltec.Storage.Implementations.S3;
using Teltec.Storage.Versioning;

namespace Teltec.Backup.App.Models
{
	public enum BackupStatus
	{
		Unknown		= 0,
		Started		= 1,
		Updated		= 2,
		Canceled	= 3,
		Failed		= 4,
		Finished	= 5,
	}

	public static class Extensions
	{
		public static bool IsEnded(this BackupStatus status)
		{
			return status == BackupStatus.Canceled
				|| status == BackupStatus.Failed
				|| status == BackupStatus.Finished;
		}
	}

	public class BackupEvent : EventArgs
	{
		public BackupStatus Status;
		public string Message;
	}

	public class BackupOptions
	{
		// ...
	}

	public class Backup : ObservableObject, IDisposable
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public delegate void UpdateEventHandler(object sender, BackupEvent e);
		public event UpdateEventHandler Updated;

		private BackupPlan _Plan;
		public BackupPlan Plan
		{
			get { return _Plan; }
			private set { _Plan = value; }
		}

		private DateTime _StartedAt;
		public DateTime StartedAt
		{
			get { return _StartedAt; }
			private set { _StartedAt = value; }
		}

		private DateTime _FinishedAt;
		public DateTime FinishedAt
		{
			get { return _FinishedAt; }
			private set { _FinishedAt = value; }
		}

		private bool _IsRunning = false;
		public bool IsRunning
		{
			get { return _IsRunning; }
			private set { _IsRunning = value; }
		}

		public string Sources
		{
			get
			{
				if (BackupAgent != null)
					return BackupAgent.SourcesAsDelimitedString(", ", 50, "...");
				else
					return Plan.SelectedSourcesAsDelimitedString(", ", 50, "...");
			}
		}

		//private BackupStatusInfo _StatusInfo;
		//public BackupStatusInfo StatusInfo
		//{
		//	get { return _StatusInfo; }
		//	private set { _StatusInfo = value; }
		//}

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
		
		public Backup(BackupPlan plan)
			: this(plan, new BackupOptions())
		{
		}

		public Backup(BackupPlan plan, BackupOptions options)
		{
			Plan = plan;
			Options = options;
			//StatusInfo = new BackupStatusInfo();
		}

		#region Transfer

		public Teltec.Storage.Monitor.TransferListControl TransferListControl; // IDisposable, but an external reference.
		BackupOptions Options;
		IAsyncTransferAgent TransferAgent; // IDisposable
		BackupAgent BackupAgent;

		public void Start(out BackupResults results)
		{
			Assert.IsFalse(IsRunning);
			Assert.AreEqual(Plan.StorageAccountType, EStorageAccountType.AmazonS3);

			IsRunning = true;

			AmazonS3AccountRepository dao = new AmazonS3AccountRepository();
			AmazonS3Account s3account = dao.Get(Plan.StorageAccount.Id);

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
			TransferAgent.RemoteRootDir = string.Format("backup-plan-{0}", Plan.Id);

			BackupAgent = new BackupAgent(TransferAgent);
			BackupAgent.Results.Monitor = TransferListControl;
			
			results = BackupAgent.Results;

			//
			// Start the backup.
			//
			DoBackup(BackupAgent, Plan, Options);
		}

		private async void DoBackup(BackupAgent agent, BackupPlan plan, BackupOptions options)
		{
			agent.Results.Failed += (object sender, TransferFileProgressArgs args, Exception ex) =>
			{
				var message = string.Format("Failed {0} - {1}", args.FilePath, ex != null ? ex.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				Updated(this, new BackupEvent { Status = BackupStatus.Updated, Message = message });
			};
			agent.Results.Canceled += (object sender, TransferFileProgressArgs args, Exception ex) =>
			{
				var message = string.Format("Canceled {0} - {1}", args.FilePath, ex != null ? ex.Message : "Unknown reason");
				Warn(message);
				//StatusInfo.Update(BackupStatusLevel.ERROR, message);
				Updated(this, new BackupEvent { Status = BackupStatus.Updated, Message = message });
			};
			agent.Results.Completed += (object sender, TransferFileProgressArgs args) =>
			{
				var message = string.Format("Completed {0}", args.FilePath);
				Info(message);
				Updated(this, new BackupEvent { Status = BackupStatus.Updated, Message = message });
			};
			agent.Results.Started += (object sender, TransferFileProgressArgs args) =>
			{
				var message = string.Format("Started {0}", args.FilePath);
				//Info(message);
				Updated(this, new BackupEvent { Status = BackupStatus.Updated, Message = message });
			};
			agent.Results.Progress += (object sender, TransferFileProgressArgs args) =>
			{
				//var message = string.Format("Progress {0}% {1} ({2}/{3} bytes)",
				//	args.PercentDone, args.FilePath, args.TransferredBytes, args.TotalBytes);
				//Info(message);
				Updated(this, new BackupEvent { Status = BackupStatus.Updated, Message = null });
			};
			agent.FileAdded += (object sender, VersionedFileInfo file) =>
			{
				Console.WriteLine("ADDED: File {0}, Version {1}", file.File, file.Version.ToString());
			};

			OnStart(agent);

			FileVersioner versioner = new FileVersioner(plan);
			Task versionerTask = versioner.AssembleVersion(agent);
			await versionerTask;

			Task transferTask = agent.StartBackup();
			await transferTask;

			OnFinish(agent);
		}

		public void Cancel()
		{
			Assert.IsTrue(IsRunning);
			DoCancel(BackupAgent);
		}

		private void DoCancel(BackupAgent agent)
		{
			BackupAgent.Cancel();
		}

		#endregion

		#region Event handlers

		public void OnStart(BackupAgent agent)
		{
			StartedAt = DateTime.UtcNow;
			
			//var message = string.Format("Estimate backup size: {0} files, {1}",
			//	BackupAgent.FileCount, FileSizeUtils.FileSizeToString(BackupAgent.EstimatedLength));
			var message = string.Format("Backup started at {0}", StartedAt);
			Info(message);
			//StatusInfo.Update(BackupStatusLevel.OK, message);
			Updated(this, new BackupEvent { Status = BackupStatus.Started, Message = message });
		}

		public void OnFinish(BackupAgent agent)
		{
			FinishedAt = DateTime.UtcNow;

			IsRunning = false;
			//btnRun.Text = "Run";

			BackupResults.Statistics stats = BackupAgent.Results.Stats;

			var message = string.Format(
				"Backup finished at {0}! Stats: {1} completed, {2} failed, {3} canceled, {4} pending, {5} running",
				FinishedAt, stats.Completed, stats.Failed, stats.Canceled, stats.Pending, stats.Running);
			Info(message);
			//StatusInfo.Update(BackupStatusLevel.OK, message);

			switch (agent.Results.OverallStatus)
			{
				default: throw new InvalidOperationException("Unexpected OverallStatus");
				case BackupResults.Status.CANCELED:
					Updated(this, new BackupEvent { Status = BackupStatus.Canceled, Message = message });
					break;
				case BackupResults.Status.FAILED:
					Updated(this, new BackupEvent { Status = BackupStatus.Failed, Message = message });
					break;
				case BackupResults.Status.COMPLETED:
					Updated(this, new BackupEvent { Status = BackupStatus.Finished, Message = message });
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
		protected virtual void Dispose(bool disposing)
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
