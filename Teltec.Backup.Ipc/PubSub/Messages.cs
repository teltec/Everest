using ProtoBuf;
using System;
using Teltec.Storage;

namespace Teltec.Backup.Ipc.PubSub
{
	public static class Protocol
	{
		public static readonly Byte Version = 1;
	}

	public enum OperationType : byte
	{
		Backup = 0,
		Restore = 1,
	}

	public enum OperationStatus : byte
	{
		Unknown = 0,
		Started = 1,
		Resumed = 2,
		ScanningFilesStarted = 3,
		ScanningFilesFinished = 4,
		ProcessingFilesStarted = 5,
		ProcessingFilesFinished = 6,
		Updated = 7,
		Canceled = 8,
		Failed = 9,
		Finished = 10,
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class TransferResultsMsg
	{
		public Statistics Stats;

		[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
		public class Statistics
		{
			public int Total;
			public int Pending;
			public int Running;
			public int Failed;
			public int Canceled;
			public int Completed;

			public Statistics()
			{
			}

			public Statistics(TransferResults.Statistics other)
			{
				if (other == null)
					return;

				Total = other.Total;
				Pending = other.Pending;
				Running = other.Running;
				Failed = other.Failed;
				Canceled = other.Canceled;
				Completed = other.Completed;
			}
		}

		public TransferResultsMsg()
		{
		}

		public TransferResultsMsg(TransferResults other)
		{
			if (other == null)
				return;

			if (other.Stats != null)
				Stats = new Statistics(other.Stats);
		}
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class OperationProgressMessage
	{
		public byte Version;
		public OperationType OperationType;
		public Int32 OperationId;
		public byte OperationStatus;
		public TransferResultsMsg TransferResults;
	}
}
