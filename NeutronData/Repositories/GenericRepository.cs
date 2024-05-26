using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AlliedLogger;
using AsyncAwaitBestPractices;

namespace NeutronData.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly DbContext _context;
        private IDynamicLogger _logger;
        private readonly DbSet<TEntity> _dbSet;


        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
           // Init();
        }

        //private void Init()
        //{
        //    _logger = NeutronCore.Global.Logger.SetupLogger("GenericRepository");
        //}

        public IEnumerable<TEntity> All()
        {
            return _dbSet.ToList();
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
            var rec = _dbSet.FirstOrDefault(s => s.Id == id);
            return rec;
        }

        public void Insert(TEntity entity)
        {
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
                _logger.LogDetailAsync($"Insert Error.  {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
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
                _logger.LogDetailAsync($"Insert Async Error.  {ex.Message} {Environment.NewLine} {ex.InnerException} {Environment.NewLine}").SafeFireAndForget();
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
                _logger.LogDetailAsync($"Update Error.  {ex.Message}{Environment.NewLine} {ex.InnerException} {Environment.NewLine}{ex.InnerException?.Message}{Environment.NewLine} {ex.InnerException?.InnerException?.Message}").SafeFireAndForget();
            }
        }

        public async Task UpdateAsync(TEntity entity)
        {
            try
            {
                var local = _context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }
                _context.Set<TEntity>().AddOrUpdate(entity);
               await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Update Error.  {ex.Message}{Environment.NewLine} {ex.InnerException} {Environment.NewLine}{ex.InnerException?.Message}{Environment.NewLine} {ex.InnerException?.InnerException?.Message}").SafeFireAndForget();
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
                _logger.LogDetailAsync("Delete Error.  " + ex.Message).SafeFireAndForget();
            }
        }

    }
}
