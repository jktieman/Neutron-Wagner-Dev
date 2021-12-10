using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeutronData.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;


        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }
        public IEnumerable<TEntity> All()
        {
            return _dbSet.AsNoTracking().ToList();
        }

        public IEnumerable<TEntity> AllInclude(
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return GetAllIncluding(includeProperties).ToList();
        }

        public IQueryable<TEntity> GetAllIncluding(Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> queryable = _dbSet.AsNoTracking();
            return includeProperties.Aggregate(
                queryable, (current, includeProperty) => current.Include(includeProperty));
        }

        public IEnumerable<TEntity> FindByInclude(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = GetAllIncluding(includeProperties);
            IEnumerable<TEntity> results = query.Where(predicate).ToList();
            return results;
        }

        public IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
        {
            IEnumerable<TEntity> results = _dbSet.AsNoTracking()
                .Where(predicate).ToList();
            return results;
        }

        public TEntity FindByKey(int? id)
        {
            var rec = _dbSet.AsNoTracking().FirstOrDefault(s => s.Id == id);
            return rec;
        }

        public void Insert(TEntity entity)
        {
            var t = typeof(TEntity);

            try
            {
                var local = _context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }

                _dbSet.Add(entity);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Insert Error.  {ex.Message} \r\n {ex.InnerException}");
            }
        }

        public async Task InsertAsync(TEntity entity)
        {

            try
            {
                var local = _context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }

                _dbSet.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Insert Error.  {ex.Message} \r\n {ex.InnerException} \r\n {ex.InnerException.Message}");
            }
        }

        public void Update(TEntity entity)
        {
            try
            {
                var local = _context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }
                _context.Set<TEntity>().AddOrUpdate(entity);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update Error.  {ex.Message} \r\n {ex.InnerException} \r\n {ex.InnerException.Message} \r\n {ex.InnerException.InnerException.Message}");
            }
        }


        public void Delete(int id)
        {
            try
            {
                var local = _context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == id);
                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }

                var entity = FindByKey(id);
                _dbSet.Attach(entity);
                _dbSet.Remove(entity);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete Error.  " + ex.Message);
            }
        }

    }
}
