using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Repositories
{
    public class NeutronOCTypeRepository : RepositoryBase<NeutronDb>
    {
        public OperationStatus SaveAll(IList<OCType> recs)
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                using (var context = DataContext)
                {
                    foreach (var rec in recs)
                    {

                        context.OCTypes.Add(rec);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of OCTypes.", ex);
            }
            return opStatus;
        }

        public OperationStatus TruncateTable()
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                var t = new SqlParameter("@tableName", "OCTypes");

                t.Direction = System.Data.ParameterDirection.Input;

                using (var context = DataContext)
                {
                    opStatus.RecordsAffected = context.Database
                    .ExecuteSqlCommand("exec TruncateTable @tableName", t);
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of OCTypes.", ex);
            }
            return opStatus;
        }

        public OCType GetFirst()
        {
            return DataContext.OCTypes.FirstOrDefault();
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

        public virtual IList<OCType> GetSpecialList()
        {
            IList<OCType> result = new List<OCType>();
            try
            {
                using (var context = DataContext)
                {
                   result = context.Database.SqlQuery<OCType>("GetSpecialOCTypes").ToList();
                }
            }
            catch (Exception)
            {

            }
            return result;
        }
    }
}
