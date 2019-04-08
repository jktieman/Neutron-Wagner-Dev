using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Planning.Bindings;

namespace NeutronData.Repositories
{
    public class NeutronNDefDbRepository : RepositoryBase<NeutronDb>, INeutronNDefDbRepository
    {

        public OperationStatus SaveAll(IList<NDefDb> recs)
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                using (var context = DataContext)
                {
                    foreach (var rec in recs)
                    {
                        context.NDefDbs.Add(rec);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of NDefDbs.", ex);
            }
            return opStatus;
        }

        public OperationStatus TruncateTable()
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                var t = new SqlParameter("@tableName", "NDefDbs");

                t.Direction = System.Data.ParameterDirection.Input;

                using (var context = DataContext)
                {
                    opStatus.RecordsAffected = context.Database
                    .ExecuteSqlCommand("exec TruncateTable @tableName", t);
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of NDefDbs.", ex);
            }
            return opStatus;
        }

        public void SaveOperationStatus(OperationStatus opStatus)
        {
            try
            {
                using (var context = DataContext)
                {
                    context.OperationStatuses.Add(opStatus);
                    context.SaveChanges();
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
