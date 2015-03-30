using NLog;
using System;
using System.Collections.Generic;
using Teltec.Storage;

namespace Teltec.Backup.App.Models
{
	public class Restore : BaseEntity<Int32?>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
			StartedAt = DateTime.UtcNow;
			Status = TransferStatus.RUNNING;
		}

		public virtual void DidComplete()
		{
			FinishedAt = DateTime.UtcNow;
			Status = TransferStatus.COMPLETED;
		}

		public virtual void DidFail()
		{
			FinishedAt = DateTime.UtcNow;
			Status = TransferStatus.FAILED;
		}

		public virtual void WasCanceled()
		{
			FinishedAt = DateTime.UtcNow;
			Status = TransferStatus.CANCELED;
		}

		public virtual bool NeedsResume()
		{
			// Check if it did run or is still running.
			return Status == TransferStatus.STOPPED || Status == TransferStatus.RUNNING;
		}

		#endregion
	}
}
