
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NeutronData.ModelViews;
using NeutronData.PrintModels;


namespace NeutronDllu
{
    public partial class ShortReportProcessor
    {
        public ShortReportProcessor(BindingSource pickStops, LabelPrinterPreferences printer, bool printPreview = false)
        {

            DateTime date = DateTime.MinValue;
            var shortReports = new List<ShortReport>();
            foreach (PickStop stop in pickStops)
            {
                foreach (PickView pickView in stop.PickViews)
                {
                    PickLocation pickLocation = pickView.PickLocations.FirstOrDefault();
                    if (pickLocation != null)
                    {
                        date = pickLocation.PickDate;
                    }

                    var sr = new ShortReport()
                    {
                        Position = pickView.PickPosition,
                        Order = pickView.Ord1,
                        Invoice = pickView.Ord2,
                        Item = pickView.Item,
                        ReqQty = pickView.Quantity,
                        IssQty = pickView.PickedQty
                    };
                    if (sr.IssQty < sr.ReqQty)
                    {
                        shortReports.Add(sr);
                    }
                }
            }
            if (shortReports.Count > 0)
            {
                GroupAndPrint(shortReports, printer);
            }
        }

        public void GroupAndPrint(List<ShortReport> shortReports, LabelPrinterPreferences printer )
        {
           // var sb = new StringBuilder();
            //shortReports = shortReports.OrderBy(o => o.Order).ThenBy(o => o.Item).ToList();

            IEnumerable<IGrouping<string, ShortReport>> groups = shortReports.GroupBy(g => g.Order);

            foreach (var group in groups)
            {
                List<ShortReport> shorts = group.ToList();
                ShortReportToPrint.Print(shorts.ToList(), printer); 
            }
        }
    }
}
