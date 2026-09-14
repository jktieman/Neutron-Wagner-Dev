using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NeutronData.Interfaces;

namespace NeutronData.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        //IEnumerable<TEntity> All();

        IEnumerable<TEntity> All(
            Expression<Func<TEntity, bool>> filter = null,
            int? skip = null,
            int? take = null);
        
        //IEnumerable<TEntity> AllInclude(
        //    params Expression<Func<TEntity, object>>[] includeProperties);

        IEnumerable<TEntity> AllInclude(
            Expression<Func<TEntity, bool>> filter,
            params Expression<Func<TEntity, object>>[] includeProperties);
       // IQueryable<TEntity> GetAllIncluding(Expression<Func<TEntity, object>>[] includeProperties);

        IEnumerable<TEntity> FindByInclude(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity FindByKeyInclude(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity FindByKeyInclude(Expression<Func<TEntity, bool>> predicate,
            string[] stringIncludes,
            params Expression<Func<TEntity, object>>[] includeProperties);

        Task<TEntity> FindByKeyIncludeAsync(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties);
        Task<TEntity> FindByFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task InsertAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        //Task DeleteAsync(int id);
        Task<bool> DeleteAsync(int id);
        bool DeleteWithReturn(int id);
        Task<List<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate);

        IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate);
        TEntity FindByKey(int id);
        TEntity FindByKey(int? id);
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete(int id);
    }
}