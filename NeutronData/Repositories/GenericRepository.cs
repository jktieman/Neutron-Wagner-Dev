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
using System.Threading;
using NeutronData.DataContexts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace NeutronData.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        // private IDynamicLogger _logger;
        //private readonly DbSet<TEntity> _dbSet;
       // private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Func<NeutronDb> _contextFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<MemoryCacheOptions> _cacheOptions;
        //private DbContext _context;

        public GenericRepository(Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            //_context = _contextFactory();

            //_dbSet = _context.Set<TEntity>();
            // SetupLogger();
        }

        public GenericRepository(Func<NeutronDb> contextFactory, IMemoryCache memoryCache, IOptions<MemoryCacheOptions> cacheOptions)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            //_context = _contextFactory();
            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions;
            //_dbSet = _context.Set<TEntity>();
            // SetupLogger();
        }

        private void SetupLogger()
        {
            // var entityType = typeof(TEntity).Name;
            // _logger = NeutronCore.Global.Logger.SetupLogger($"GenericRepository-{entityType}");
        }

        //public IEnumerable<TEntity> All()
        //{
        //    return _dbSet.ToList();
        //}

        public IEnumerable<TEntity> All(
            Expression<Func<TEntity, bool>> filter = null,
            int? skip = null,
            int? take = null)
        {
            using (var context = _contextFactory())
            {
                  IQueryable<TEntity> query = context.Set<TEntity>().AsNoTracking();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }
            if (take.HasValue)
                
            {
                query = query.Take(take.Value);
            }
            return query.ToList();
            }
          

        }


        public async Task<IEnumerable<TEntity>> AllAsync(
            Expression<Func<TEntity, bool>> filter = null,
            int? skip = null,
            int? take = null,
            CancellationToken cancellationToken = default)
        {
            using (var context = _contextFactory())
            {
                IQueryable<TEntity> query = context.Set<TEntity>().AsNoTracking();
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (skip.HasValue)
                {
                    query = query.Skip(skip.Value);
                }

                if (take.HasValue)
                {
                    query = query.Take(take.Value);
                }

                return await query.ToListAsync(cancellationToken);
            }

            //if (_dbSet == null)
            //{
            //    throw new InvalidOperationException("The DbSet is not initialized.");
            //}
            //IQueryable<TEntity> query = _dbSet;
            //if (filter != null)
            //{
            //    query = query.Where(filter);
            //}
            //if (skip.HasValue)
            //{
            //    query = query.Skip(skip.Value);
            //}
            //if (take.HasValue)
            //{
            //    query = query.Take(take.Value);
            //}
            //return await query.ToListAsync(cancellationToken);
        }


        //public async Task<IEnumerable<TEntity>> AllAsync()
        //{
        //    return await _dbSet.ToListAsync();
        //}

        //AI Enhanced
        public IEnumerable<TEntity> AllInclude(
            Expression<Func<TEntity, bool>> filter,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            using (var context = _contextFactory())
            {
                IQueryable<TEntity> query = context.Set<TEntity>().AsNoTracking();
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
                if (filter != null)
                {
                    query = query.Where(filter);
                }
                return query.ToList();
            }

        }

        //IQueryable<TEntity> query = GetAllIncluding(includeProperties);
        //if (filter != null)
        //{
        //    query = query.Where(filter);
        //}
        //return query.ToList();



        //public IEnumerable<TEntity> AllInclude(
        //    params Expression<Func<TEntity, object>>[] includeProperties)
        //{
        //    return GetAllIncluding(includeProperties).ToList();
        //}

        //public IQueryable<TEntity> GetAllIncluding(Expression<Func<TEntity, object>>[] includeProperties)
        //{

        //    IQueryable<TEntity> queryable = _dbSet.AsNoTracking();
        //    return includeProperties.Aggregate(
        //        queryable, (current, includeProperty) => current.Include(includeProperty));
        //}

        public IEnumerable<TEntity> FindByInclude(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            using (var context = _contextFactory())
            {
                IQueryable<TEntity> query = context.Set<TEntity>().AsNoTracking();
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
                return query.Where(predicate).ToList();
            }
        }

        public IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
        {
            using (var context = _contextFactory())
            {
                return context.Set<TEntity>().AsNoTracking()
                    .Where(predicate).ToList();
            }
        }

        public async Task<List<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            using (var context = _contextFactory())
            {
                return await context.Set<TEntity>().AsNoTracking()
                    .Where(predicate).ToListAsync();
            }

            //await _semaphore.WaitAsync();
            //try
            //{
            //    var results = await _dbSet.AsNoTracking()
            //        .Where(predicate).ToListAsync();
            //    return results;
            //}
            //finally
            //{
            //    _semaphore.Release();
            //}
        }

        public async Task<TEntity> FindByFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            using (var context = _contextFactory())
            {
                return await context.Set<TEntity>().AsNoTracking()
                    .Where(predicate).FirstOrDefaultAsync();
            }

            //var result = await _dbSet.AsNoTracking()
            //    .Where(predicate).FirstOrDefaultAsync();
            //return result;
        }

        public TEntity FindByKeyInclude(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            using (var context = _contextFactory())
            {
                IQueryable<TEntity> query = context.Set<TEntity>().AsNoTracking();
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
                return query.Where(predicate).FirstOrDefault();
            }
        }

        public TEntity FindByKeyInclude(Expression<Func<TEntity, bool>> predicate,
            string[] stringIncludes,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            using (var context = _contextFactory())
            {
                IQueryable<TEntity> query = context.Set<TEntity>().AsNoTracking();
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
                query = stringIncludes.Aggregate(query, (current, include) => current.Include(include));
                return query.Where(predicate).FirstOrDefault();
            }
        }

        public async Task<TEntity> FindByKeyIncludeAsync(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            using (var context = _contextFactory())
            {
                IQueryable<TEntity> query = context.Set<TEntity>().AsNoTracking();
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
                return await query.Where(predicate).FirstOrDefaultAsync();
            }

            //var query = GetAllIncluding(includeProperties);
            //IEnumerable<TEntity> results = await query.Where(predicate).ToListAsync();
            //return results.FirstOrDefault();
        }

        public TEntity FindByKey(int id)  //took off the ? from int?
        {
            using (var context = _contextFactory())
            {
                return context.Set<TEntity>().AsNoTracking().FirstOrDefault(s => s.Id == id);
            }

            //var rec = _dbSet.FirstOrDefault(s => s.Id == id);
            //return rec;
        }
        public TEntity FindByKey(int? id)  //took off the ? from int?
        {
            if (id == null) return null;

            using (var context = _contextFactory())
            {
                return context.Set<TEntity>().AsNoTracking().FirstOrDefault(s => s.Id == id.Value);
            }

            //var rec = _dbSet.FirstOrDefault(s => s.Id == id);
            //return rec;
        }
        public async Task<TEntity> FindByKeyAsync(int? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), @"The id parameter cannot be null.");
            }
            try
            {
                using (var context = _contextFactory())
                {
                    return await context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(s => s.Id == id.Value);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logger)
                throw new InvalidOperationException("An error occurred while retrieving the entity.", ex);
            }

            //if (id == null)
            //{
            //    throw new ArgumentNullException(nameof(id), @"The id parameter cannot be null.");
            //}
            //try
            //{
            //    return await _dbSet.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id.Value);
            //}
            //catch (Exception ex)
            //{
            //    // Log the exception (e.g., using a logger)
            //    throw new InvalidOperationException("An error occurred while retrieving the entity.", ex);
            //}
        }
        public void Insert(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            try
            {
                using (var context = _contextFactory())
                {
                    var local = context.Set<TEntity>().Local.FirstOrDefault(f => f == entity);
                    if (local != null) context.Entry(local).State = EntityState.Detached;

                    context.Set<TEntity>().Add(entity);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "INSERT");
            }

            //if (entity == null) throw new ArgumentNullException(nameof(entity));

            //try
            //{
            //    //using (var context = _contextFactory())
            //    //{
            //    var local = _context.Set<TEntity>().Local.FirstOrDefault(f => f == entity);
            //    if (local != null) _context.Entry(local).State = EntityState.Detached;

            //    _dbSet.Add(entity);
            //    _context.SaveChanges();
            //    // }
            //}
            //catch (Exception ex)
            //{
            //    LogError(ex, "INSERT");
            //}
        }

        public async Task InsertAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                using (var context = _contextFactory())
                {
                    var local = context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
                    if (local != null)
                    {
                        context.Entry(local).State = EntityState.Detached;
                    }
                    context.Set<TEntity>().Add(entity);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "INSERT ASYNC");
            }

            //if (entity == null) throw new ArgumentNullException(nameof(entity));
            //try
            //{
            //    //using (var context = _contextFactory())
            //    //{
            //    var local = _context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
            //    if (local != null)
            //    {
            //        _context.Entry(local).State = EntityState.Detached;
            //    }
            //    _context.Entry(entity).State = EntityState.Added;
            //    _dbSet.Add(entity);
            //    await _context.SaveChangesAsync();
            //    // }
            //}
            //catch (Exception ex)
            //{
            //    LogError(ex, "INSERT ASYNC");
            //}
        }
        //private void DetachLocalEntityIfExists(DbContext context, int entityId)
        //{
        //    var localEntity = context.Set<TEntity>().Local.FirstOrDefault(e => e.Id == entityId);
        //    if (localEntity != null)
        //    {
        //        context.Entry(localEntity).State = EntityState.Detached;
        //    }
        //}
        private void LogError(Exception exception, string operation)
        {
            var errorMessage = $"Operation: {operation} Error: {exception.Message}{Environment.NewLine}{exception.InnerException}";
            // await _logger.LogDetailAsync(errorMessage).ConfigureAwait(false);
        }
        public void Update(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                using (var context = _contextFactory())
                {
                    var local = context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
                    if (local != null)
                    {
                        context.Entry(local).State = EntityState.Detached;
                    }
                    context.Set<TEntity>().AddOrUpdate(entity);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "UPDATE");
            }

            //if (entity == null) throw new ArgumentNullException(nameof(entity));
            //try
            //{
            //    //using (var context = _contextFactory())
            //    //{
            //    var local = _context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
            //    if (local != null)
            //    {
            //        _context.Entry(local).State = EntityState.Detached;
            //    }
            //    _context.Set<TEntity>().AddOrUpdate(entity);
            //    _context.SaveChanges();
            //    //var dbSet = context.Set<TEntity>();
            //    //DetachLocalEntityIfExists(context, entity.Id);
            //    //dbSet.AddOrUpdate(entity);
            //    //context.SaveChanges();
            //    // }
            //}
            //catch (Exception ex)
            //{
            //    LogError(ex, "UPDATE");
            //}
        }

        public async Task UpdateAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                using (var context = _contextFactory())
                {
                    var local = context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
                    if (local != null)
                    {
                        context.Entry(local).State = EntityState.Detached;
                    }
                    context.Set<TEntity>().AddOrUpdate(entity);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "UPDATE ASYNC");
            }

            //if (entity == null) throw new ArgumentNullException(nameof(entity));
            //try
            //{
            //    //using (var context = _contextFactory())
            //    //{
            //    var local = _context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == entity.Id);
            //    if (local != null)
            //    {
            //        _context.Entry(local).State = EntityState.Detached;
            //    }
            //    _context.Set<TEntity>().AddOrUpdate(entity);
            //    await _context.SaveChangesAsync();


            //}
            //catch (Exception ex)
            //{
            //    LogError(ex, "UPDATE ASYNC");
            //}
        }
        
 //var dbSet = context.Set<TEntity>();
                //DetachLocalEntityIfExists(context, entity.Id);
                //dbSet.AddOrUpdate(entity);
                //await context.SaveChangesAsync();
                //}
        //public void Delete(int id)
        //{
        //    try
        //    {
        //        using (var context = _contextFactory())
        //        {
        //            DetachLocalEntity(context, id);
        //           var entity =  context.Set<TEntity>().Find(id);
        //            if (entity == null)
        //            {
        //               // _logger.LogDetailAsync($"Delete Error. Entity with ID {id} not found.").SafeFireAndForget();
        //                return;
        //            }
        //            context.Set<TEntity>().Attach(entity);
        //            context.Set<TEntity>().Remove(entity);
        //            context.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex, "DELETE").SafeFireAndForget();
        //    }
        //}

        public void Delete(int id)
        {
            using (var context = _contextFactory())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        DetachLocalEntity(context, id);
                        var entity = context.Set<TEntity>().Find(id);
                        if (entity == null)
                        {
                            throw new KeyNotFoundException($"Entity with ID {id} not found.");
                        }
                        context.Set<TEntity>().Attach(entity);
                        context.Set<TEntity>().Remove(entity);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogError(ex, "DELETE");
                        throw;
                    }
                }
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var context = _contextFactory())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        DetachLocalEntity(context, id);
                        var entity = await context.Set<TEntity>().FindAsync(id);
                        if (entity == null)
                        {
                            return false;
                        }

                        context.Set<TEntity>().Attach(entity);
                        context.Set<TEntity>().Remove(entity);
                        await context.SaveChangesAsync();
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogError(ex, "DELETE ASYNC");
                        return false; // Or rethrow if you prefer exceptions
                    }
                }
            }
        }

        //public async Task DeleteAsync(int id)
        //{

        //    try
        //    {
        //        using (var context = _contextFactory())
        //        {
        //            DetachLocalEntity(context, id);
        //            var entity = context.Set<TEntity>().Find(id);
        //            if (entity == null)
        //            {
        //               // _logger.LogDetailAsync($"Delete Error. Entity with ID {id} not found.").SafeFireAndForget();
        //                return;
        //            }
        //            context.Set<TEntity>().Attach(entity);
        //            context.Set<TEntity>().Remove(entity);
        //            await context.SaveChangesAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex, "DELETE ASYNC");
        //    }
        //}

        public bool DeleteWithReturn(int id)
        {

            try
            {
                using (var context = _contextFactory())
                {
                    // Detach any existing tracked entity with the same key
                    DetachLocalEntity(context, id);
                    // Find the entity by its key
                    var entity = context.Set<TEntity>().Find(id);
                    if (entity == null) return false;
                    context.Set<TEntity>().Attach(entity);
                    // Remove the entity
                    context.Set<TEntity>().Remove(entity);
                    // Save changes asynchronously
                    return context.SaveChanges() > 0;
                }




                //using (var context = _contextFactory())
                //{
                //    DetachLocalEntity(context, id);
                //    var entity = FindByKey(id);
                //    if (entity == null)
                //    {
                //       // _logger.LogDetailAsync($"Delete Error. Entity with ID {id} not found.").SafeFireAndForget();
                //        return false;
                //    }

                //    _dbSet.Attach(entity);
                //    _dbSet.Remove(entity);
                //    context.SaveChanges();
                //    return await context.SaveChangesAsync() > 0;
                //}
            }
            catch (Exception ex)
            {
                LogError(ex, "DELETE ASYNC");
            }
            return false;
        }


        private void DetachLocalEntity(NeutronDb context, int id)
        {
            var localEntity = context.Set<TEntity>().Local.FirstOrDefault(f => f.Id == id);
            if (localEntity != null)
            {
                context.Entry(localEntity).State = EntityState.Detached;
            }
        }


    }
}
