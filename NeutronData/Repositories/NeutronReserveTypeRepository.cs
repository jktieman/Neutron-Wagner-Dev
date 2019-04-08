using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace NeutronData.Repositories
{
    public class NeutronReserveTypeRepository : RepositoryBase<NeutronDb>
    {
        public OperationStatus SaveAll(IList<ReserveType> recs)
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                using (var context = DataContext)
                {
                    foreach (var rec in recs)
                    {
                        context.ReserveTypes.Add(rec);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of ReserveTypes.", ex);
            }
            return opStatus;
        }

        public OperationStatus TruncateTable()
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                var t = new SqlParameter("@tableName", "ReserveTypes");

                t.Direction = System.Data.ParameterDirection.Input;

                using (var context = DataContext)
                {
                    opStatus.RecordsAffected = context.Database
                    .ExecuteSqlCommand("exec TruncateTable @tableName", t);
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of ReserveTypes.", ex);
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

        public IList<ReserveType> CheckForInventory(ResDefType rec)
        {
            //is there any inventory in this slot
            // return a list
            IList<ReserveType> recs = new List<ReserveType>();
            try
            {
                using (var context = DataContext)
                {
                    recs = context.ReserveTypes.Where(r => r.Slot == rec.Slot).ToList();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("CheckForInventory Error. " + e.Message);
            }
            return recs;
        }
    }
}
