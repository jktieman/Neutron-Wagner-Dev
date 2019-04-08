using System;
using System.Collections.Generic;
using System.Linq;
using Allied2Nova.Extensions;
using Allied2Nova.Models;

namespace Allied2Nova.Processors
{
    public class ReplenItemsProcessor
    {
        private IList<ReplenRec> replenItems;
       
        public ReplenItemsProcessor()
        {
            replenItems = new List<ReplenRec>();
            Init();
        }
        
        public IList<ReplenRec> ReplenItems
        {
            get { return replenItems; }
            set { replenItems = value; }
        }


        public void Init()
        {
            string[] lineDetail;

            string allRecs = SafeNativeMethods.GETDETAILRECS();

            foreach (var line in allRecs.SplitToLines().ToArray())
            {
                lineDetail = line.Split('|');
                switch (lineDetail[0])
                {
                    case "REPLENOPRP":
                        replenItems.Add(new ReplenRec(lineDetail[0], lineDetail[1], lineDetail[2], int.Parse(lineDetail[3])));
                        break;
                    default:
                        break;
                }
            }
        }

        public IList<ReplenRec> ReplenItemsSummary()
        {
            List<ReplenRec> summary = replenItems
                    .GroupBy(n => n.Sku)
                    .Select(s => new ReplenRec
                    {
                        Sku = s.First().Sku
                        ,
                        Qty = s.Sum(c => c.Qty)
                    }).ToList();
            return summary;
        }
    }
}
