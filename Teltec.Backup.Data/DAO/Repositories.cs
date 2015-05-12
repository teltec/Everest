using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Teltec.Common.Extensions;
using Teltec.Storage;

namespace Teltec.Backup.Data.DAO
{
	#region Accounts

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

		public IList<Models.BackupPlanFile> GetAllByPlan(Models.BackupPlan plan)
		{
			Assert.IsNotNull(plan);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string backupPlanPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.BackupPlan);
			crit.Add(Restrictions.Eq(backupPlanPropertyName, plan));
			return crit.List<Models.BackupPlanFile>();
		}

		public Models.BackupPlanFile GetByPlanAndPath(Models.BackupPlan plan, string path, bool ignoreCase = false)
		{
			Assert.IsNotNullOrEmpty(path);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string backupPlanPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.BackupPlan);
			crit.Add(Restrictions.Eq(backupPlanPropertyName, plan));
			string pathPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.Path);
			SimpleExpression expr = Restrictions.Eq(pathPropertyName, path);
			if (ignoreCase)
				expr = expr.IgnoreCase();
			crit.Add(expr);
			return crit.UniqueResult<Models.BackupPlanFile>();
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

		public IList<Models.BackupPlanPathNode> GetAllDrivesByPlan(Models.BackupPlan plan)
		{
			Assert.IsNotNull(plan);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string backupPlanPropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.BackupPlan);
			string typePropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.Type);
			crit.Add(Restrictions.Eq(backupPlanPropertyName, plan));
			crit.Add(Restrictions.Eq(typePropertyName, Models.EntryType.DRIVE));
			return crit.List<Models.BackupPlanPathNode>();
		}

		public Models.BackupPlanPathNode GetByPlanAndTypeAndPath(Models.BackupPlan plan, Models.EntryType type, string path, bool ignoreCase = false)
		{
			Assert.IsNotNull(plan);
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string backupPlanPropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.BackupPlan);
			string typePropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.Type);
			string pathPropertyName = this.GetPropertyName((Models.BackupPlanPathNode x) => x.Path);
			crit.Add(Restrictions.Eq(backupPlanPropertyName, plan));
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

		public IList<Models.BackupedFile> GetCompletedByPlanAndPath(Models.BackupPlan plan, string path, bool ignoreCase = false)
		{
			Assert.IsNotNull(plan);
			Assert.IsNotNullOrEmpty(path);
			ICriteria crit = Session.CreateCriteria(PersistentType);

			string backupPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.Backup);
			string backupPlanPropertyName = this.GetPropertyName((Models.Backup x) => x.BackupPlan);
			crit.CreateAlias(backupPropertyName, "bkp");
			crit.Add(Restrictions.Eq("bkp." + backupPlanPropertyName, plan)); 
			
			string transferStatusPropertyName = this.GetPropertyName((Models.BackupedFile x) => x.TransferStatus);
			crit.Add(Restrictions.Eq(transferStatusPropertyName, TransferStatus.COMPLETED));

			string filePropertyName = this.GetPropertyName((Models.BackupedFile x) => x.File);
			string filePathPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.Path);
			crit.CreateAlias(filePropertyName, "f");
			SimpleExpression expr = Restrictions.Eq("f." + filePathPropertyName, path);
			if (ignoreCase)
				expr = expr.IgnoreCase();
			crit.Add(expr);
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
}
