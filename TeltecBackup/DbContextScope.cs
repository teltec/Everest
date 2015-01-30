using System;
using Teltec.Backup.Models;
using Teltec.Data;

namespace Teltec.Backup
{
	public class DBContextScope : IDisposable
	{
		private DatabaseContext _context = new DatabaseContext();
		private GenericRepository<AmazonS3Account> _AmazonS3Accounts;

		public GenericRepository<AmazonS3Account> AmazonS3Accounts
		{
			get
			{
				if (_AmazonS3Accounts == null)
					_AmazonS3Accounts = new GenericRepository<AmazonS3Account>(_context);
				return _AmazonS3Accounts;
			}
		}

		public int Save()
		{
			return _context.SaveChanges();
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
					if (_context != null)
						_context.Dispose();
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
