using NHibernate;
using NUnit.Framework;
using System;
using System.Data;

namespace Teltec.Backup.Data.DAO.NH
{
	public class BatchTransaction : ITransaction, IDisposable
	{
		internal ISession Session { get; private set; }
		private ITransaction CurrentTransaction;

		internal BatchTransaction(ISession session)
		{
			Session = session;
			CurrentTransaction = Session.BeginTransaction();
		}

		private void DisposeTransaction()
		{
			Assert.IsNotNull(CurrentTransaction);
			CurrentTransaction.Dispose();
			CurrentTransaction = null;
		}

		public void CommitAndRenew()
		{
			Assert.IsNotNull(CurrentTransaction);

			Commit();

			// Internally, the `Commit` method already disposes the transaction.
			// REFERENCE: https://github.com/nhibernate/nhibernate-core/blob/1c74ebd373db6beec4d0f510a05354191074b1ee/src/NHibernate/Transaction/AdoTransaction.cs#L203
			//CurrentTransaction.Dispose();

			CurrentTransaction = Session.BeginTransaction();
		}

		#region ITransaction

		public void Begin(IsolationLevel isolationLevel)
		{
			Assert.IsNotNull(CurrentTransaction);
			CurrentTransaction.Begin(isolationLevel);
		}

		public void Begin()
		{
			Assert.IsNotNull(CurrentTransaction);
			CurrentTransaction.Begin();
		}

		public void Commit()
		{
			Assert.IsNotNull(CurrentTransaction);
			CurrentTransaction.Commit();
		}

		public void Enlist(IDbCommand command)
		{
			Assert.IsNotNull(CurrentTransaction);
			CurrentTransaction.Enlist(command);
		}

		public bool IsActive
		{
			get { return CurrentTransaction == null ? false : CurrentTransaction.IsActive; }
		}

		public void RegisterSynchronization(NHibernate.Transaction.ISynchronization synchronization)
		{
			Assert.IsNotNull(CurrentTransaction);
			CurrentTransaction.RegisterSynchronization(synchronization);
		}

		public bool WasCommitted
		{
			get { return CurrentTransaction == null ? false : CurrentTransaction.WasCommitted; }
		}

		public bool WasRolledBack
		{
			get { return CurrentTransaction == null ? false : CurrentTransaction.WasRolledBack; }
		}

		public void Rollback()
		{
			Assert.IsNotNull(CurrentTransaction);
			CurrentTransaction.Rollback();
		}

		#endregion

		#region IDiposable

		bool disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				if (CurrentTransaction != null)
				{
					CurrentTransaction.Dispose();
					CurrentTransaction = null;
				}
			}

			disposed = true;
		}

		~BatchTransaction()
		{
			Dispose(false);
		}

		#endregion
	}
}
