using AlliedLogger;
using Microsoft.Win32.SafeHandles;
using NeutronData.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace NeutronData.Repositories
{
    public class RepositoryBase<C> : IDisposable
        where C : DbContext, new()
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;
        
        // Instantiate a SafeHandle instance.
        readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //
            }

            disposed = true;
        }

        private C dataContext;

        public virtual C DataContext
        {
            get
            {
                //dataContext = null;
                if (dataContext == null)
                {
                    dataContext = new C();
                    AllowSerialization = true;
                }
                return dataContext;
            }
        }

        public virtual bool AllowSerialization
        {
            get
            {
                return dataContext.Configuration.ProxyCreationEnabled;
            }
            set
            {
                dataContext.Configuration.ProxyCreationEnabled = !value;
            }
        }

        public virtual T Get<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            if (predicate != null)
            {
                using (DataContext)
                {
                    return DataContext.Set<T>().Where(predicate).FirstOrDefault();
                }
            }
            else
            {
                throw new ApplicationException("Predicate value must be passed to Get<T>.");
            }
        }

        public virtual IQueryable<T> GetList<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                return DataContext.Set<T>().Where(predicate);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
            return null;
        }

        public virtual IQueryable<T> GetList<T, TKey>(Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderBy) where T : class
        {
            try
            {
                return GetList(predicate).OrderBy(orderBy);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
            return null;
        }

        public virtual IQueryable<T> GetList<T, TKey>(Expression<Func<T, TKey>> orderBy) where T : class
        {
            try
            {
                return GetList<T>().OrderBy(orderBy);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
            return null;
        }

        public virtual IQueryable<T> GetList<T>() where T : class
        {
            try
            {
                return DataContext.Set<T>();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
            return null;
        }

        public virtual int GetCount<T>() where T : class
        {
            try
            {
                return DataContext.Set<T>().Count();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
            return 0;
        }

        public virtual OperationStatus Save<T>(T entity) where T: class
        {
            var opStatus = new OperationStatus { Status = true };
            try
            {
                opStatus.Status = DataContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                opStatus = OperationStatus.CreateFromException("Error saving " + typeof(T) + ".", ex);
            }
            return opStatus;
        }

        public virtual OperationStatus Update<T>(T entity, params string[] propsToUpdate) where T: class
        {
            var opStatus = new OperationStatus { Status = true };

            try
            {
                System.Collections.Generic.IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> recs = DataContext.ChangeTracker.Entries();
                DataContext.Database.Log = Console.Write;
                DataContext.Set<T>().Attach(entity);
                //DataContext.Entry(entity).State = EntityState.Modified;
                opStatus.Status = DataContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                opStatus = OperationStatus.CreateFromException("Error updating " + typeof(T) + ".", ex);
            }
            return opStatus;
        }

        public OperationStatus ExecuteStoreCommand(string cmdText, params object[] parameters)
        {
            var opStatus = new OperationStatus { Status = true };

            try
            {
                opStatus.RecordsAffected = DataContext.Database.ExecuteSqlCommand(cmdText, parameters);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                opStatus = OperationStatus.CreateFromException("Error Executing Store Command.", ex);                
            }
            return opStatus;
        }

        public virtual OperationStatus Delete<T>(T entity) where T : class
        {
            var opStatus = new OperationStatus { Status = true };
            try
            {
                opStatus.Status = DataContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                opStatus = OperationStatus.CreateFromException("Error deleting " + typeof(T) + ".", ex);
            }
            return opStatus;
        }

        //public virtual void Dispose()
        //{
        //    MessageBox.Show("Disposing");
        //    if (DataContext != null) DataContext.Dispose();
        //}

        //protected void Dispose(bool disposing)
        //{
        //    if (disposing && (DataContext != null))
        //    {
        //        DataContext.Dispose();
        //    }
        //    Dispose(disposing);
        //}

        //public void Dispose()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
