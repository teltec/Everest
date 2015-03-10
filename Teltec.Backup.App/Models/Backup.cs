using NLog;
using System;
using System.Collections.Generic;
using Teltec.Storage;

namespace Teltec.Backup.App.Models
{
	public class Backup : BaseEntity<Int32?>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public Backup()
		{
		}
		
		public Backup(BackupPlan plan)
			: this()
		{
			BackupPlan = plan;
			//StatusInfo = new BackupStatusInfo();
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

		private BackupStatus _Status;
		public virtual BackupStatus Status
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
			StartedAt = DateTime.UtcNow;
			Status = BackupStatus.RUNNING;
		}

		public virtual void DidComplete()
		{
			FinishedAt = DateTime.UtcNow;
			Status = BackupStatus.COMPLETED;
		}

		public virtual void DidFail()
		{
			FinishedAt = DateTime.UtcNow;
			Status = BackupStatus.FAILED;
		}

		public virtual void WasCanceled()
		{
			FinishedAt = DateTime.UtcNow;
			Status = BackupStatus.CANCELED;
		}

		#endregion
	}
}
