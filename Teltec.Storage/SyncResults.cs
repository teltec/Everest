using System;
using System.Collections.Generic;
using Teltec.Storage.Backend;

namespace Teltec.Storage
{
	public enum SyncStatus
	{
		FAILED = -2,
		CANCELED = -1,
		STOPPED = 0,
		RUNNING = 1,
		COMPLETED = 2,
	}

	public class SyncResults : IResults
	{
		public class Statistics
		{
			private int _FileCount = 0;
			private long _TotalSize = 0;
			private int _SavedFileCount = 0;

			public int FileCount { get { return _FileCount; } set { _FileCount = value; } }
			public Int64 TotalSize { get { return _TotalSize; } set { _TotalSize = value; } }
			public int SavedFileCount { get { return _SavedFileCount; } set { _SavedFileCount = value; } }

			internal void Reset()
			{
				_SavedFileCount = 0;
				_FileCount = 0;
				_TotalSize = 0;
			}
		}

		public Statistics Stats { get; private set; }

		public List<string> ErrorMessages { get; private set; }

		public event ListingExceptionHandler Failed;
		public event ListingExceptionHandler Canceled;
		public event ListingProgressHandler Completed;
		public event ListingProgressHandler Progress;
		public event ListingProgressHandler Started;

		public SyncResults()
		{
			Stats = new Statistics();
			ErrorMessages = new List<string>();
		}

		internal void OnStarted(object sender, ListingProgressArgs args)
		{
			if (Started != null)
				Started.Invoke(sender, args);
		}

		internal void OnProgress(object sender, ListingProgressArgs args)
		{
			if (Progress != null)
				Progress.Invoke(sender, args);
		}

		internal void OnCompleted(object sender, ListingProgressArgs args)
		{
			if (Completed != null)
				Completed.Invoke(sender, args);
		}

		internal void OnCanceled(object sender, ListingProgressArgs args)
		{
			ErrorMessages.Add(string.Format("Listing canceled: {0}", args.Exception.Message));
			if (Canceled != null)
				Canceled.Invoke(sender, args);
		}

		internal void OnFailed(object sender, ListingProgressArgs args)
		{
			ErrorMessages.Add(string.Format("Listing failed: {0}", args.Exception.Message));
			if (Failed != null)
				Failed.Invoke(sender, args);
		}
	}
}
