using System;
using System.Collections.Generic;
using Teltec.Common.Collections;
using Teltec.Storage.Backend;
using Teltec.Storage.Monitor;

namespace Teltec.Storage
{
	public enum TransferStatus
	{
		FAILED = -2,
		CANCELED = -1,
		STOPPED = 0,
		RUNNING = 1,
		COMPLETED = 2,
		PURGED = 3,
	}

	public class TransferResults : IResults
	{
		public class Statistics
		{
			private int _Total = 0;
			private int _Pending = 0;
			private int _Running = 0;
			private int _Failed = 0;
			private int _Canceled = 0;
			private int _Completed = 0;

			public int Total
			{
				get { return _Total; }
				set { _Total = value; }
			}

			public int Pending
			{
				get { return _Pending; }
				set { _Pending = value; }
			}

			public int Running
			{
				get { return _Running; }
				set { _Running = value; }
			}

			public int Failed
			{
				get { return _Failed; }
				set { _Failed = value; }
			}

			public int Canceled
			{
				get { return _Canceled; }
				set { _Canceled = value; }
			}

			public int Completed
			{
				get { return _Completed; }
				set { _Completed = value; }
			}

			private long _BytesTotal = 0;
			private long _BytesFailed = 0;
			private long _BytesCanceled = 0;
			private long _BytesCompleted = 0;

			public long BytesTotal
			{
				get { return _BytesTotal; }
				set { _BytesTotal = value; }
			}

			public long BytesPending
			{
				get { return BytesTotal - BytesCompleted - BytesFailed - BytesCanceled; }
			}

			public long BytesFailed
			{
				get { return _BytesFailed; }
				set { _BytesFailed = value; }
			}

			public long BytesCanceled
			{
				get { return _BytesCanceled; }
				set { _BytesCanceled = value; }
			}

			public long BytesCompleted
			{
				get { return _BytesCompleted; }
				set { _BytesCompleted = value; }
			}

			internal void Reset(int pending)
			{
				_Total = pending;
				_Pending = pending;
				_Running = 0;
				_Failed = 0;
				_Canceled = 0;
				_Completed = 0;

				_BytesTotal = 0;
				_BytesFailed = 0;
				_BytesCanceled = 0;
				_BytesCompleted = 0;
			}
		}

		private ITransferMonitor _Monitor;
		public ITransferMonitor Monitor
		{
			get { return _Monitor; }
			set { _Monitor = value; }
		}

		public TransferStatus OverallStatus
		{
			get
			{
				if (Stats.Canceled > 0) // Cancelation has priority over failure.
					return TransferStatus.CANCELED;
				else if (Stats.Failed > 0) // Failure has priority over completetion.
					return TransferStatus.FAILED;
				else if (Stats.Completed == Stats.Total) // Completion has priority over running.
					return TransferStatus.COMPLETED;
				else if (Stats.Pending > 0 || Stats.Running > 0) // Running has priority over stopped.
					return TransferStatus.RUNNING;
				else
					return TransferStatus.STOPPED;
			}
		}

		public Statistics Stats { get; private set; }

		public List<string> ErrorMessages { get; private set; }

		public TransferResults()
		{
			Stats = new Statistics();
			ActiveTransfers = new ObservableDictionary<string, TransferFileProgressArgs>();
			ActiveDeletions = new ObservableDictionary<string, DeletionArgs>();
			ErrorMessages = new List<string>();
		}

		public void Reset(int pending)
		{
			Stats.Reset(pending);
			ActiveTransfers.Clear();
			ActiveDeletions.Clear();
			ErrorMessages.Clear();
		}

		#region Transfers

		public ObservableDictionary<string, TransferFileProgressArgs> ActiveTransfers { get; private set; }

		public event TransferFileExceptionHandler Failed;
		public event TransferFileExceptionHandler Canceled;
		public event TransferFileProgressHandler Completed;
		public event TransferFileProgressHandler Progress;
		public event TransferFileProgressHandler Started;

		internal void OnStarted(object sender, TransferFileProgressArgs args)
		{
			AddActiveTransfer(args.FilePath, args);
			if (Started != null)
				Started.Invoke(sender, args);
			if (Monitor != null)
				Monitor.TransferStarted(this, args);
		}

		internal void OnProgress(object sender, TransferFileProgressArgs args)
		{
			UpdateActiveTransfer(args.FilePath, args);
			if (Progress != null)
				Progress.Invoke(sender, args);
			if (Monitor != null)
				Monitor.TransferProgress(this, args);
		}

		internal void OnCompleted(object sender, TransferFileProgressArgs args)
		{
			RemoveActiveTransfer(args.FilePath);
			if (Completed != null)
				Completed.Invoke(sender, args);
			if (Monitor != null)
				Monitor.TransferCompleted(this, args);
		}

		internal void OnCanceled(object sender, TransferFileProgressArgs args)
		{
			RemoveActiveTransfer(args.FilePath);
			ErrorMessages.Add(string.Format("{0} canceled: {1}", args.FilePath, args.Exception.Message));
			if (Canceled != null)
				Canceled.Invoke(sender, args);
			if (Monitor != null)
				Monitor.TransferCanceled(this, args);
		}

		internal void OnFailed(object sender, TransferFileProgressArgs args)
		{
			RemoveActiveTransfer(args.FilePath);
			ErrorMessages.Add(string.Format("{0} failed: {1}", args.FilePath, args.Exception.Message));
			if (Failed != null)
				Failed.Invoke(sender, args);
			if (Monitor != null)
				Monitor.TransferFailed(this, args);
		}

		private void AddActiveTransfer(string key, TransferFileProgressArgs value)
		{
			bool contains = ActiveTransfers.ContainsKey(key);
			if (!contains)
				ActiveTransfers.Add(key, value);
		}

		private void RemoveActiveTransfer(string key)
		{
			bool contains = ActiveTransfers.ContainsKey(key);
			if (contains)
				ActiveTransfers.Remove(key);
		}

		private void UpdateActiveTransfer(string key, TransferFileProgressArgs value)
		{
			bool contains = ActiveTransfers.ContainsKey(key);
			if (contains)
				ActiveTransfers[key] = value;
		}

		#endregion

		#region Deletion

		public ObservableDictionary<string, DeletionArgs> ActiveDeletions { get; private set; }

		public event DeleteFileExceptionHandler DeleteFailed;
		public event DeleteFileExceptionHandler DeleteCanceled;
		public event DeleteFileProgressHandler DeleteCompleted;
		public event DeleteFileProgressHandler DeleteStarted;

		internal void OnDeleteStarted(object sender, DeletionArgs args)
		{
			AddActiveDeletion(args.FilePath, args);
			if (DeleteStarted != null)
				DeleteStarted.Invoke(sender, args);
			//if (Monitor != null)
			//	Monitor.DeleteAdded(this, args);
		}

		internal void OnDeleteCompleted(object sender, DeletionArgs args)
		{
			RemoveActiveDeletion(args.FilePath);
			if (DeleteCompleted != null)
				DeleteCompleted.Invoke(sender, args);
			//if (Monitor != null)
			//	Monitor.DeleteCompleted(this, args);
		}

		internal void OnDeleteCanceled(object sender, DeletionArgs args)
		{
			RemoveActiveDeletion(args.FilePath);
			ErrorMessages.Add(string.Format("Deleting {0} canceled: {1}", args.FilePath, args.Exception.Message));
			if (DeleteCanceled != null)
				DeleteCanceled.Invoke(sender, args);
			//if (Monitor != null)
			//	Monitor.DeleteCanceled(this, args);
		}

		internal void OnDeleteFailed(object sender, DeletionArgs args)
		{
			RemoveActiveDeletion(args.FilePath);
			ErrorMessages.Add(string.Format("Deleting {0} failed: {1}", args.FilePath, args.Exception.Message));
			if (DeleteFailed != null)
				DeleteFailed.Invoke(sender, args);
			//if (Monitor != null)
			//	Monitor.DeleteFailed(this, args);
		}

		private void AddActiveDeletion(string key, DeletionArgs value)
		{
			// TODO(jweyrich): Do we want to track this visually?
		}

		private void RemoveActiveDeletion(string key)
		{
			// TODO(jweyrich): Do we want to track this visually?
		}

		#endregion
	}
}
