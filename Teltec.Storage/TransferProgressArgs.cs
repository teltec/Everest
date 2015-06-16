using System;
using System.Collections.Generic;
using Teltec.Common;
using Teltec.Common.Extensions;

namespace Teltec.Storage
{
	public enum TransferState
	{
		PENDING			= 0, // default!
		STARTED			= 1,
		TRANSFERRING	= 2,
		CANCELED		= 3,
		FAILED			= 4,
		COMPLETED		= 5,
	}

	public abstract class TransferProgressArgs : ObservableEventArgs
	{
		private object _Tag;
		public object Tag
		{
			get { return _Tag; }
			set { SetField(ref _Tag, value); }
		}

		private TransferState _State;
		public TransferState State
		{
			get { return _State; }
			set { SetField(ref _State, value); }
		}

		private long _TotalBytes;
		public long TotalBytes
		{
			get { return _TotalBytes; }
			set
			{
				SetField(ref _TotalBytes, value);
				PercentDone = ToPercent(TransferredBytes, TotalBytes);
				RemainingBytes = _TotalBytes - TransferredBytes;
			}
		}

		private long _TransferredBytes;
		public long TransferredBytes
		{
			get { return _TransferredBytes; }
			set
			{
				SetField(ref _TransferredBytes, value);
				PercentDone = ToPercent(_TransferredBytes, TotalBytes);
				RemainingBytes = TotalBytes - _TransferredBytes;
			}
		}

		private int _PercentDone;
		public int PercentDone
		{
			get { return _PercentDone; }
			private set { SetField(ref _PercentDone, value); }
		}

		private long _RemainingBytes;
		public long RemainingBytes
		{
			get { return _RemainingBytes; }
			private set { SetField(ref _RemainingBytes, value); }
		}

		public bool IsDone
		{
			// TODO(jweyrich): Change to `State == TransferState.COMPLETED` ?
			get { return TransferredBytes == TotalBytes; }
		}

		private int ToPercent(long partial, long total)
		{
			if (total == 0)
				return 0;
			return (int)(partial * 100 / total);
		}
	}

	public class TransferFileProgressArgs : TransferProgressArgs
	{
		public string FilePath { get; set; }
	}

	public class ListingObject
	{
		public ListingObject()
		{
		}

		public string ETag { get; set; }
		public string Key { get; set; }
		public DateTime LastModified { get; set; }
		//public Owner Owner { get; set; }
		public long Size { get; set; }
		//public S3StorageClass StorageClass { get; set; }
	}

	public class ListingProgressArgs : ObservableEventArgs
	{
		private TransferState _State;
		public TransferState State
		{
			get { return _State; }
			set { SetField(ref _State, value); }
		}

		private List<ListingObject> _Objects;
		public List<ListingObject> Objects
		{
			get { return _Objects; }
			set { SetField(ref _Objects, value); }
		}
	}
}
