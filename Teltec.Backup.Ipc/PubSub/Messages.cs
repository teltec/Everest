using ProtoBuf;
using System;
using Teltec.Storage;

//
// Protobuf-net manual at http://www.codeproject.com/Articles/642677/Protobuf-net-the-unofficial-manual#without
//
namespace Teltec.Backup.Ipc.PubSub
{
	public abstract class ProtocolMsg
	{
		public bool IsAssignableFrom(Type expectedType)
		{
			return GetType().IsAssignableFrom(expectedType);
		}
	}

	public abstract class ProtocolMsgPart
	{
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class StatisticsMsgPart : ProtocolMsgPart
	{
		public int Total;
		public int Pending;
		public int Running;
		public int Failed;
		public int Canceled;
		public int Completed;

		public static StatisticsMsgPart CopyFrom(TransferResults.Statistics other)
		{
			if (other == null)
				return null;
			
			StatisticsMsgPart obj = new StatisticsMsgPart();

			obj.Total = other.Total;
			obj.Pending = other.Pending;
			obj.Running = other.Running;
			obj.Failed = other.Failed;
			obj.Canceled = other.Canceled;
			obj.Completed = other.Completed;

			return obj;
		}

		public void CopyTo(TransferResults.Statistics other)
		{
			if (other == null)
				return;

			other.Total = this.Total;
			other.Pending = this.Pending;
			other.Running = this.Running;
			other.Failed = this.Failed;
			other.Canceled = this.Canceled;
			other.Completed = this.Completed;
		}
	}
	
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class TransferResultsMsgPart : ProtocolMsgPart
	{
		public StatisticsMsgPart Stats;

		public static TransferResultsMsgPart CopyFrom(TransferResults other)
		{
			if (other == null)
				return null;

			TransferResultsMsgPart obj = new TransferResultsMsgPart();

			obj.Stats = StatisticsMsgPart.CopyFrom(other.Stats);

			return obj;
		}

		public void CopyTo(TransferResults other)
		{
			if (other == null)
				return;

			if (this.Stats != null)
				this.Stats.CopyTo(other.Stats);
		}
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	[ProtoInclude(101, typeof(BackupUpdateMsg))]
	[ProtoInclude(201, typeof(RestoreUpdateMsg))]
	public class OperationMsg : ProtocolMsg
	{
		public Int32 PlanId;
		public Int32 OperationId;
	}

	#region Backup operation

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class BackupUpdateMsg : OperationMsg
	{
		public bool IsResuming;
		public DateTime StartedAt;
		public DateTime FinishedAt;
		public byte OperationStatus;
		public TransferResultsMsgPart TransferResults;
	}

	#endregion

	#region Restore operation

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class RestoreUpdateMsg : OperationMsg
	{
		public bool IsResuming;
		public DateTime StartedAt;
		public DateTime FinishedAt;
		public byte OperationStatus;
		public TransferResultsMsgPart TransferResults;
	}

	#endregion
}
