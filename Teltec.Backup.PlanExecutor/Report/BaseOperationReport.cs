using System;
using System.Collections.Generic;
using Teltec.Backup.PlanExecutor.Versioning;
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

		public FileVersionerResults VersionerResults;
		public TransferResults TransferResults;
		public TransferStatus TransferStatus;

		public List<string> ErrorMessages { get; protected set; }

		public BaseOperationReport()
		{
			ErrorMessages = new List<string>();
		}

		public void AggregateResults()
		{
			ErrorMessages.AddRange(VersionerResults.ErrorMessages);
			ErrorMessages.AddRange(TransferResults.ErrorMessages);

			// TODO(jweyrich): Should aggreatate `VersionerResults.Stats.Failed + TransferResults.Stats.Failed` into a local `Failed` variable.
		}

		public bool HasErrorMessages
		{
			get { return ErrorMessages.Count > 0; }
		}
	}
}
