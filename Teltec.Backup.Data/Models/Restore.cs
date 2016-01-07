using System;
using System.Collections.Generic;
using Teltec.Storage;

namespace Teltec.Backup.Data.Models
{
	public class Restore : BaseEntity<Int32?>
	{
		public Restore()
		{
		}

		public Restore(RestorePlan plan)
			: this()
		{
			RestorePlan = plan;
			//StatusInfo = new RestoreStatusInfo();
		}

		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private RestorePlan _RestorePlan;
		public virtual RestorePlan RestorePlan
		{
			get { return _RestorePlan; }
			protected set { _RestorePlan = value; }
		}

		private DateTime _StartedAt;
		public virtual DateTime StartedAt
		{
			get { return _StartedAt; }
			protected set { _StartedAt = value; }
		}

		private DateTime? _FinishedAt;
		public virtual DateTime? FinishedAt
		{
			get { return _FinishedAt; }
			protected set { _FinishedAt = value; }
		}

		private TransferStatus _Status;
		public virtual TransferStatus Status
		{
			get { return _Status; }
			protected set { _Status = value; }
		}

		//private RestoreStatusInfo _StatusInfo;
		//public virtual RestoreStatusInfo StatusInfo
		//{
		//	get { return _StatusInfo; }
		//	protected set { _StatusInfo = value; }
		//}

		private IList<RestoredFile> _Files = new List<RestoredFile>();
		public virtual IList<RestoredFile> Files
		{
			get { return _Files; }
			protected set { SetField(ref _Files, value); }
		}

		#region Status reporting

		public virtual void DidStart()
		{
			DidStartAt(DateTime.UtcNow);
		}

		public virtual void DidStartAt(DateTime when)
		{
			StartedAt = when;
			Status = TransferStatus.RUNNING;
		}

		public virtual void DidComplete()
		{
			DidCompleteAt(DateTime.UtcNow);
		}

		public virtual void DidCompleteAt(DateTime when)
		{
			FinishedAt = when;
			Status = TransferStatus.COMPLETED;
		}

		public virtual void DidFail()
		{
			DidFailAt(DateTime.UtcNow);
		}

		public virtual void DidFailAt(DateTime when)
		{
			FinishedAt = when;
			Status = TransferStatus.FAILED;
		}

		public virtual void WasCanceled()
		{
			WasCanceledAt(DateTime.UtcNow);
		}

		public virtual void WasCanceledAt(DateTime when)
		{
			FinishedAt = when;
			Status = TransferStatus.CANCELED;
		}

		public virtual bool NeedsResume()
		{
			// Check if it did run or is still running.
			return Status == TransferStatus.STOPPED || Status == TransferStatus.RUNNING;
		}

		public virtual bool IsFinished()
		{
			return Status == TransferStatus.CANCELED
				|| Status == TransferStatus.COMPLETED
				|| Status == TransferStatus.FAILED;
		}

		#endregion
	}
}
