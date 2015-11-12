using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teltec.Common.Extensions;
using Teltec.Storage.Backend;
using Teltec.Storage.Versioning;

namespace Teltec.Storage
{
	public abstract class AbstractSyncAgent<TFile> : IDisposable where TFile : IVersionedFile
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected ITransferAgent TransferAgent { get; private set; }

		public AbstractSyncAgent(ITransferAgent agent)
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
			CancelTransfers();
		}

		private void CancelTransfers()
		{
			CancellationTokenSource.Cancel();
		}

		private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

		private void RenewCancellationToken()
		{
			bool alreadyUsed = CancellationTokenSource != null && CancellationTokenSource.IsCancellationRequested;
			if (alreadyUsed || CancellationTokenSource == null)
			{
				if (CancellationTokenSource != null)
					CancellationTokenSource.Dispose();

				CancellationTokenSource = new CancellationTokenSource();
			}
		}

		public async Task Start(string prefix, bool recursive)
		{
			Results.Stats.Reset();

			RenewCancellationToken();

			await Task.Run(() =>
			{
				try
				{
					ParallelOptions options = new ParallelOptions();
					options.CancellationToken = CancellationTokenSource.Token;
					options.MaxDegreeOfParallelism = AsyncHelper.SettingsMaxThreadCount;

					ParallelLoopResult result = Parallel.ForEach(Files, options, (currentFile) =>
					{
						DoImplementation(prefix, recursive, /*userData*/ null);
					});
				}
				catch (Exception ex)
				{
					// When there are Tasks running inside another Task, and the inner-tasks are cancelled,
					// the propagated exception is an instance of `AggregateException`, rather than
					// `OperationCanceledException`.
					if (ex.IsCancellation())
					{
						throw new OperationCanceledException("The operation was canceled.");
					}
					else
					{
						throw ex;
					}
				}
			});
		}

		public abstract void DoImplementation(string prefix, bool recursive, object userData);

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
					if (CancellationTokenSource != null)
					{
						CancellationTokenSource.Dispose();
						CancellationTokenSource = null;
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
