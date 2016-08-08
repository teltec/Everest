using System;
using Teltec.Storage;

namespace Teltec.Backup.PlanExecutor.Report
{
	public class BaseOperationReport
	{
		public string PlanType;
		public string PlanName;
		public string BucketName;
		public string HostName;

		public DateTime StartedAt;
		public DateTime FinishedAt;

		public TransferStatus TransferStatus;
		public TransferResults TransferResults;
	}
}
