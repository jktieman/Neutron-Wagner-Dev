using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Allied2Nova.Extensions;
using Allied2Nova.Models;

namespace Allied2Nova.Processors
{
    public class ReserveProcessor
    {
         private IList<ReserveRec> reserveRecords;

        public ReserveProcessor()
        {
            reserveRecords = new List<ReserveRec>();
            Init();
        }

        public IList<ReserveRec> ReserveRecords
        {
            get { return reserveRecords; }
            set { reserveRecords = value; }
        }

        public ReserveRec Get(string sku)
        {
            return reserveRecords.Where(c => c.Sku == sku).FirstOrDefault();
        }

        public ReserveRec GetFromNova(string sku)
        {
            ReserveRec def = new ReserveRec();
            var result = SafeNativeMethods.GETRESERVERECORD(ref sku);
            if (!string.IsNullOrWhiteSpace(result))
            {
                string[] values = result.Split('|');
                def.Sku = values[0];
                def.Slot = values[1];
                def.Qty = int.Parse(values[2]);
                return def;
            }

            return null;

        }

        public bool Save(ReserveRec reserveRec)
        {
            string rec = reserveRec.ToCsv();
            bool result = SafeNativeMethods.SAVERESERVERECORD(ref rec);

            return result;

        }

        public void Init()
        {
            try
            {
                string[] lineDetail;
                string allRecs = SafeNativeMethods.GETRESERVERECS();

                foreach (var line in allRecs.SplitToLines().ToArray())
                {
                    lineDetail = line.Split('|');
                    var c = new ReserveRec(lineDetail[0], lineDetail[1], int.Parse(lineDetail[2]));
                    reserveRecords.Add(c);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ERROR - Cannot initialize Nova Records. {0}", ex.Message));
            }

        }

        public IList<ReserveRec> ReserveRecordsSummary()
        {
            var summary = reserveRecords
                .GroupBy(n => n.Sku)
                .Select(s => new ReserveRec
                    {
                        Sku = s.First().Sku,
                        Slot = s.First().Slot,
                        Qty = s.Sum(c => c.Qty),
                    })
                .ToList();
            return summary;
        }
    }
}
