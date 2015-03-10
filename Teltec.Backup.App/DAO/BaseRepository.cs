using NHibernate;
using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Teltec.Backup.App.DAO.NHibernate;
using Teltec.Backup.App.Models;

namespace Teltec.Backup.App.DAO
{
	public delegate void BaseRepositoryEventHandler<T>(ITransaction tx, T instance);

	public abstract class BaseRepository<T, ID> where T : BaseEntity<ID>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private bool _canDisposeSession = false;

		protected Type _PersistentType = typeof(T);
		protected Type PersistentType { get { return _PersistentType; } }

		protected BaseRepositoryEventHandler<T> BeforeInsert;
		protected BaseRepositoryEventHandler<T> AfterInsert;

		protected BaseRepositoryEventHandler<T> BeforeUpdate;
		protected BaseRepositoryEventHandler<T> AfterUpdate;
		
		protected BaseRepositoryEventHandler<T> BeforeInsertOrUpdate;
		protected BaseRepositoryEventHandler<T> AfterInsertOrUpdate;

		protected BaseRepositoryEventHandler<T> BeforeDelete;
		protected BaseRepositoryEventHandler<T> AfterDelete;

		public BaseRepository()
		{
			_canDisposeSession = true;
		}

		public BaseRepository(ISession session)
		{
			_canDisposeSession = false;
			Session = session;
		}

		public T Get(ID id)
		{
			return Session.Get<T>(id);
		}

		public T GetForReadOnly(ID id)
		{
			T obj = Session.Get<T>(id);
			Session.SetReadOnly(obj, true);
			return obj;
		}

		public void Refresh(T instance)
		{
			if (!IsTransient(Session, instance))
				Session.Refresh(instance);
		}

		public void Insert(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				if (BeforeInsert != null)
					BeforeInsert(tx, instance);
				
				Session.Save(instance);
				
				if (AfterInsert != null)
					AfterInsert(tx, instance);

				tx.Commit();
			}
		}

		public void Insert(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				foreach (T item in items)
				{
					if (BeforeInsert != null)
						BeforeInsert(tx, item);
					
					Session.Save(item);

					if (AfterInsert != null)
						AfterInsert(tx, item);
				}
				tx.Commit();
			}
		}

		public void InsertOrUpdate(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				if (BeforeInsertOrUpdate != null)
					BeforeInsertOrUpdate(tx, instance);

				Session.SaveOrUpdate(instance);

				if (AfterInsertOrUpdate != null)
					AfterInsertOrUpdate(tx, instance);

				tx.Commit();
			}
		}

		public void InsertOrUpdate(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				foreach (T item in items)
				{
					if (BeforeInsertOrUpdate != null)
						BeforeInsertOrUpdate(tx, item);

					Session.SaveOrUpdate(item);

					if (AfterInsertOrUpdate != null)
						AfterInsertOrUpdate(tx, item);
				}
				tx.Commit();
			}
		}

		public void Update(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				if (BeforeUpdate != null)
					BeforeUpdate(tx, instance);
				
				// Remove `instance` from the cache - detach it.
				//Session.Evict(instance);
				// Make the detached entity persistent (with the non-flushed changes) .				
				Session.Update(instance);
				
				if (AfterUpdate != null)
					AfterUpdate(tx, instance);

				tx.Commit();
			}
		}

		public void Update(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				foreach (T item in items)
				{
					if (BeforeUpdate != null)
						BeforeUpdate(tx, item);

					Session.Update(item);
					
					if (AfterUpdate != null)
						AfterUpdate(tx, item);
				}
				tx.Commit();
			}
		}

		public void Delete(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				if (BeforeDelete != null)
					BeforeDelete(tx, instance);

				Session.Delete(instance);

				if (AfterDelete != null)
					AfterDelete(tx, instance);

				tx.Commit();
			}
		}

		public void Delete(ID id)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				// We do `Get` because `session.Delete()` always load the entity anyway.
				// See http://stackoverflow.com/a/1323461/298054
				T instance = Get(id);

				if (BeforeDelete != null)
					BeforeDelete(tx, instance);

				Session.Delete(instance);

				if (AfterDelete != null)
					AfterDelete(tx, instance);
	
				tx.Commit();
			}
		}

		public void Delete(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				foreach (T item in items)
				{
					if (BeforeDelete != null)
						BeforeDelete(tx, item);

					Session.Delete(item);

					if (AfterDelete != null)
						AfterDelete(tx, item);
				}
				tx.Commit();
			}
		}

		public List<T> GetAll()
		{
			return Session.Query<T>().ToList();
		}

		public IQueryable<T> QueryMany(Expression<System.Func<T, bool>> expression)
		{
			return Session.Query<T>().Where(expression).AsQueryable();
		}

		public T Query(Expression<System.Func<T, bool>> expression)
		{
			return QueryMany(expression).SingleOrDefault();
		}

		protected bool IsTransient(ISession session, T obj)
		{
			return NHibernate.NHibernateHelper.IsTransient(Session, obj);
		}

		public ISession GetSession()
		{
			return NHibernate.NHibernateHelper.GetSession();
		}

		public ISession _Session;
		public ISession Session
		{
			get
			{
				if (_Session == null)
					_Session = GetSession();
				return _Session;
			}

			set
			{
				if (_Session != null)
				{
					if (value != null)
						logger.Warn("ATTENTION! Attempt to overwrite an ISession. Forcing a Dispose().");
					_Session.Dispose();
				}
				_Session = value;
			}
		}
	}
}
