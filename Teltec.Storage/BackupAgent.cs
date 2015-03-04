using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Teltec.Common.Extensions;
using Teltec.Storage;
using Teltec.Storage.Agent;
using Teltec.Storage.Versioning;

namespace App
{
	public class BackupAgent
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		LinkedList<VersionedFileInfo> _Files = new LinkedList<VersionedFileInfo>();
		IAsyncTransferAgent _Agent;

		private string _CachedSourcesAsDelimitedString;
		public string SourcesAsDelimitedString(string delimiter, int maxLength, string trail)
		{
			if (_CachedSourcesAsDelimitedString == null)
				_CachedSourcesAsDelimitedString = _Files.AsDelimitedString(p => p.File.FullName,
					"No files to transfer", delimiter, maxLength, trail);
			return _CachedSourcesAsDelimitedString;
		}

		private void InvalidateCachedSourcesAsDelimitedString()
		{
			_CachedSourcesAsDelimitedString = null;
		}

		long _EstimatedLength = 0; // In bytes.
		public long EstimatedLength { get { return _EstimatedLength; } }

		public int FileCount { get { return _Files.Count; } }
		public BackupResults Results { get; private set; }

		public delegate void FileAddedHandler(object sender, VersionedFileInfo file);
		public FileAddedHandler FileAdded;

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

			InvalidateCachedSourcesAsDelimitedString();
		}

		public void AddFile(string path, IFileVersion version)
		{
			AddFile(new VersionedFileInfo(path, version));
		}

		public void AddFile(VersionedFileInfo file)
		{
			if (!file.File.Exists)
			{
				logger.Warn("File {0} does not exist", file.File.FullName);
				return;
			}

			_Files.AddLast(file);
			_EstimatedLength += file.File.Length;
			logger.Debug("File added: {0}, {1} bytes", file.File.FullName, file.File.Length);

			InvalidateCachedSourcesAsDelimitedString();

			if (FileAdded != null)
				FileAdded(this, file);
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
				AddFile(new VersionedFileInfo(file));

			// Add all sub-directories recursively.
			foreach (DirectoryInfo subdir in directory.GetDirectories())
				AddDirectory(subdir);

			InvalidateCachedSourcesAsDelimitedString();
		}

		public async Task StartBackup()
		{
			Results.Stats.Reset(_Files.Count);
			
			_Agent.RenewCancellationToken();
			List<Task> activeTasks = new List<Task>();

			foreach (VersionedFileInfo file in _Files)
			{
				Results.Stats.Pending -= 1;
				Results.Stats.Running += 1;

				Task task = _Agent.UploadVersionedFile(file.File.FullName, file.Version)
					.ContinueWith((Task t) =>
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
