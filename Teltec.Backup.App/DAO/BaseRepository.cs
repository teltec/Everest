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
	public abstract class BaseRepository<T, ID> where T : BaseEntity<ID>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
				Session.Save(instance);
				tx.Commit();
			}
		}

		public void Insert(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				foreach (T item in items)
					Session.Save(item);
				tx.Commit();
			}
		}

		public void InsertOrUpdate(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				Session.SaveOrUpdate(instance);
				tx.Commit();
			}
		}

		public void InsertOrUpdate(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				foreach (T item in items)
					Session.SaveOrUpdate(item);
				tx.Commit();
			}
		}

		public void Update(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				// Remove `instance` from the cache - detach it.
				//Session.Evict(instance);
				// Make the detached entity persistent (with the non-flushed changes) .
				Session.Update(instance);
				tx.Commit();
			}
		}

		public void Update(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				foreach (T item in items)
					Session.Update(item);
				tx.Commit();
			}
		}

		public void Delete(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				Session.Delete(instance);
				tx.Commit();
			}
		}

		public void Delete(ID id)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				// We do `Get` because `session.Delete()` always load the entity anyway.
				// See http://stackoverflow.com/a/1323461/298054
				T obj = Get(id);
				Session.Delete(obj);
				tx.Commit();
			}
		}

		public void Delete(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				foreach (T item in items)
					Session.Delete(item);
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
