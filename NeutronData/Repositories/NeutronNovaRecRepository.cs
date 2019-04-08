using NeutronData.DataContexts;
using NeutronData.General;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace NeutronData.Repositories
{
    public class NeutronNovaRecRepository : RepositoryBase<NeutronDb>
    {
        public OperationStatus SaveAll(IList<NovaRec> recs)
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                using (var context = DataContext)
                {
                    foreach (var rec in recs)
                    {
                        context.NovaRecs.Add(rec);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of NovaRecs.", ex);
            }

            return opStatus;
        }

        public OperationStatus TruncateTable()
        {
            OperationStatus opStatus = new OperationStatus { Status = true };
            try
            {
                var t = new SqlParameter("@tableName", "NovaRecs");

                t.Direction = System.Data.ParameterDirection.Input;

                using (var context = DataContext)
                {
                    opStatus.RecordsAffected = context.Database
                    .ExecuteSqlCommand("exec TruncateTable @tableName", t);
                }
            }
            catch (Exception ex)
            {
                opStatus = OperationStatus.CreateFromException("Error Saving a list of NovaRecs.", ex);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public IList<NovaRec> GetSkuList(string sku)
        {
            IList<NovaRec> recs = new List<NovaRec>();
            using (var context = DataContext)
            {

                recs = context.NovaRecs.Where(n => n.Sku == sku).ToList();

            }
            return recs;
        }

        public IList<NovaRec> GetAll()
        {
            IList<NovaRec> recs = new List<NovaRec>();
            using (var context = DataContext)
            {
                recs = context.NovaRecs.ToList();
            }
            return recs;
        }

        public IList<TransferRec> GetTransferList(int station)
        {
            IList<TransferRec> recs = new List<TransferRec>();
            using (var context = DataContext)
            {
                recs = context.NovaRecs.Where(n => n.Station == station)
                .Select(r => new TransferRec
                {
                    Sku = r.Sku,
                    Des = r.Des,
                    Car = r.Car,
                    Bin = r.Bin,
                    Level = r.Level,
                    Part = r.Part,
                    Qty = r.Qty,
                    Station = r.Station
                })
                .OrderBy(o => o.Station).ThenBy(o => o.Car).ThenBy(o => o.Bin).ThenBy(o => o.Level).ThenBy(o => o.Part)
                .ToList();
            }
            return recs;
        }

        public int QuantityOnHand(string sku)
        {
            int result = 0;
            using (var context = DataContext)
            {
                var recs = context.NovaRecs.Where(n => n.Sku == sku).ToList();
                foreach (var rec in recs)
                {
                    result += rec.Qty;
                }
            }
            return result;
        }

        public IList<NovaRec> ClearInventory(string sku)
        {
            IList<NovaRec> result = new List<NovaRec>();
            try
            {
                var recs = GetSkuList(sku);
                foreach (var rec in recs)
                {
                    rec.Qty = 0;
                }
                result = recs;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Clearing Inventory.  " + e.Message);
            }
            return result;
        }
    }
}
