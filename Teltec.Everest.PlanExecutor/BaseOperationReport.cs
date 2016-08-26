using System;
using System.Collections.Generic;

namespace Teltec.Everest.PlanExecutor
{
	public enum OperationStatus
	{
		UNKNOWN = 0,
		COMPLETED = 1,
		FAILED = 2,
		CANCELED = 3,
	}

	public interface IBaseOperationReport
	{
		IReadOnlyList<string> ErrorMessages { get; }
		bool HasErrorMessages { get; }
		void AddErrorMessage(string message);
		void AddErrorMessages(IEnumerable<string> messages);

		void Reset();
		void AggregateResults();
	}

	public abstract class BaseOperationReport : IBaseOperationReport
	{
		public OperationStatus OperationStatus = OperationStatus.UNKNOWN;

		public string PlanType;
		public string PlanName;
		public string BucketName;
		public string HostName;

		public DateTime StartedAt;
		public DateTime FinishedAt;

		private List<string> _ErrorMessages;
		public IReadOnlyList<string> ErrorMessages
		{
			get { return _ErrorMessages.AsReadOnly(); }
		}

		public virtual bool HasErrorMessages
		{
			get { return _ErrorMessages.Count > 0; }
		}

		public void AddErrorMessage(string message)
		{
			_ErrorMessages.Add(message);
		}

		public void AddErrorMessages(IEnumerable<string> messages)
		{
			_ErrorMessages.AddRange(messages);
		}

		public BaseOperationReport()
		{
			_ErrorMessages = new List<string>();
		}

		public virtual void Reset()
		{
			OperationStatus = OperationStatus.UNKNOWN;
			PlanType = null;
			PlanName = null;
			BucketName = null;
			HostName = null;

			StartedAt = new DateTime();
			FinishedAt = new DateTime();

			_ErrorMessages.Clear();
		}

		public virtual void AggregateResults()
		{
		}
	}
}
