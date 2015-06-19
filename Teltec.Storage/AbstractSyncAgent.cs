using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teltec.Storage.Agent;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public abstract class AbstractSyncAgent<TFile> where TFile : IVersionedFile
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected IAsyncTransferAgent TransferAgent { get; private set; }

		public AbstractSyncAgent(IAsyncTransferAgent agent)
		{
			Results = new SyncResults();
			TransferAgent = agent;
		}

		//private string _FilesAsDelimitedString;
		//public string FilesAsDelimitedString(string delimiter, int maxLength, string trail)
		//{
		//	if (_FilesAsDelimitedString == null)
		//		_FilesAsDelimitedString = Files.AsDelimitedString(p => p.Path,
		//			"No files to transfer", delimiter, maxLength, trail);
		//	return _FilesAsDelimitedString;
		//}

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
			_EstimatedTotalSize = 0;
			//_FilesAsDelimitedString = null;
			Results.Stats.Reset();
		}

		// In Bytes
		private long _EstimatedTotalSize;
		public long EstimatedTotalSize
		{
			get
			{
				if (_EstimatedTotalSize == 0 && Files != null)
					_EstimatedTotalSize = Enumerable.Sum(Files, p => p.Size);
				return _EstimatedTotalSize;
			}
		}

		public SyncResults Results { get; private set; }

		protected void RegisterListingEventHandlers()
		{
			TransferAgent.ListingStarted += (object sender, ListingProgressArgs e) =>
			{
				Results.OnStarted(this, e);
			};
			TransferAgent.ListingProgress += (object sender, ListingProgressArgs e) =>
			{
				Results.OnProgress(this, e);
			};
			TransferAgent.ListingCanceled += (object sender, ListingProgressArgs e, Exception ex) =>
			{
				Results.OnCanceled(this, e, ex);
			};
			TransferAgent.ListingFailed += (object sender, ListingProgressArgs e, Exception ex) =>
			{
				Results.OnFailed(this, e, ex);
			};
			TransferAgent.ListingCompleted += (object sender, ListingProgressArgs e) =>
			{
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

		public async Task Start(string prefix, bool recursive)
		{
			Results.Stats.Reset();

			TransferAgent.RenewCancellationToken();
			List<Task> activeTasks = new List<Task>();

			Task task = DoImplementation(prefix, recursive);
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

			await Task.WhenAll(activeTasks.ToArray());
		}

		public abstract Task DoImplementation(string prefix, bool recursive);
	}
}
