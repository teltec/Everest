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
	public abstract class AbstractAgent<TFile> where TFile : IVersionedFile
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected IAsyncTransferAgent TransferAgent { get; private set; }

		public AbstractAgent(IAsyncTransferAgent agent)
		{
			Results = new TransferResults();
			TransferAgent = agent;
		}

		private string _FilesAsDelimitedString;
		public string FilesAsDelimitedString(string delimiter, int maxLength, string trail)
		{
			if (_FilesAsDelimitedString == null)
				_FilesAsDelimitedString = Files.AsDelimitedString(p => p.Path,
					"No files to transfer", delimiter, maxLength, trail);
			return _FilesAsDelimitedString;
		}

		private IEnumerable<TFile> _Files = new List<TFile>();
		public IEnumerable<TFile> Files
		{
			get { return _Files; }
			set
			{
				_Files = value;
				FilesChanged();
			}
		}

		protected void FilesChanged()
		{
			_EstimatedTransferSize = 0;
			_FilesAsDelimitedString = null;
			Results.Stats.Reset(_Files.Count());
		}

		// In Bytes
		private long _EstimatedTransferSize;
		public long EstimatedTransferSize
		{
			get
			{
				if (_EstimatedTransferSize == 0 && Files != null)
					_EstimatedTransferSize = Enumerable.Sum(Files, p => p.Size);
				return _EstimatedTransferSize;
			}
		}

		public TransferResults Results { get; private set; }

		protected void RegisterUploadEventHandlers()
		{
			TransferAgent.UploadFileStarted += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.Running -= 1;
				Results.OnStarted(this, e);
			};
			TransferAgent.UploadFileProgress += (object sender, TransferFileProgressArgs e) =>
			{
				Results.OnProgress(this, e);
			};
			TransferAgent.UploadFileCanceled += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Canceled += 1;
				Results.Stats.BytesCanceled += e.TotalBytes;
				Results.OnCanceled(this, e, ex);
			};
			TransferAgent.UploadFileFailed += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Failed += 1;
				Results.Stats.BytesFailed += e.TotalBytes;
				Results.OnFailed(this, e, ex);
			};
			TransferAgent.UploadFileCompleted += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.Completed += 1;
				Results.Stats.BytesCompleted += e.TotalBytes;
				Results.OnCompleted(this, e);
			};
		}

		protected void RegisterDownloadEventHandlers()
		{
			TransferAgent.DownloadFileStarted += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.Running -= 1;
				Results.OnStarted(this, e);
			};
			TransferAgent.DownloadFileProgress += (object sender, TransferFileProgressArgs e) =>
			{
				Results.OnProgress(this, e);
			};
			TransferAgent.DownloadFileCanceled += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Canceled += 1;
				Results.Stats.BytesCanceled += e.TotalBytes;
				Results.OnCanceled(this, e, ex);
			};
			TransferAgent.DownloadFileFailed += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Failed += 1;
				Results.Stats.BytesFailed += e.TotalBytes;
				Results.OnFailed(this, e, ex);
			};
			TransferAgent.DownloadFileCompleted += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.Completed += 1;
				Results.Stats.BytesCompleted += e.TotalBytes;
				Results.OnCompleted(this, e);
			};
		}

		public void RemoveAllFiles()
		{
			Files = new List<TFile>();
			FilesChanged();
		}

		public void Cancel()
		{
			TransferAgent.CancelTransfers();
		}

		public async Task Start()
		{
			Results.Stats.Reset(Files.Count());

			TransferAgent.RenewCancellationToken();
			List<Task> activeTasks = new List<Task>();

			foreach (IVersionedFile file in Files)
			{
				Results.Stats.Pending -= 1;
				Results.Stats.Running += 1;

				Task task = DoImplementation(file);
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

		public abstract Task DoImplementation(IVersionedFile file);
	}
}
