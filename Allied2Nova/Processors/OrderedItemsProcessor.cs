using System;
using System.Collections.Generic;
using System.Linq;
using Allied2Nova.Extensions;
using Allied2Nova.Models;

namespace Allied2Nova.Processors
{
    public class OrderedItemsProcessor
    {

        private IList<DetailRec> orderedItems;

        public OrderedItemsProcessor()
        {
            orderedItems = new List<DetailRec>();
            Init();
        }

        public IList<DetailRec> OrderedItems
        {
            get { return orderedItems; }
            set { orderedItems = value; }
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
                        break;
                    default:
                        orderedItems.Add(new DetailRec(lineDetail[0], lineDetail[1], lineDetail[2], int.Parse(lineDetail[3])));
                        break;
                }
            }
        }

        public IList<DetailRec> OrderedItemsSummary()
        {

            List<DetailRec> summary = orderedItems
                .GroupBy(n => n.Sku)
                .Select(s => new DetailRec
                {
                    Sku = s.First().Sku
                    ,
                    Qty = s.Sum(c => c.Qty)
                }).ToList();
            return summary;
        }
    }
}
