﻿using System;
using Teltec.Backup.Models;
using Teltec.Data;

namespace Teltec.Backup
{
	public class DBContextScope : IDisposable
	{
		public readonly DatabaseContext Context = Provider.DBContext;

		private GenericRepository<AmazonS3Account> _AmazonS3Accounts;
		public GenericRepository<AmazonS3Account> AmazonS3Accounts
		{
			get
			{
				if (_AmazonS3Accounts == null)
					_AmazonS3Accounts = new GenericRepository<AmazonS3Account>(Context);
				return _AmazonS3Accounts;
			}
		}

		private GenericRepository<BackupPlan> _BackupPlans;
		public GenericRepository<BackupPlan> BackupPlans
		{
			get
			{
				if (_BackupPlans == null)
					_BackupPlans = new GenericRepository<BackupPlan>(Context);
				return _BackupPlans;
			}
		}

		private GenericRepository<BackupPlanSourceEntry> _BackupPlanSourceEntries;
		public GenericRepository<BackupPlanSourceEntry> BackupPlanSourceEntries
		{
			get
			{
				if (_BackupPlanSourceEntries == null)
					_BackupPlanSourceEntries = new GenericRepository<BackupPlanSourceEntry>(Context);
				return _BackupPlanSourceEntries;
			}
		}

		public int Save()
		{
			return Context.SaveChanges();
		}

		private bool _disposed = false;

		~DBContextScope()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					//if (Context != null)
					//	Context.Dispose();
				}
				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
