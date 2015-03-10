using NHibernate;
using NHibernate.Criterion;
using System;
using System.Linq;
using System.Collections.Generic;
using Teltec.Common.Extensions;

namespace Teltec.Backup.App.DAO
{
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

		public Models.Backup GetLatest(Models.BackupPlan plan)
		{
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string idPropertyName = this.GetPropertyName((Models.Backup x) => x.Id);
			crit.AddOrder(Order.Desc(idPropertyName));
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

		public Models.BackupPlanFile GetByPath(string path, bool ignoreCase = false)
		{
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string pathPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.Path);
			SimpleExpression expr = Restrictions.Eq(pathPropertyName, path);
			if (ignoreCase)
				expr = expr.IgnoreCase();
			crit.Add(expr);
			return crit.UniqueResult<Models.BackupPlanFile>();
		}

		public IList<Models.BackupPlanFile> GetAllByPlan(Models.BackupPlan plan)
		{
			ICriteria crit = Session.CreateCriteria(PersistentType);
			string backupPlanPropertyName = this.GetPropertyName((Models.BackupPlanFile x) => x.BackupPlan);
			crit.Add(Restrictions.Eq(backupPlanPropertyName, plan));
			return crit.List<Models.BackupPlanFile>();
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
	}
}
