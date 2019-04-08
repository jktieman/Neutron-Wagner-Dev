using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace NeutronData.Repositories
{
    public class NeutronNLocDbRepository : RepositoryBase<NeutronDb>
    {

        public NLocDb GetBySlot(int station, int car, int bin, int level, int part)
        {
            using (var context = DataContext)
            {
                var rec = context.NLocDbs.Where(n => n.Station == station
                    && n.Car == car
                    && n.Bin == bin
                    && n.Level == level
                    && n.Part == part).FirstOrDefault();
                return rec;
            }
        }

        public OperationStatus SaveAll(IList<NLocDb> recs)
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                using (var context = DataContext)
                {
                    foreach (var rec in recs)
                    {
                        context.NLocDbs.Add(rec);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of NLocDBs.", ex);
            }
            return opStatus;
        }

        public OperationStatus TruncateTable()
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                var t = new SqlParameter("@tableName", "NLocDBs");

                t.Direction = System.Data.ParameterDirection.Input;

                using (var context = DataContext)
                {
                    opStatus.RecordsAffected = context.Database
                    .ExecuteSqlCommand("exec TruncateTable @tableName", t);
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of NLocDBs.", ex);
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
            catch (Exception e)
            {
                MessageBox.Show("SaveOperationStatus Error " + e.Message);
            }
        }

        public int Add(NLocDb loc)
        {
            int result = 0;
            try
            {
                using (var context = DataContext)
                {
                    var temp = context.NLocDbs.Add(loc);
                    var temp2 =context.SaveChanges();
                    result = 1;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("SaveOperationStatus Error " + e.Message + "\r\n" 
                    + e.InnerException.InnerException.Message + "\r\n");
            }
            return result;
        }
    }
}
