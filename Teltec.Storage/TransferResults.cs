using System;
using System.Collections.Generic;
using Teltec.Common.Collections;
using Teltec.Storage.Agent;
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
	}

	public class TransferResults
	{
		public class Statistics
		{
			private int _Total = 0;
			private int _Pending = 0;
			private int _Running = 0;
			private int _Failed = 0;
			private int _Canceled = 0;
			private int _Completed = 0;

			public int Total { get { return _Total; } set { _Total = value; } }
			public int Pending { get { return _Pending; } set { _Pending = value; } }
			public int Running { get { return _Running; } set { _Running = value; } }
			public int Failed { get { return _Failed; } set { _Failed = value; } }
			public int Canceled { get { return _Canceled; } set { _Canceled = value; } }
			public int Completed { get { return _Completed; } set { _Completed = value; } }

			internal void Reset(int pending)
			{
				_Total = pending;
				_Pending = pending;
				_Running = 0;
				_Failed = 0;
				_Canceled = 0;
				_Completed = 0;
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
				if (Stats.Pending > 0 || Stats.Running > 0) // Running has priority over all status.
					return TransferStatus.RUNNING;
				else if (Stats.Failed > 0) // Failure has priority over cancelation.
					return TransferStatus.FAILED;
				else if (Stats.Canceled > 0) // Cancelation has priority over completion.
					return TransferStatus.CANCELED;
				else if (Stats.Completed == Stats.Total) // Completion has priority over stopped.
					return TransferStatus.COMPLETED;
				else
					return TransferStatus.STOPPED;
			}
		}

		public Statistics Stats { get; private set; }

		public ObservableDictionary<string, TransferFileProgressArgs> ActiveTransfers { get; private set; }

		public List<string> ErrorMessages { get; private set; }

		public event TransferFileExceptionHandler Failed;
		public event TransferFileExceptionHandler Canceled;
		public event TransferFileProgressHandler Completed;
		public event TransferFileProgressHandler Progress;
		public event TransferFileProgressHandler Started;

		public TransferResults()
		{
			Stats = new Statistics();
			ActiveTransfers = new ObservableDictionary<string, TransferFileProgressArgs>();
			ErrorMessages = new List<string>();
		}

		internal void OnStarted(object sender, TransferFileProgressArgs args)
		{
			AddActiveTransfer(args.FilePath, args);
			if (Started != null)
				Started.Invoke(sender, args);
			if (Monitor != null)
				Monitor.TransferAdded(this, args);
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

		internal void OnCanceled(object sender, TransferFileProgressArgs args, Exception exception)
		{
			RemoveActiveTransfer(args.FilePath);
			ErrorMessages.Add(string.Format("{0} canceled: {1}", args.FilePath, exception.Message));
			if (Canceled != null)
				Canceled.Invoke(sender, args, exception);
			if (Monitor != null)
				Monitor.TransferCanceled(this, args, exception);
		}

		internal void OnFailed(object sender, TransferFileProgressArgs args, Exception exception)
		{
			RemoveActiveTransfer(args.FilePath);
			ErrorMessages.Add(string.Format("{0} failed: {1}", args.FilePath, exception.Message));
			if (Failed != null)
				Failed.Invoke(sender, args, exception);
			if (Monitor != null)
				Monitor.TransferFailed(this, args, exception);
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
	}
}
