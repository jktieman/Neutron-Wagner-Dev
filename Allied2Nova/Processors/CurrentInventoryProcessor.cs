using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Allied2Nova.Extensions;
using Allied2Nova.Models;

namespace Allied2Nova.Processors
{
    public class CurrentInventoryProcessor
    {
        private IList<NovaRec> novaRecords;

        public CurrentInventoryProcessor()
        {
            novaRecords = new List<NovaRec>();
            Init();
        }

        public IList<NovaRec> NovaRecords
        {
            get { return novaRecords; }
            set { novaRecords = value; }
        }

        public NovaRec Get(string sku)
        {
            return novaRecords.Where(c => c.Sku == sku).FirstOrDefault();
        }

        public NovaRec GetFromNova(string sku)
        {
            NovaRec def = new NovaRec();
            var result = SafeNativeMethods.GETNOVARECORD(ref sku);
            if (!string.IsNullOrWhiteSpace(result))
            {
                string[] values = result.Split('|');
                def.Sku = values[0];
                def.Des = values[1];
                def.Qty = int.Parse(values[2]);
                def.Station = int.Parse(values[3]);
                return def;
            }

            return null;

        }

        public bool Save(NovaRec novaRec)
        {
            string rec = novaRec.ToCsv();
            bool result = SafeNativeMethods.SAVENOVARECORD(ref rec);

            return result;

        }

        public void Init()
        {
            try
            {
                string[] lineDetail;
                string allRecs = SafeNativeMethods.GETNOVARECS();

                foreach (var line in allRecs.SplitToLines().ToArray())
                {
                    lineDetail = line.Split('|');
                    var c = new NovaRec(lineDetail[0], lineDetail[1], int.Parse(lineDetail[2]), int.Parse(lineDetail[3]));
                    novaRecords.Add(c);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ERROR - Cannot initialize Nova Records. {0}", ex.Message));
            }

        }

        public IList<NovaRec> NovaRecordsSummary()
        {
            var summary = novaRecords
                .GroupBy(n => n.Sku)
                .Select(s => new NovaRec
                    {
                        Sku = s.First().Sku,
                        Des = s.First().Des,
                        Qty = s.Sum(c => c.Qty),
                        Station = s.First().Station
                    })
                .ToList();
            return summary;
        }
    }
}
