using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Teltec.Storage;
using Teltec.Storage.Agent;

namespace App
{
	public class BackupAgent
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		LinkedList<FileInfo> _Files = new LinkedList<FileInfo>();
		IAsyncTransferAgent _Agent;

		private string _Sources;
		public string Sources(string delimiter, int maxLength, string trail)
		{
			if (_Sources == null)
			{
				_Sources = Enumerable.Aggregate<FileInfo, string>(
					_Files, "", (result, next) => result + next.FullName + delimiter);
				_Sources = _Sources.Substring(0, maxLength) + trail;
			}
			return _Sources;
		}

		private void InvalidateSources()
		{
			_Sources = null;
		}

		long _EstimatedLength = 0; // In bytes.
		public long EstimatedLength { get { return _EstimatedLength; } }

		public int FileCount { get { return _Files.Count; } }
		public BackupResults Results { get; private set; }

		public BackupAgent(IAsyncTransferAgent agent)
		{
			Results = new BackupResults();
			_Agent = agent;
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
			_Files.Clear();
			_EstimatedLength = 0;
			
			InvalidateSources();
		}

		public void AddFile(string path)
		{
			AddFile(new FileInfo(path));
		}

		public void AddFile(FileInfo file)
		{
			if (!file.Exists)
			{
				logger.Warn("File {0} does not exist", file.FullName);
				return;
			}

			_Files.AddLast(file);
			_EstimatedLength += file.Length;
			logger.Debug("File added: {0}, {1} bytes", file.FullName, file.Length);
			
			InvalidateSources();
		}

		public void AddDirectory(string path)
		{
			AddDirectory(new DirectoryInfo(path));
		}

		public void AddDirectory(DirectoryInfo directory)
		{
			if (!directory.Exists)
			{
				logger.Warn("Directory {0} does not exist", directory.FullName);
				return;
			}

			// Add all files from this directory.
			foreach (FileInfo file in directory.GetFiles())
				AddFile(file);

			// Add all sub-directories recursively.
			foreach (DirectoryInfo subdir in directory.GetDirectories())
				AddDirectory(subdir);
			
			InvalidateSources();
		}

		public async Task StartBackup()
		{
			Results.Stats.Reset(_Files.Count);
			
			_Agent.RenewCancellationToken();
			List<Task> activeTasks = new List<Task>();

			foreach (FileInfo file in _Files)
			{
				Results.Stats.Pending -= 1;
				Results.Stats.Running += 1;

				Task task = _Agent.UploadFile(file.FullName).ContinueWith((Task t) =>
				{
					//switch (t.Status)
					//{
					//	case TaskStatus.Faulted:
					//		break;
					//	case TaskStatus.Canceled:
					//		break;
					//	case TaskStatus.RanToCompletion:
					//		break;
					//}
				});
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
