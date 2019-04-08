using NeutronData.DataContexts;
using NeutronData.Migrations;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Repositories
{
    public class NeutronDetailRecRepository : RepositoryBase<NeutronDb>
    {
        public OperationStatus SaveAll(IList<DetailRec> recs)
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                using (var context = DataContext)
                {
                    foreach (var rec in recs)
                    {
                        context.DetailRecs.Add(rec);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of DetailRecs.", ex);
            }
            return opStatus;
        }

        public OperationStatus TruncateTable()
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                var t = new SqlParameter("@tableName", "DetailRecs");

                t.Direction = System.Data.ParameterDirection.Input;

                using (var context = DataContext)
                {
                    opStatus.RecordsAffected = context.Database
                    .ExecuteSqlCommand("exec TruncateTable @tableName", t);
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of DetailRecs.", ex);
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
