/* 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using NHibernate;
using NHibernate.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Teltec.Everest.Data.Models;

namespace Teltec.Everest.Data.DAO
{
	public delegate void BaseRepositoryEventHandler<T>(ITransaction tx, T instance);

	public abstract class BaseRepository<T, ID> where T : BaseEntity<ID>
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		//private bool _canDisposeSession = false;
		//private bool _canDisposeStatelessSession = false;

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
			//_canDisposeSession = true;
		}

		public BaseRepository(ISession session)
		{
			//_canDisposeSession = false;
			Session = session;
		}

		public BaseRepository(IStatelessSession session)
		{
			//_canDisposeStatelessSession = false;
			StatelessSession = session;
		}

		public T Get(ID id)
		{
			return Session.Get<T>(id);
		}

		public T Load(ID id)
		{
			return Session.Load<T>(id);
		}

		public T GetStateless(ID id)
		{
			return StatelessSession.Get<T>(id);
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
				Insert(tx, instance);
				tx.Commit();
			}
		}

		public void Insert(ITransaction tx, T instance)
		{
			if (BeforeInsert != null)
				BeforeInsert(tx, instance);

			InsertImpl(tx, instance);

			if (AfterInsert != null)
				AfterInsert(tx, instance);
		}

		public void Insert(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				Insert(tx, items);
				tx.Commit();
			}
		}

		public void Insert(ITransaction tx, IEnumerable<T> items)
		{
			foreach (T item in items)
			{
				if (BeforeInsert != null)
					BeforeInsert(tx, item);

				InsertImpl(tx, item);

				if (AfterInsert != null)
					AfterInsert(tx, item);
			}
		}

		public virtual void InsertImpl(ITransaction tx, T instance)
		{
			Session.Save(instance);
		}

		public void InsertOrUpdate(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				InsertOrUpdate(tx, instance);
				tx.Commit();
			}
		}

		public void InsertOrUpdate(ITransaction tx, T instance)
		{
			if (BeforeInsertOrUpdate != null)
				BeforeInsertOrUpdate(tx, instance);

			InsertOrUpdateImpl(tx, instance);

			if (AfterInsertOrUpdate != null)
				AfterInsertOrUpdate(tx, instance);
		}

		public void InsertOrUpdate(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				InsertOrUpdate(tx, items);
				tx.Commit();
			}
		}

		public void InsertOrUpdate(ITransaction tx, IEnumerable<T> items)
		{
			foreach (T item in items)
			{
				if (BeforeInsertOrUpdate != null)
					BeforeInsertOrUpdate(tx, item);

				InsertOrUpdateImpl(tx, item);

				if (AfterInsertOrUpdate != null)
					AfterInsertOrUpdate(tx, item);
			}
		}

		public virtual void InsertOrUpdateImpl(ITransaction tx, T instance)
		{
			Session.SaveOrUpdate(instance);
		}

		public void Update(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				Update(tx, instance);
				tx.Commit();
			}
		}

		public void Update(ITransaction tx, T instance)
		{
			if (BeforeUpdate != null)
				BeforeUpdate(tx, instance);

			UpdateImpl(tx, instance);

			if (AfterUpdate != null)
				AfterUpdate(tx, instance);
		}

		public void Update(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				Update(tx, items);
				tx.Commit();
			}
		}

		public void Update(ITransaction tx, IEnumerable<T> items)
		{
			foreach (T item in items)
			{
				if (BeforeUpdate != null)
					BeforeUpdate(tx, item);

				UpdateImpl(tx, item);

				if (AfterUpdate != null)
					AfterUpdate(tx, item);
			}
		}

		public virtual void UpdateImpl(ITransaction tx, T instance)
		{
			// Remove `instance` from the cache - detach it.
			//Session.Evict(instance);
			// Make the detached entity persistent (with the non-flushed changes) .
			Session.Update(instance);
		}

		public void Delete(T instance)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				Delete(tx, instance);
				tx.Commit();
			}
		}

		public void Delete(ITransaction tx, T instance)
		{
			if (BeforeDelete != null)
				BeforeDelete(tx, instance);

			DeleteImpl(tx, instance);

			if (AfterDelete != null)
				AfterDelete(tx, instance);
		}

		public void Delete(ID id)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				Delete(tx, id);
				tx.Commit();
			}
		}

		public void Delete(ITransaction tx, ID id)
		{
			// We do `Get` because `session.Delete()` always load the entity anyway.
			// See http://stackoverflow.com/a/1323461/298054
			T instance = Get(id);

			if (BeforeDelete != null)
				BeforeDelete(tx, instance);

			DeleteImpl(tx, instance);

			if (AfterDelete != null)
				AfterDelete(tx, instance);
		}

		public void Delete(IEnumerable<T> items)
		{
			using (ITransaction tx = Session.BeginTransaction())
			{
				Delete(tx, items);
				tx.Commit();
			}
		}

		public void Delete(ITransaction tx, IEnumerable<T> items)
		{
			foreach (T item in items)
			{
				if (BeforeDelete != null)
					BeforeDelete(tx, item);

				DeleteImpl(tx, item);

				if (AfterDelete != null)
					AfterDelete(tx, item);
			}
		}

		public virtual void DeleteImpl(ITransaction tx, T instance)
		{
			Session.Delete(instance);
		}

		public List<T> GetAll()
		{
			return Session.Query<T>().ToList();
		}

		public List<T> GetAllStateless()
		{
			return StatelessSession.Query<T>().ToList();
		}

		public IQueryable<T> QueryMany(Expression<System.Func<T, bool>> expression)
		{
			return Session.Query<T>().Where(expression).AsQueryable();
		}

		public IQueryable<T> QueryManyStateless(Expression<System.Func<T, bool>> expression)
		{
			return StatelessSession.Query<T>().Where(expression).AsQueryable();
		}

		public T Query(Expression<System.Func<T, bool>> expression)
		{
			return QueryMany(expression).SingleOrDefault();
		}

		public T QueryStateless(Expression<System.Func<T, bool>> expression)
		{
			return QueryManyStateless(expression).SingleOrDefault();
		}

		protected bool IsTransient(ISession session, T obj)
		{
			return NH.NHibernateHelper.IsTransient(Session, obj);
		}

		public ISession GetSession()
		{
			return NH.NHibernateHelper.GetSession();
		}

		public IStatelessSession GetStatelessSession()
		{
			return NH.NHibernateHelper.GetStatelessSession();
		}

		private ISession _Session;
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

		private IStatelessSession _StatelessSession;
		public IStatelessSession StatelessSession
		{
			get
			{
				if (_StatelessSession == null)
					_StatelessSession = GetStatelessSession();
				return _StatelessSession;
			}

			set
			{
				if (_StatelessSession != null)
				{
					if (value != null)
						logger.Warn("ATTENTION! Attempt to overwrite an ISession. Forcing a Dispose().");
					_StatelessSession.Dispose();
				}
				_StatelessSession = value;
			}
		}
	}
}
