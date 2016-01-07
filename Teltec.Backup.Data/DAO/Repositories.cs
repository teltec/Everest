using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using Teltec.Common.Extensions;
using Teltec.Storage;

namespace Teltec.Backup.Data.DAO
{
	#region Accounts

	public class StorageAccountRepository : BaseRepository<Models.StorageAccount, Int32?>
	{
		public StorageAccountRepository()
		{
		}

		public StorageAccountRepository(ISession session)
			: base(session)
		{
		}
	}

	public class AmazonS3AccountRepository : BaseRepository<Models.AmazonS3Account, Int32?>
	{
		public AmazonS3AccountRepository()
		{
		}

		public AmazonS3AccountRepository(ISession session)
			: base(session)
		{
		}
	}

	#endregion

	public class NetworkCredentialRepository : BaseRepository<Models.NetworkCredential, Int32?>
	{
		public NetworkCredentialRepository()
		{
		}

		public NetworkCredentialRepository(ISession session)
			: base(session)
		{
		}
	}

	#region Schedule

	public class PlanScheduleRepository : BaseRepository<Models.PlanSchedule, Int32?>
	{
		public PlanScheduleRepository()
		{
		}

		public PlanScheduleRepository(ISession session)
			: base(session)
		{
		}
	}

	#endregion

	#region Backup

	public class BackupPlanRepository : BaseRepository<Models.BackupPlan, Int32?>
	{
		public BackupPlanRepository()
		{
		}

		public BackupPlanRepository(ISession session)
			: base(session)
		{
		}

		public IList<Models.BackupPlan> GetAllActive()
		{
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string isDeletedPropertyName = this.GetPropertyName((Models.BackupPlan x) => x.IsDeleted);
			crit.Add(Restrictions.Eq(isDeletedPropertyName, false));
			string lastRunAtPropertyName = this.GetPropertyName((Models.BackupPlan x) => x.LastRunAt);
			return crit.List<Models.BackupPlan>();
		}

		[ThreadStatic]
		private static Random _Random = new Random(Environment.TickCount);

		public override void DeleteImpl(ITransaction tx, Models.BackupPlan instance)
		{
			// Update an instance property instead of actually deleting it.
			instance.IsDeleted = true;
			// Avoid Unique Key exceptions - The user might want to create another
			// plan using the same name of a deleted plan, so we append a randomized
			// string to the deleted one.
			instance.Name += "_DELETED_" + _Random.GenerateRandomHexaString(10);
		}
	}

	public class BackupPlanSourceEntryRepository : BaseRepository<Models.BackupPlanSourceEntry, Int64?>
	{
		public BackupPlanSourceEntryRepository()
		{
		}

		public BackupPlanSourceEntryRepository(ISession session)
			: base(session)
		{
		}
	}

	public class BackupRepository : BaseRepository<Models.Backup, Int32?>
	{
		public BackupRepository()
		{
		}

		public BackupRepository(ISession session)
			: base(session)
		{
		}

		public Models.Backup GetLatestByPlan(Models.BackupPlan plan)
		{
			Assert.IsNotNull(plan);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string backupPlanPropertyName = this.GetPropertyName((Models.Backup x) => x.BackupPlan);
			crit.Add(Restrictions.Eq(backupPlanPropertyName, plan));
			string idPropertyName = this.GetPropertyName((Models.Backup x) => x.Id);
			crit.AddOrder(Order.Desc(idPropertyName));
			crit.SetMaxResults(1);
			return crit.UniqueResult<Models.Backup>();
		}
	}

	public class BackupPlanFileRepository : BaseRepository<Models.BackupPlanFile, Int64?>
	{
		public BackupPlanFileRepository()
		{
		}

		public BackupPlanFileRepository(ISession session)
			: base(session)
		{
		}

		//public BackupPlanFileRepository()
		//{
		//	BeforeInsert = (ITransaction tx, Models.BackupPlanFile instance) =>
		//	{
		//		instance.CreatedAt = DateTime.UtcNow;
		//	};
		//	BeforeUpdate = (ITransaction tx, Models.BackupPlanFile instance) =>
		//	{
		//		instance.UpdatedAt = DateTime.UtcNow;
		//	};
		//}

		public IList<Models.BackupPlanFile> GetAllByStorageAccount(Models.StorageAccount account)
		{
			Assert.IsNotNull(account);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string storageAccountPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.StorageAccount);
			crit.Add(Restrictions.Eq(storageAccountPropertyName, account));
			return crit.List<Models.BackupPlanFile>();
		}

		public IList<Models.BackupPlanFile> GetAllByBackupPlan(Models.BackupPlan plan)
		{
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string backupPlanPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.BackupPlan);
			if (plan == null)
				crit.Add(Restrictions.IsNull(backupPlanPropertyName));
			else
				crit.Add(Restrictions.Eq(backupPlanPropertyName, plan));
			return crit.List<Models.BackupPlanFile>();
		}

		public IList<Models.BackupPlanFile> GetAllPendingByBackup(Models.Backup backup)
		{
			string bfModelName = typeof(Models.BackupedFile).Name;
			string bfFilePropertyName = this.GetPropertyName((Models.BackupedFile x) => x.File);
			string bfBackupIdPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.Backup);
			string bfTransferStatusPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.TransferStatus);

			// REFERENCE: https://docs.jboss.org/hibernate/orm/3.5/reference/en-US/html/queryhql.html#queryhql-expressions
			string queryString = string.Format(
				"SELECT bpf"
				+ " FROM {0} AS bf"
				+ " INNER JOIN bf.{1} AS bpf"
				+ " WHERE"
				+ "   bf.{2} = :backupId"
				+ "   AND bf.{3} != :transferStatusCompleted"
				, bfModelName
				, bfFilePropertyName
				, bfBackupIdPropertyName
				, bfTransferStatusPropertyName
			);

			IQuery query = Session.CreateQuery(queryString)
				.SetParameter("backupId", backup.Id)
				.SetParameter("transferStatusCompleted", TransferStatus.COMPLETED)
				;

			return query.List<Models.BackupPlanFile>();
		}

		public Models.BackupPlanFile GetByStorageAccountAndPath(Models.StorageAccount account, string path, bool ignoreCase = false)
		{
			Assert.IsNotNullOrEmpty(path);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string storageAccountPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.StorageAccount);
			crit.Add(Restrictions.Eq(storageAccountPropertyName, account));
			string pathPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.Path);
			SimpleExpression expr = Restrictions.Eq(pathPropertyName, path);
			if (ignoreCase)
				expr = expr.IgnoreCase();
			crit.Add(expr);
			return crit.UniqueResult<Models.BackupPlanFile>();
		}


		public int AssociateSyncedFileToBackupPlan(Models.BackupPlan plan, string path, bool ignoreCase = false)
		{
			Assert.IsNotNull(plan);
			Assert.IsNotNull(plan.Id);
			Assert.IsNotNullOrEmpty(path);

			string modelName = typeof(Models.BackupPlanFile).Name;
			string backupPlanPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.BackupPlan);
			string storageAccountPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.StorageAccount);
			string pathPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.Path);

			string queryString = string.Format(
				"UPDATE {0}"
				+ "   SET {1} = :backupPlanId"
				+ " WHERE"
				+ "   {1} IS NULL"
				+ "   AND {2} = :storageAccountId"
				+ (ignoreCase ? "   AND lower({3}) = lower(:path)" : "   AND {3} = :path")
				, modelName, backupPlanPropertyName, storageAccountPropertyName, pathPropertyName
			);

			IQuery query = Session.CreateQuery(queryString)
				.SetParameter("backupPlanId", plan.Id)
				.SetParameter("storageAccountId", plan.StorageAccount.Id)
				.SetParameter("path", path)
				;

			return query.ExecuteUpdate();
		}
	}

	public class BackupPlanPathNodeRepository : BaseRepository<Models.BackupPlanPathNode, Int64?>
	{
		public BackupPlanPathNodeRepository()
		{
		}

		public BackupPlanPathNodeRepository(ISession session)
			: base(session)
		{
		}

		public IList<Models.BackupPlanPathNode> GetAllDrivesByStorageAccount(Models.StorageAccount account)
		{
			Assert.IsNotNull(account);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string storageAccountPropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.StorageAccount);
			string typePropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.Type);
			crit.Add(Restrictions.Eq(storageAccountPropertyName, account));
			crit.Add(Restrictions.Eq(typePropertyName, Models.EntryType.DRIVE));
			return crit.List<Models.BackupPlanPathNode>();
		}

		public Models.BackupPlanPathNode GetByStorageAccountAndTypeAndPath(Models.StorageAccount account, Models.EntryType type, string path, bool ignoreCase = false)
		{
			Assert.IsNotNull(account);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string storageAccountPropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.StorageAccount);
			string typePropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.Type);
			string pathPropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.Path);
			crit.Add(Restrictions.Eq(storageAccountPropertyName, account));
			crit.Add(Restrictions.Eq(typePropertyName, type));
			SimpleExpression expr = Restrictions.Eq(pathPropertyName, path);
			if (ignoreCase)
				expr = expr.IgnoreCase();
			crit.Add(expr);
			return crit.UniqueResult<Models.BackupPlanPathNode>();
		}
	}

	public class BackupedFileRepository : BaseRepository<Models.BackupedFile, Int64?>
	{
		public BackupedFileRepository()
		{
		}

		public BackupedFileRepository(ISession session)
			: base(session)
		{
		}

		//public BackupedFileRepository()
		//{
		//	BeforeInsert = (ITransaction tx, Models.BackupedFile instance) =>
		//	{
		//		instance.UpdatedAt = DateTime.UtcNow;
		//	};
		//	BeforeUpdate = (ITransaction tx, Models.BackupedFile instance) =>
		//	{
		//		instance.UpdatedAt = DateTime.UtcNow;
		//	};
		//}

		public bool IsLatestVersion(Models.BackupedFile file)
		{
			Models.BackupedFile found = GetLatestVersionCompleted(file.File);
			return found == null || (found != null && found.Id.Value == file.Id.Value);
		}

		// NOTE: This may return a version that has not COMPLETED the transfer.
		public Models.BackupedFile GetLatestVersion(Models.BackupPlanFile planFile)
		{
			return GetLatestVersionWithTransferStatus(planFile);
		}

		public Models.BackupedFile GetLatestVersionCompleted(Models.BackupPlanFile planFile)
		{
			return GetLatestVersionWithTransferStatus(planFile, new TransferStatus[] { TransferStatus.COMPLETED });
		}

		public Models.BackupedFile GetLatestVersionWithTransferStatus(Models.BackupPlanFile planFile, params TransferStatus[] statuses)
		{
			Assert.IsNotNull(planFile);
			//Assert.IsFalse(IsTransient(Session, file));

			ICriteria crit = Session.CreateCriteria(PersistentType);

			string filePropertyName = this.GetPropertyName((Models.BackupedFile x) => x.File);
			crit.Add(Restrictions.Eq(filePropertyName, planFile));

			if (statuses != null && statuses.Length > 0)
			{
				string transferStatus = this.GetPropertyName((Models.BackupedFile x) => x.TransferStatus);
				crit.Add(Restrictions.Eq(transferStatus, TransferStatus.COMPLETED));
			}

			//string idPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.Id);
			//crit.AddOrder(Order.Desc(idPropertyName));

			string fileLastWrittenAtPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.FileLastWrittenAt);
			crit.AddOrder(Order.Desc(fileLastWrittenAtPropertyName));

			crit.SetMaxResults(1);
			return crit.UniqueResult<Models.BackupedFile>(); // May return null
		}

		public Models.BackupedFile GetByBackupAndPath(Models.Backup backup, string path, bool ignoreCase = false)
		{
			Assert.IsNotNull(backup);
			Assert.IsNotNullOrEmpty(path);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string backupPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.Backup);
			string filePropertyName = this.GetPropertyName((Models.BackupedFile x) => x.File);
			string filePathPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.Path);
			crit.CreateAlias(filePropertyName, "f");
			crit.Add(Restrictions.Eq(backupPropertyName, backup));
			SimpleExpression expr = Restrictions.Eq("f." + filePathPropertyName, path);
			if (ignoreCase)
				expr = expr.IgnoreCase();
			crit.Add(expr);
			return crit.UniqueResult<Models.BackupedFile>();
		}

		public IList<Models.BackupedFile> GetByBackupAndStatus(Models.Backup backup, params TransferStatus[] statuses)
		{
			Assert.IsNotNull(backup);
			Assert.IsNotNull(statuses);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string backupPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.Backup);
			string transferStatusPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.TransferStatus);
			crit.Add(Restrictions.Eq(backupPropertyName, backup));
			crit.Add(Restrictions.In(transferStatusPropertyName, statuses));
			return crit.List<Models.BackupedFile>();
		}

		public IList<Models.BackupedFile> GetCompletedByStorageAccountAndPath(Models.StorageAccount account, string path, string version = null, bool ignoreCase = false)
		{
			Assert.IsNotNull(account);
			Assert.IsNotNullOrEmpty(path);
			ICriteria crit = Session.CreateCriteria(PersistentType);

			string storageAccountPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.StorageAccount);
			crit.Add(Restrictions.Eq(storageAccountPropertyName, account));

			string transferStatusPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.TransferStatus);
			crit.Add(Restrictions.Eq(transferStatusPropertyName, TransferStatus.COMPLETED));

			if (version != null)
			{
				DateTime fileLastWrittenAt = DateTime.ParseExact(version, Models.BackupedFile.VersionFormat, CultureInfo.InvariantCulture);
				string fileLastWrittenAtPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.FileLastWrittenAt);
				crit.Add(Restrictions.Eq(fileLastWrittenAtPropertyName, fileLastWrittenAt));
			}

			string filePropertyName = this.GetPropertyName((Models.BackupedFile x) => x.File);
			string filePathPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.Path);
			crit.CreateAlias(filePropertyName, "f");
			SimpleExpression expr = Restrictions.Eq("f." + filePathPropertyName, path);
			if (ignoreCase)
				expr = expr.IgnoreCase();
			crit.Add(expr);
			return crit.List<Models.BackupedFile>();
		}

		public IList<Models.BackupedFile> GetCompleteByPlanAndPath(Models.BackupPlan plan, string path, bool ignoreCase = false)
		{
			Assert.IsNotNull(plan);
			Assert.IsNotNullOrEmpty(path);
			ICriteria crit = Session.CreateCriteria(PersistentType);

			// By status
			string transferStatusPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.TransferStatus);
			crit.Add(Restrictions.Eq(transferStatusPropertyName, TransferStatus.COMPLETED));

			// By plan
			string backupPlanPropertyName = this.GetPropertyName((Models.Backup x) => x.BackupPlan);
			string backupPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.Backup);
			crit.CreateAlias(backupPropertyName, "bp");
			crit.Add(Restrictions.Eq("bp." + backupPlanPropertyName, plan));

			// By path
			string filePropertyName = this.GetPropertyName((Models.BackupedFile x) => x.File);
			string filePathPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.Path);
			crit.CreateAlias(filePropertyName, "f");
			SimpleExpression expr = Restrictions.Eq("f." + filePathPropertyName, path);
			if (ignoreCase)
				expr = expr.IgnoreCase();
			crit.Add(expr);

			// Order
			string idPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.Id);
			crit.AddOrder(Order.Desc(idPropertyName));

			return crit.List<Models.BackupedFile>();
		}
	}

	#endregion

	#region Restore

	public class RestorePlanRepository : BaseRepository<Models.RestorePlan, Int32?>
	{
		public RestorePlanRepository()
		{
		}

		public RestorePlanRepository(ISession session)
			: base(session)
		{
		}

		public IList<Models.RestorePlan> GetAllActive()
		{
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string isDeletedPropertyName = this.GetPropertyName((Models.RestorePlan x) => x.IsDeleted);
			crit.Add(Restrictions.Eq(isDeletedPropertyName, false));
			string lastRunAtPropertyName = this.GetPropertyName((Models.RestorePlan x) => x.LastRunAt);
			return crit.List<Models.RestorePlan>();
		}

		[ThreadStatic]
		private static Random _Random = new Random(Environment.TickCount);

		public override void DeleteImpl(ITransaction tx, Models.RestorePlan instance)
		{
			// Update an instance property instead of actually deleting it.
			instance.IsDeleted = true;
			// Avoid Unique Key exceptions - The user might want to create another
			// plan using the same name of a deleted plan, so we append a randomized
			// string to the deleted one.
			instance.Name += "_DELETED_" + _Random.GenerateRandomHexaString(10);
		}
	}

	public class RestorePlanSourceEntryRepository : BaseRepository<Models.RestorePlanSourceEntry, Int64?>
	{
		public RestorePlanSourceEntryRepository()
		{
		}

		public RestorePlanSourceEntryRepository(ISession session)
			: base(session)
		{
		}
	}

	public class RestoreRepository : BaseRepository<Models.Restore, Int32?>
	{
		public RestoreRepository()
		{
		}

		public RestoreRepository(ISession session)
			: base(session)
		{
		}

		public Models.Restore GetLatestByPlan(Models.RestorePlan plan)
		{
			Assert.IsNotNull(plan);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string restorePlanPropertyName = this.GetPropertyName((Models.Restore x) => x.RestorePlan);
			crit.Add(Restrictions.Eq(restorePlanPropertyName, plan));
			string idPropertyName = this.GetPropertyName((Models.Restore x) => x.Id);
			crit.AddOrder(Order.Desc(idPropertyName));
			crit.SetMaxResults(1);
			return crit.UniqueResult<Models.Restore>();
		}
	}

	public class RestorePlanFileRepository : BaseRepository<Models.RestorePlanFile, Int64?>
	{
		public RestorePlanFileRepository()
		{
		}

		public RestorePlanFileRepository(ISession session)
			: base(session)
		{
		}

		//public RestorePlanFileRepository()
		//{
		//	BeforeInsert = (ITransaction tx, Models.RestorePlanFile instance) =>
		//	{
		//		instance.CreatedAt = DateTime.UtcNow;
		//	};
		//	BeforeUpdate = (ITransaction tx, Models.RestorePlanFile instance) =>
		//	{
		//		instance.UpdatedAt = DateTime.UtcNow;
		//	};
		//}

		//public Models.RestorePlanFile GetByPath(string path, bool ignoreCase = false)
		//{
		//	Assert.IsNotNullOrEmpty(path);
		//	ICriteria crit = Session.CreateCriteria(PersistentType);
		//	string pathPropertyName = this.GetPropertyName((Models.RestorePlanFile x) => x.Path);
		//	SimpleExpression expr = Restrictions.Eq(pathPropertyName, path);
		//	if (ignoreCase)
		//		expr = expr.IgnoreCase();
		//	crit.Add(expr);
		//	return crit.UniqueResult<Models.RestorePlanFile>();
		//}

		public IList<Models.RestorePlanFile> GetAllByPlan(Models.RestorePlan plan)
		{
			Assert.IsNotNull(plan);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string restorePlanPropertyName = this.GetPropertyName((Models.RestorePlanFile x) => x.RestorePlan);
			crit.Add(Restrictions.Eq(restorePlanPropertyName, plan));
			return crit.List<Models.RestorePlanFile>();
		}
	}

	public class RestoredFileRepository : BaseRepository<Models.RestoredFile, Int64?>
	{
		public RestoredFileRepository()
		{
		}

		public RestoredFileRepository(ISession session)
			: base(session)
		{
		}

		//public RestoredFileRepository()
		//{
		//	BeforeInsert = (ITransaction tx, Models.RestoredFile instance) =>
		//	{
		//		instance.UpdatedAt = DateTime.UtcNow;
		//	};
		//	BeforeUpdate = (ITransaction tx, Models.RestoredFile instance) =>
		//	{
		//		instance.UpdatedAt = DateTime.UtcNow;
		//	};
		//}

		public Models.RestoredFile GetByRestoreAndPath(Models.Restore restore, string path, bool ignoreCase = false)
		{
			Assert.IsNotNull(restore);
			Assert.IsNotNullOrEmpty(path);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string restorePropertyName = this.GetPropertyName((Models.RestoredFile x) => x.Restore);
			string filePropertyName = this.GetPropertyName((Models.RestoredFile x) => x.File);
			string filePathPropertyName = this.GetPropertyName((Models.RestorePlanFile x) => x.Path);
			crit.CreateAlias(filePropertyName, "f");
			crit.Add(Restrictions.Eq(restorePropertyName, restore));
			SimpleExpression expr = Restrictions.Eq("f." + filePathPropertyName, path);
			if (ignoreCase)
				expr = expr.IgnoreCase();
			crit.Add(expr);
			return crit.UniqueResult<Models.RestoredFile>();
		}

		//public IList<Models.RestoredFile> GetByRestoreAndStatus(Models.Restore restore, params TransferStatus[] statuses)
		//{
		//	Assert.IsNotNull(restore);
		//	Assert.IsNotNull(statuses);
		//	ICriteria crit = Session.CreateCriteria(PersistentType);
		//	string restorePropertyName = this.GetPropertyName((Models.RestoredFile x) => x.Restore);
		//	string transferStatusPropertyName = this.GetPropertyName((Models.RestoredFile x) => x.TransferStatus);
		//	crit.Add(Restrictions.Eq(restorePropertyName, restore));
		//	crit.Add(Restrictions.In(transferStatusPropertyName, statuses));
		//	return crit.List<Models.RestoredFile>();
		//}
	}

	#endregion

	#region Synchronization

	public class SynchronizationRepository : BaseRepository<Models.Synchronization, Int32?>
	{
		public SynchronizationRepository()
		{
		}

		public SynchronizationRepository(ISession session)
			: base(session)
		{
		}

		public Models.Synchronization GetLatestByStorageAccount(Models.StorageAccount account)
		{
			Assert.IsNotNull(account);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string storageAccountPropertyName = this.GetPropertyName((Models.Synchronization x) => x.StorageAccount);
			crit.Add(Restrictions.Eq(storageAccountPropertyName, account));
			string idPropertyName = this.GetPropertyName((Models.Synchronization x) => x.Id);
			crit.AddOrder(Order.Desc(idPropertyName));
			crit.SetMaxResults(1);
			return crit.UniqueResult<Models.Synchronization>();
		}
	}

	#endregion
}
