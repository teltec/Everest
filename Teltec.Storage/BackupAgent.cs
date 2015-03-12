using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teltec.Common.Extensions;
using Teltec.Storage.Agent;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public class BackupAgent<T> where T : IVersionedFile
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private IAsyncTransferAgent _Agent;

		public BackupAgent(IAsyncTransferAgent agent)
		{
			Results = new BackupResults();
			_Agent = agent;
			RegisterTransferEventHandlers();
		}

		private string _FilesAsDelimitedString;
		public string FilesAsDelimitedString(string delimiter, int maxLength, string trail)
		{
			if (_FilesAsDelimitedString == null)
				_FilesAsDelimitedString = Files.AsDelimitedString(p => p.Path,
					"No files to transfer", delimiter, maxLength, trail);
			return _FilesAsDelimitedString;
		}

		private LinkedList<T> _Files = new LinkedList<T>();
		public LinkedList<T> Files
		{
			get { return _Files;  }
			set
			{
				_Files = value;
				FilesChanged();
			}
		}

		protected void FilesChanged()
		{
			_EstimatedBackupSize = 0;
			_FilesAsDelimitedString = null;
			Results.Stats.Reset(_Files.Count);
		}

		// In Bytes
		private long _EstimatedBackupSize;
		public long EstimatedBackupSize
		{
			get
			{
				if (_EstimatedBackupSize == 0 && Files != null)
					_EstimatedBackupSize = Enumerable.Sum(Files, p => p.Size);
				return _EstimatedBackupSize;
			}
		}

		public BackupResults Results { get; private set; }

		protected void RegisterTransferEventHandlers()
		{
			_Agent.UploadFileStarted += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.Running -= 1;
				Results.OnStarted(this, e);
			};
			_Agent.UploadFileProgress += (object sender, TransferFileProgressArgs e) =>
			{
				Results.OnProgress(this, e);
			};
			_Agent.UploadFileCanceled += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Canceled += 1;
				Results.OnCanceled(this, e, ex);
			};
			_Agent.UploadFileFailed += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Failed += 1;
				Results.OnFailed(this, e, ex);
			};
			_Agent.UploadFileCompleted += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.Completed += 1;
				Results.OnCompleted(this, e);
			};
		}

		public void RemoveAllFiles()
		{
			Files.Clear();
			FilesChanged();
		}

		public async Task StartBackup()
		{
			Results.Stats.Reset(_Files.Count);
			
			_Agent.RenewCancellationToken();
			List<Task> activeTasks = new List<Task>();

			foreach (IVersionedFile file in Files)
			{
				Results.Stats.Pending -= 1;
				Results.Stats.Running += 1;

				Task task = _Agent.UploadVersionedFile(file.Path, file.Version);
				//	.ContinueWith((Task t) =>
				//{
				//	switch (t.Status)
				//	{
				//		case TaskStatus.Faulted:
				//			break;
				//		case TaskStatus.Canceled:
				//			break;
				//		case TaskStatus.RanToCompletion:
				//			break;
				//	}
				//});

				activeTasks.Add(task);
			}

			await Task.WhenAll(activeTasks.ToArray());
		}

		public void Cancel()
		{
			_Agent.CancelTransfers();
		}
	}
}
