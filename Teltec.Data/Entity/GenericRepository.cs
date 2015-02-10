using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Teltec.Data.Entity
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        internal DbContext _context;
        internal DbSet<TEntity> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public DbSet<TEntity> Objects
        {
            get { return _dbSet; }
        }

        public virtual TEntity Get(object id)
        {
            return _dbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            _dbSet.Attach(entity); // Attaches whole entity graph to the new context with `Unchanged` entity state
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(object id)
        {
            TEntity entity = _dbSet.Find(id);
            Delete(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                // Attaches whole entity graph to the new context with `Unchanged` entity state.
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public virtual void Reload(TEntity entity)
        {
            _context.Entry(entity).Reload();
        }

        public virtual void UndoChanges(TEntity entity)
        {
            // Under the covers, changing the state of an entity from  
            // Modified to Unchanged first sets the values of all  
            // properties to the original values that were read from  
            // the database when it was queried, and then marks the  
            // entity as Unchanged. This will also reject changes to  
            // FK relationships since the original value of the FK  
            // will be restored. 
            _context.Entry(entity).State = EntityState.Unchanged;
        }

        // Usage:
        //  .Filter(includeProperties: "SomeProperty")
        //  .Filter(orderBy: q => q.OrderBy(o => o.SomeProperty));
        public virtual IEnumerable<TEntity> Find(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var property in includeProperties.Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }

            return orderBy != null ? orderBy(query).ToList() : query.ToList();
        }
    }
}
