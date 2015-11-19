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
	public abstract class AbstractAgent<TFile> : IDisposable where TFile : IVersionedFile
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		protected ITransferAgent TransferAgent { get; private set; }

		public AbstractAgent(ITransferAgent agent)
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
				Results.Stats.Running += 1;
				Results.Stats.Pending -= 1;
				Results.OnStarted(this, e);
			};
			TransferAgent.UploadFileProgress += (object sender, TransferFileProgressArgs e) =>
			{
				//logger.Debug("## DEBUG Results.Stats.BytesCompleted = {0}, e.DeltaTransferredBytes = {1}",
				//				Results.Stats.BytesCompleted, e.DeltaTransferredBytes);
				Results.Stats.BytesCompleted += e.DeltaTransferredBytes;
				Results.OnProgress(this, e);
			};
			TransferAgent.UploadFileCanceled += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Running -= 1;
				Results.Stats.Canceled += 1;
				Results.Stats.BytesCanceled += e.TotalBytes;
				//Results.Stats.BytesCompleted -= e.TransferredBytes;
				Results.OnCanceled(this, e, ex);
			};
			TransferAgent.UploadFileFailed += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Running -= 1;
				Results.Stats.Failed += 1;
				Results.Stats.BytesFailed += e.TotalBytes;
				//Results.Stats.BytesCompleted -= e.TransferredBytes;
				Results.OnFailed(this, e, ex);
			};
			TransferAgent.UploadFileCompleted += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.Running -= 1;
				Results.Stats.Completed += 1;
				//Results.Stats.BytesCompleted += e.TotalBytes;
				Results.OnCompleted(this, e);
			};
		}

		protected void RegisterDownloadEventHandlers()
		{
			TransferAgent.DownloadFileStarted += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.Running += 1;
				Results.Stats.Pending -= 1;
				Results.OnStarted(this, e);
			};
			TransferAgent.DownloadFileProgress += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.BytesCompleted += e.DeltaTransferredBytes;
				Results.OnProgress(this, e);
			};
			TransferAgent.DownloadFileCanceled += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Running -= 1;
				Results.Stats.Canceled += 1;
				Results.Stats.BytesCanceled += e.TotalBytes;
				//Results.Stats.BytesCompleted -= e.TransferredBytes;
				Results.OnCanceled(this, e, ex);
			};
			TransferAgent.DownloadFileFailed += (object sender, TransferFileProgressArgs e, Exception ex) =>
			{
				Results.Stats.Running -= 1;
				Results.Stats.Failed += 1;
				Results.Stats.BytesFailed += e.TotalBytes;
				//Results.Stats.BytesCompleted -= e.TransferredBytes;
				Results.OnFailed(this, e, ex);
			};
			TransferAgent.DownloadFileCompleted += (object sender, TransferFileProgressArgs e) =>
			{
				Results.Stats.Running -= 1;
				Results.Stats.Completed += 1;
				//Results.Stats.BytesCompleted += e.TotalBytes;
				Results.OnCompleted(this, e);
			};
		}

		protected void RegisterDeleteEventHandlers()
		{
			TransferAgent.DeleteFileStarted += (object sender, DeletionArgs e) =>
			{
				Results.OnDeleteStarted(this, e);
			};
			TransferAgent.DeleteFileCanceled += (object sender, DeletionArgs e, Exception ex) =>
			{
				Results.OnDeleteCanceled(this, e, ex);
			};
			TransferAgent.DeleteFileFailed += (object sender, DeletionArgs e, Exception ex) =>
			{
				Results.OnDeleteFailed(this, e, ex);
			};
			TransferAgent.DeleteFileCompleted += (object sender, DeletionArgs e) =>
			{
				Results.OnDeleteCompleted(this, e);
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

		public async Task Start()
		{
			Results.Stats.Reset(Files.Count());

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
						DoImplementation(currentFile, /*userData*/ null);
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

		public abstract void DoImplementation(IVersionedFile file, object userData);

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
