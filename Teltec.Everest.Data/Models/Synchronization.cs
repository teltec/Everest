/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NLog;
using System;
using System.Collections.Generic;

namespace Teltec.Everest.Data.Models
{
	public enum SynchronizationStatus
	{
		FAILED = -2,
		CANCELED = -1,
		STOPPED = 0,
		RUNNING = 1,
		COMPLETED = 2,
	}

	// Represents a file that is stored on the remote storage (S3 for example),
	// and was synchronized (not the same as restored)
	public class Synchronization : BaseEntity<Int32?>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public Synchronization()
		{
		}

		private Int32? _Id;
		public virtual Int32? Id
		{
			get { return _Id; }
			set { SetField(ref _Id, value); }
		}

		#region Accounts

		private EStorageAccountType _StorageAccountType;
		public virtual EStorageAccountType StorageAccountType
		{
			get { return _StorageAccountType; }
			set { SetField(ref _StorageAccountType, value); }
		}

		//private int _StorageAccountId;
		//public virtual int StorageAccountId
		//{
		//	get { return _StorageAccountId; }
		//	set { SetField(ref _StorageAccountId, value); }
		//}

		//public static ICloudStorageAccount GetStorageAccount(Synchronization sync, ICloudStorageAccount dao)
		//{
		//	switch (sync.StorageAccountType)
		//	{
		//		default:
		//			throw new ArgumentException("Unhandled StorageAccountType", "plan");
		//		case EStorageAccountType.AmazonS3:
		//			return dao.Get(sync.StorageAccountId);
		//	}
		//}

		private StorageAccount _StorageAccount;
		public virtual StorageAccount StorageAccount
		{
			get { return _StorageAccount; }
			set { SetField(ref _StorageAccount, value); }
		}

		#endregion

		private DateTime? _StartedAt;
		public virtual DateTime? StartedAt
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

		private SynchronizationStatus _Status;
		public virtual SynchronizationStatus Status
		{
			get { return _Status; }
			protected set { _Status = value; }
		}

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
			Status = SynchronizationStatus.RUNNING;
		}

		public virtual void DidComplete()
		{
			DidCompleteAt(DateTime.UtcNow);
		}

		public virtual void DidCompleteAt(DateTime when)
		{
			FinishedAt = when;
			Status = SynchronizationStatus.COMPLETED;
		}

		public virtual void DidFail()
		{
			DidFailAt(DateTime.UtcNow);
		}

		public virtual void DidFailAt(DateTime when)
		{
			FinishedAt = when;
			Status = SynchronizationStatus.FAILED;
		}

		public virtual void WasCanceled()
		{
			WasCanceledAt(DateTime.UtcNow);
		}

		public virtual void WasCanceledAt(DateTime when)
		{
			FinishedAt = when;
			Status = SynchronizationStatus.CANCELED;
		}

		public virtual bool NeedsResume()
		{
			// Check if it did run or is still running.
			return Status == SynchronizationStatus.STOPPED || Status == SynchronizationStatus.RUNNING;
		}

		#endregion
	}
}
