using System;
using System.Collections.Generic;
using Teltec.Storage;

namespace Teltec.Everest.Data.Models
{
	public class Backup : BaseEntity<Int32?>
	{
		public Backup()
		{
		}

		public Backup(BackupPlan plan)
			: this()
		{
			_BackupPlan = plan;
			//_StatusInfo = new BackupStatusInfo();
		}

		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		private BackupPlan _BackupPlan;
		public virtual BackupPlan BackupPlan
		{
			get { return _BackupPlan; }
			protected set { _BackupPlan = value; }
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

		//private BackupStatusInfo _StatusInfo;
		//public virtual BackupStatusInfo StatusInfo
		//{
		//	get { return _StatusInfo; }
		//	protected set { _StatusInfo = value; }
		//}

		private IList<BackupedFile> _Files = new List<BackupedFile>();
		public virtual IList<BackupedFile> Files
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
